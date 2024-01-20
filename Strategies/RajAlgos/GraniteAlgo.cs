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
using NinjaTrader.NinjaScript.Indicators.RajIndicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class GraniteAlgo : Strategy
    {       
        private SMA shortSMA;
        private SMA mediumSMA;
        private SMA longSMA;
        private RSI rsi;
        private Bollinger bb;
        private ATR atr;

        private Series<bool> smaBuyCondition;
        private Series<bool> smaSellCondition;
        private Series<bool> rsiBuyCondition;
        private Series<bool> rsiSellCondition;
        private Series<bool> adxBuyCondition;
        private Series<bool> adxSellCondition;

        private Series<bool> buyCondition;
        private Series<bool> sellCondition;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"GraniteAlgo";
                Name = "GraniteAlgo";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 1;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 10;

                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = false;

                //EnableAtm = false;
                //AtmStrategyTemplateId = "your atm";

                enableSmaFilter = true;
                shortSmaPeriod = 7;
                mediumSmaPeriod = 14;
                longSmaPeriod = 21;

                enableAdxFilter = false;
                adxFilterFrom = 15;
                adxFilterTo = 50;

                enableBBFilter = false;
                bbLength = 14;
                bbStdDev = 1;

                enableRsiFilter = true;
                rsiPeriod = 14;
                rsiOverbought = 70;
                rsiOversold = 30;

                enablePercentTpSl = false;
                tpPercent = 0.8;
                slPercent = 0.3;

                enableAtrTpSl = true;
                atrPeriod = 14;
                atrMultiplierForTakeProfit = 4;
                atrMultiplierForStopLoss = 2;

                enableSmaTpSl = false;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                shortSMA = SMA(shortSmaPeriod);
                mediumSMA = SMA(mediumSmaPeriod);
                longSMA = SMA(longSmaPeriod);

                // adx
                // bb

                rsi = RSI(rsiPeriod, 3);
                atr = ATR(atrPeriod);

                smaBuyCondition = new Series<bool>(this);
                smaSellCondition = new Series<bool>(this);
                rsiBuyCondition = new Series<bool>(this);
                rsiSellCondition = new Series<bool>(this);
                adxBuyCondition = new Series<bool>(this);
                adxSellCondition = new Series<bool>(this);

                buyCondition = new Series<bool>(this);
                sellCondition = new Series<bool>(this);

                if (enableSmaFilter)
                {
                    shortSMA.Plots[0].Brush = Brushes.Red;
                    mediumSMA.Plots[0].Brush = Brushes.Green;
                    longSMA.Plots[0].Brush = Brushes.Yellow;
                    AddChartIndicator(shortSMA);
                    AddChartIndicator(mediumSMA);
                    AddChartIndicator(longSMA);
                }
				
				if (enableRsiFilter)
				{
					AddChartIndicator(rsi);
				}
            }
            else if (State == State.DataLoaded)
            {
                // sessionLevels = SessionLevels(Close, true, true, true, true, DateTime.Parse("6:00 PM"), DateTime.Parse("11:59 PM"), true, DateTime.Parse("12:00 AM"), DateTime.Parse("6:00 AM"), true, DateTime.Parse("6:00 AM"), DateTime.Parse("12:00 PM"), true, DateTime.Parse("12:00 PM"), DateTime.Parse("6:00 PM"));

                // SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                // SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                // Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                // Print("Time[0]: " + Time[0].ToString());
                // Print("CurrentBar: " + CurrentBar);

                // [_, _, adxValue] = ta.dmi(14, 14)
                // adxCondtion = adxValue > adxFilterFrom and adxValue < adxFilterTo

                smaBuyCondition[0] = shortSMA[0] > mediumSMA[0] && shortSMA[0] > longSMA[0];
                // bbBuyCondition[0] = close > bbUpper
                rsiBuyCondition[0] = (rsiBuyCondition[1] || rsi[0] < rsiOversold) && rsi[0] < rsiOverbought;
                buyCondition[0] = (!enableSmaFilter || smaBuyCondition[0]) && (!enableRsiFilter || rsiBuyCondition[0]) && (!enableAdxFilter) && (!enableBBFilter);

                smaSellCondition[0] = shortSMA[0] < mediumSMA[0] && shortSMA[0] < longSMA[0];
                rsiSellCondition[0] = (rsiSellCondition[1] || rsi[0] > rsiOverbought) && rsi[0] > rsiOversold;
                // bbSellCondition[0] = close < bbLower
                sellCondition[0] = (!enableSmaFilter || smaSellCondition[0]) && (!enableRsiFilter || rsiSellCondition[0]) && (!enableAdxFilter) && (!enableBBFilter);

                if (buyCondition[0] && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atr[0];
                    SetStopLoss("", CalculationMode.Price, Close[0] - atrMultiplierForStopLoss * atrValue, false);
                    SetProfitTarget("", CalculationMode.Price, Close[0] + atrMultiplierForTakeProfit * atrValue);
                    EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Long");
                }

                if (sellCondition[0] && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atr[0];
                    SetStopLoss("", CalculationMode.Price, Close[0] + atrMultiplierForStopLoss * atrValue, false);
                    SetProfitTarget("", CalculationMode.Price, Close[0] - atrMultiplierForTakeProfit * atrValue);
                    EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Short");
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties

        // ATM properties
        // [NinjaScriptProperty]
        // [Display(Name = "Enable Atm", Order = 2, GroupName = "ATM")]
        // public bool EnableAtm
        // { get; set; }

        // [NinjaScriptProperty]
        // [Display(Name = "ATM Strategy (Only real time)", Order = 3, GroupName = "ATM")]
        // public string AtmStrategyTemplateId
        // { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable SMA Filter", Order = 3, GroupName = "SMA")]
        public bool enableSmaFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Short Sma Period", Order = 3, GroupName = "SMA")]
        public int shortSmaPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Medium Sma Period", Order = 3, GroupName = "SMA")]
        public int mediumSmaPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Long Sma Period", Order = 3, GroupName = "SMA")]
        public int longSmaPeriod { get; set; }

        private bool enableAdxFilter = true;
        private int adxFilterFrom = 15;
        private int adxFilterTo = 50;

        private bool enableBBFilter = true;
        private int bbLength = 14;
        private int bbStdDev = 1;

        [NinjaScriptProperty]
        [Display(Name = "Enable RSI Filter", Order = 3, GroupName = "RSI")]
        public bool enableRsiFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "RSI Period", Order = 3, GroupName = "RSI")]
        public int rsiPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "RSI Overbought", Order = 3, GroupName = "RSI")]
        public int rsiOverbought { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "RSI Oversold", Order = 3, GroupName = "RSI")]
        public int rsiOversold { get; set; }

        private bool enablePercentTpSl = false;
        private double tpPercent = 0.8;
        private double slPercent = 0.3;

        //        private int atrMaLength = 26;
        //        private double tpAtr = 4;
        //        private double slAtr = 2;

        private bool enableSmaTpSl = false;

        [NinjaScriptProperty]
        [Display(Name = "Enable ATR Tp/Sl", Order = 3, GroupName = "ATM")]
        public bool enableAtrTpSl { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr Period", Order = 3, GroupName = "ATM")]
        public int atrPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for SL", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForStopLoss { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for TP", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForTakeProfit { get; set; }

        //[NinjaScriptProperty]
        //[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        //[Display(Name = "Asian Start", Order = 1, GroupName = "Time")]
        //public DateTime Start_Time_1
        //{ get; set; }

        //[NinjaScriptProperty]
        //[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        //[Display(Name = "Asian End", Order = 2, GroupName = "Time")]
        //public DateTime Stop_Time_1
        //{ get; set; }

        #endregion
    }
}
