#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class VolSuperTrendAI2 : Strategy
    {
        // ~~ Input settings for K && N values;
        private int k = 3; // Neighbors;
        private int n_ = 10; // Data
        private int n = 0;

        // ~~ Input settings for prediction values;
        private int KNN_PriceLen = 20; // Price Trend
        private int KNN_STLen = 100; // Prediction Trend
        private bool aisignals = true;

        // ~~ Define SuperTrend parameters;
        private int len = 10;
        private double factor = 3.0;

        private ATR atr;
        private WMA price;
        private WMA sT;
        private EMA ema;

        private Series<double> upperBand;
        private Series<double> lowerBand;
        private Series<double> superTrend;

        private Series<double?> label_;
        private Series<int> direction;

        private bool countOnce = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "VolSuperTrendAI2";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                n = Math.Max(k, n_);
                countOnce = true;
            }
            else if (State == State.Configure)
            {
                upperBand = new Series<double>(this);
                lowerBand = new Series<double>(this);
                superTrend = new Series<double>(this);

                label_ = new Series<double?>(this);
                direction = new Series<int>(this);

            }
            else if (State == State.DataLoaded)
            {
                atr = ATR(len);
                price = WMA(Close, KNN_PriceLen);
                sT = WMA(superTrend, KNN_STLen);
                ema = EMA(Close, 100);

                ClearOutputWindow(); //Clears Output window every time strategy is enabled                
            }
        }

        private double VWMA(int len)
        {
            double weightedCloseVolumeSum = 0.0;
            double weightedVolumeSum = 0.0;
            double divisor = 0.0;

            for (int i = 0; i < len; i++)
            {
                double weight = len - i;
                weightedCloseVolumeSum += Close[i] * Volume[i] * weight;
                weightedVolumeSum += Volume[i] * weight;
                divisor += weight;
            }

            return weightedCloseVolumeSum / weightedVolumeSum;
        }

        // ~~ Define the weighted k-nearest neighbors (KNN) function;
        private double knn_weighted(List<double> data, List<int> labels, int k, double x)
        {
            int n1 = data.Count;
            // distances = array.new_float(n1) ;
            // indices   = array.new_int(n1) ;
            List<double> distances = new List<double>(n1);
            List<int> indices = new List<int>(n1);

            // Compute distances from the current point to all other points;
            for (int i = 0; i < n1; i++)
            {
                // x_i = data.get(i);
                // dist = distance(x, x_i) ;
                double dist = Math.Abs(x - data[i]);
                // distances.set(i, dist);
                distances.Add(dist);
                // indices.set(i, i);
                indices.Add(i);
            }

            // Sort distances && corresponding indices in ascending order
            // Bubble sort method
            for (int i = 0; i < n1 - 1; i++)
            {
                for (int j = 0; j < n1 - i - 1; j++)
                {					
                    if (distances[j] > distances[j + 1])
                    {
                        // Swap distances
                        double tempDist = distances[j];
                        distances[j] = distances[j + 1];
                        distances[j + 1] = tempDist;

                        // Swap corresponding indices
                        int tempIndex = indices[j];
                        indices[j] = indices[j + 1];
                        indices[j + 1] = tempIndex;
                    }
                }
            }

            // Compute weighted sum of labels of the k nearest neighbors
            double weighted_sum = 0.0;
            double total_weight = 0.0;
            for (int i = 0; i < k; i++)
            {
                // index = indices.get(i);
                int label_i = labels[indices[i]];
                double weight_i = 1 / (distances[0] + 1e-6);
                weighted_sum += weight_i * label_i;
                total_weight += weight_i;
            }

            return weighted_sum / total_weight;
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (State == State.Realtime && IsFirstTickOfBar)
                    return;  // Ignore intrabar ticks            

                if (BarsInProgress != 0)
                    return;

                if (Bars.BarsSinceNewTradingDay < 1) //Needs more than 1 bar on new day to begin trading. (Prevents trades if previous day closed as a pattern for our entry)			
                    return;

                if (CurrentBar < 20)
                    return;

                if (countOnce)
                {
                    //Add your custom strategy logic here.
                    double vwma = VWMA(len);
                    upperBand[0] = vwma + factor * atr[0];
                    lowerBand[0] = vwma - factor * atr[0];
                    double prevLowerBand = lowerBand[1];
                    double prevUpperBand = upperBand[1];

                    lowerBand[0] = (lowerBand[0] > prevLowerBand || Close[1] < prevLowerBand) ? lowerBand[0] : prevLowerBand;
                    upperBand[0] = (upperBand[0] < prevUpperBand || Close[1] > prevUpperBand) ? upperBand[0] : prevUpperBand;
                    //int direction = 0;

                    double prevSuperTrend = superTrend.Count > 0 ? superTrend[1] : 0;
                    direction[0] = 1;
                    if (prevSuperTrend == prevUpperBand)
                        direction[0] = Close[0] > upperBand[0] ? -1 : 1;
                    else
                        direction[0] = Close[0] < lowerBand[0] ? 1 : -1;
                    superTrend[0] = direction[0] == -1 ? lowerBand[0] : upperBand[0];

                    // ~~ Collect data points && their corresponding labels;
                    //double price = ta.wma(Close, KNN_PriceLen);
                    //double sT = ta.wma(superTrend, KNN_STLen);

                    // data   = array.new_float(n);
                    // labels = array.new_int(n);
                    List<double> data = new List<double>(n);
                    List<int> labels = new List<int>(n);

                    for (int i = 0; i < n; i++)
                    {
                        data.Add(superTrend[i]);
                        // data.set(i, superTrend[i]) ;
                        int label_i = price[i] > sT[i] ? 1 : 0;
                        // labels.set(i, label_i);
                        labels.Add(label_i);
                    }

                    // ~~ Classify the current data point;
                    // current_superTrend = superTrend;
                    label_[0] = knn_weighted(data, labels, k, superTrend[0]);

                    // ~~ Plot;
                    // col[0] = label_[0] == 1?upCol:label_[0] == 0?dnCol:neCol;
                    // plot(current_superTrend, color=col, title="Volume Super Trend AI");
                    // ;
                    // upTrend   = plot(superTrend==lowerBand?current_superTrend:na, title="Up Volume Super Trend AI", color=col, style=plot.style_linebr);
                    // Middle    = plot((open + Close) / 2, display=display.none, editable=false);
                    // downTrend = plot(superTrend==upperBand?current_superTrend:na, title="Down Volume Super Trend AI", color=col, style=plot.style_linebr);
                    // fill_col  = color.new(col,90);
                    // fill(Middle, upTrend, fill_col, fillgaps=false,title="Up Volume Super Trend AI");
                    // fill(Middle, downTrend, fill_col, fillgaps=false, title="Down Volume Super Trend AI");
                    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~};

                    // ~~ Ai Super Trend Signals;
                    bool Start_TrendUp = label_[0] == 1 && (label_[1] != 1 || label_[1] == null) && aisignals;
                    bool Start_TrendDn = label_[0] == 0 && (label_[1] != 0 || label_[1] == null) && aisignals;
                    bool TrendUp = direction[0] == -1 && direction[1] == 1 && label_[0] == 1 && aisignals;
                    bool TrendDn = direction[0] == 1 && direction[1] == -1 && label_[0] == 0 && aisignals;

                    // plotshape(Start_TrendUp?superTrend:na, location=location.absolute, style= shape.circle, size=size.tiny, color=Bullish_col, title="AI Bullish Trend Start");
                    // plotshape(Start_TrendDn?superTrend:na, location=location.absolute, style= shape.circle,size=size.tiny, color=Bearish_col, title="AI Bearish Trend Start");
                    // plotshape(TrendUp?superTrend:na, location=location.absolute, style= shape.triangleup, size=size.small, color=Bullish_col, title="AI Bullish Trend Signal");
                    // plotshape(TrendDn?superTrend:na, location=location.absolute, style= shape.triangledown,size=size.small, color=Bearish_col, title="AI Bearish Trend Signal");

                    //ema = ta.ema(Close, 100);

                    if ((Start_TrendUp || TrendUp) && Close[0] > ema[0])
                        EnterLong("Buy");

                    if ((Start_TrendDn || TrendDn) && Close[0] < ema[0])
                        EnterShort("Sell");

                    // countOnce = false;
                }

                if (IsFirstTickOfBar)
                {
                    countOnce = true;
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }
    }
}
