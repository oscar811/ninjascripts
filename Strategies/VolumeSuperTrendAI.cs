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
	public class VolumeSuperTrendAI : Strategy
	{
        private static int K = 3;
        public static int N = 10;
        private int n = Math.Max(K, N);

        public int KNN_PriceLen = 20;
        public int KNN_STLen = 100;
        public bool aiSignals = true;

        private int length = 10;
        private int factor = 3;        

        private Series<double> UpperBand;
        private Series<double> LowerBand;
        private Series<double> SuperTrend;

        private Series<double?> label_;
        private Series<int> direction;
		
		private ATR atr;
		private VWMA vwma;
		
		private Series<double> price;
        private Series<double> sT;
		
		private bool countOnce = false;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volume SuperTrend";
				Name										= "VolumeSuperTrendAI";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 600;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				aiSignals = true;
				length  = 10;
				factor = 3;
				countOnce = true;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 3);

                UpperBand = new Series<double>(this);
				LowerBand = new Series<double>(this);
				SuperTrend = new Series<double>(this);
				
				label_ = new Series<double?>(this);
                direction = new Series<int>(this);				
            }
			else if (State == State.DataLoaded)
			{
				atr = ATR(length);
				vwma = VWMA(length);
				
				price = WMA(Close, KNN_PriceLen).Value;
            	sT = WMA(SuperTrend, KNN_STLen).Value;
				
				ClearOutputWindow(); //Clears Output window every time strategy is enabled
			}
		}

        // Main strategy logic
        protected override void OnBarUpdate()
        {
//			if (State != State.Realtime) //Only trades realtime. Ignores historical trades.			
//				return;
			if (State == State.Realtime && IsFirstTickOfBar)
            {
                return;  // Ignore intrabar ticks
            }
			
            if (BarsInProgress != 0)
                return;
			
			if (Bars.BarsSinceNewTradingDay < 1) //Needs more than 1 bar on new day to begin trading. (Prevents trades if previous day closed as a pattern for our entry)			
				return;			

            if (CurrentBar < 20) return;

			if (countOnce)
			{
				UpperBand[0] = vwma[0] + factor * atr[0];
	            LowerBand[0] = vwma[0] - factor * atr[0];

	            double prevLowerBand = LowerBand.Count > 0 ? LowerBand[1] : 0;
	            double prevUpperBand = UpperBand.Count > 0 ? UpperBand[1] : 0;

	            LowerBand[0] = LowerBand[0] > prevLowerBand || Close[1] < prevLowerBand ? LowerBand[0] : prevLowerBand;
	            UpperBand[0] = UpperBand[0] < prevUpperBand || Close[1] > prevUpperBand ? UpperBand[0] : prevUpperBand;
				
	            double prevSuperTrend = SuperTrend.Count > 0 ? SuperTrend[1] : 0;
				
				if (prevSuperTrend == prevUpperBand)
	                direction[0] = Close[0] > UpperBand[0] ? -1 : 1;
	            else
	                direction[0] = Close[0] < LowerBand[0] ? 1 : -1;

	            SuperTrend[0] = direction[0] == -1 ? LowerBand[0] : UpperBand[0];

	            List<double> data = new List<double>(n);
	            List<int> labels = new List<int>(n);
	            for (int i = 0; i < n; i++)
	            {
					data.Add(SuperTrend[i]);
					labels.Add(price[i] > sT[i] ? 1 : 0);
	            }
				
				Print(labels[0]);
				Print(labels[1]);

	//            Series<double> current_superTrend = SuperTrend;
	            label_[0] = KnnWeighted(data, labels, K, SuperTrend[0]);
				
	            bool Start_TrendUp = label_[0] == 1 && (label_[1] != 1 || label_[1] == null) && aiSignals;
	            bool Start_TrendDn = label_[0] == 0 && (label_[1] != 0 || label_[1] == null) && aiSignals;
				
				Print(label_[0]);
				Print(label_[1]);
				Print("Start_TrendUp: " + Start_TrendUp);
				Print("Start_TrendDn: " + Start_TrendDn);

	//            //TrendUp = direction == -1 and direction[1] == 1  and label_ == 1 and aisignals
	            bool TrendUp = direction[0] == -1 && direction[1] == 1 && label_[0] == 1 && aiSignals;
	            bool TrendDn = direction[0] == 1 && direction[1] == -1 && label_[0] == 0 && aiSignals;
				
				Print("TrendUp: " + TrendUp);
				Print("TrendDn: " + TrendDn);
				
	            //plotshape(Start_TrendUp ? superTrend : na, location = location.absolute, style = shape.circle, size = size.tiny, color = Bullish_col, title = "AI Bullish Trend Start")
	            //plotshape(Start_TrendDn ? superTrend : na, location = location.absolute, style = shape.circle, size = size.tiny, color = Bearish_col, title = "AI Bearish Trend Start")
	            //plotshape(TrendUp ? superTrend : na, location = location.absolute, style = shape.triangleup, size = size.small, color = Bullish_col, title = "AI Bullish Trend Signal")
	            //plotshape(TrendDn ? superTrend : na, location = location.absolute, style = shape.triangledown, size = size.small, color = Bearish_col, title = "AI Bearish Trend Signal")	

				if (Start_TrendUp || TrendUp)
					Draw.Text(this, "BuyLabel" + CurrentBar, "Buy", 0, Highs[0][0], Brushes.Green);
					if (Position.MarketPosition == MarketPosition.Short)
					{
						Print(Position.Quantity);
						ExitLong("Sell");
					}
		            else if (Position.MarketPosition == MarketPosition.Flat)
		                EnterLong("Buy");
	            else if (Start_TrendDn || TrendDn)
	                Draw.Text(this, "SellLabel" + CurrentBar, "Sell", 0, Lows[0][0], Brushes.Red);
	//				Print(Position.Quantity);				
					if (Position.MarketPosition == MarketPosition.Long)
						ExitLong("Buy");
		            else if (Position.MarketPosition == MarketPosition.Flat)
		                EnterLong("Sell");
					
				countOnce = false;
			}
			
			if (IsFirstTickOfBar)
			{
				countOnce = true;	
			}
            

        }

        private double KnnWeighted(List<double> data, List<int> labels, int k, double x)
        {
            int n1 = data.Count;
            List<double> distances = new List<double>(n1);
            List<int> indices = new List<int>(n1);

            // Compute distances from the current point to all other points
            for (int i = 0; i < n1; i++)
            {
                double x_i = data[i];
                double dist = Math.Abs(x - x_i);
                distances.Add(dist);
                indices.Add(i);
            }

            // Sort distances and corresponding indices in ascending order using LINQ
            var sortedDistances = distances.Select((value, index) => new { Value = value, Index = index })
                                  .OrderBy(pair => pair.Value)
                                  .ToList();
			var sortedIndices = indices.Select((value, index) => new { Value = value, Index = index })
                                  .OrderBy(pair => pair.Value)
                                  .ToList();

            // Compute weighted sum of labels of the k nearest neighbors
            double weightedSum = 0.0;
            double totalWeight = 0.0;
            for (int i = 0; i < k; i++)
            {
                int index = sortedIndices[i].Index;
                int label_i = labels[index];
                double weight_i = 1.0 / (sortedDistances[i].Value + 1e-6);
                weightedSum += weight_i * label_i;
                totalWeight += weight_i;
            }

            return Math.Round(weightedSum / totalWeight);
        }
    }
}
