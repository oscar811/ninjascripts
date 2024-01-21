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
        private RSI rsiHtf;
        private MACD macd;
        private ADX adx;
        private Bollinger bb;
        private ATR atr;

        //        private Series<bool> smaBuyCondition;
        //        private Series<bool> smaSellCondition;
        private Series<bool> rsiBuyCondition;
        private Series<bool> rsiSellCondition;
        //private Series<bool> adxBuyCondition;
        //private Series<bool> adxSellCondition;

        //        private Series<bool> buyCondition;
        //        private Series<bool> sellCondition;

        private Series<bool> t_trade;

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
                BarsRequiredToTrade = 20;

                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                //EnableAtm = false;
                //AtmStrategyTemplateId = "your atm";
                trade_start = 100000;
                trade_end = 160000;

                enableSmaFilter = true;
                shortSmaPeriod = 7;
                mediumSmaPeriod = 14;
                longSmaPeriod = 21;

                enableAdxFilter = true;
                adxPeriod = 14;
                adxFilterFrom = 15;
                adxFilterTo = 50;

                enableBBFilter = true;
                bbLength = 14;
                bbStdDev = 0.5;

                enableRsiFilter = false;
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
                //                ClearOutputWindow();
                //AddDataSeries("VIX", Data.BarsPeriodType.Minute, 1);
                AddDataSeries(BarsPeriodType.Minute, 15);

                rsiBuyCondition = new Series<bool>(this);
                rsiSellCondition = new Series<bool>(this);

                t_trade = new Series<bool>(this);
            }
            else if (State == State.DataLoaded)
            {
                shortSMA = SMA(shortSmaPeriod);
                mediumSMA = SMA(mediumSmaPeriod);
                longSMA = SMA(longSmaPeriod);

                if (enableSmaFilter)
                {
                    shortSMA.Plots[0].Brush = Brushes.Red;
                    mediumSMA.Plots[0].Brush = Brushes.Green;
                    longSMA.Plots[0].Brush = Brushes.Yellow;
                    AddChartIndicator(shortSMA);
                    AddChartIndicator(mediumSMA);
                    AddChartIndicator(longSMA);
                }

                adx = ADX(adxPeriod);
                if (enableAdxFilter)
                    AddChartIndicator(adx);

                bb = Bollinger(bbStdDev, bbLength);
                if (enableBBFilter)
                    AddChartIndicator(bb);

                rsi = RSI(Closes[0], rsiPeriod, 3);
                rsiHtf = RSI(Closes[1], rsiPeriod, 3);

                if (enableRsiFilter)
                {
                    AddChartIndicator(rsi);
                    AddChartIndicator(rsiHtf);
                }

                atr = ATR(atrPeriod);

                // sessionLevels = SessionLevels(Close, true, true, true, true, DateTime.Parse("6:00 PM"), DateTime.Parse("11:59 PM"), true, DateTime.Parse("12:00 AM"), DateTime.Parse("6:00 AM"), true, DateTime.Parse("6:00 AM"), DateTime.Parse("12:00 PM"), true, DateTime.Parse("12:00 PM"), DateTime.Parse("6:00 PM"));

                // SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                // SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade || CurrentBar < longSmaPeriod || CurrentBar < rsiPeriod || CurrentBar < adxPeriod)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                t_trade[0] = check_time(trade_start, trade_end);

                //                Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                //                Print("Time[0]: " + Time[0].ToString());
                                Print("CurrentBar: " + CurrentBar);
                Print("rsi[0]: " + rsi[0]);
                Print("rsiHtf[0]: " + rsiHtf[0]);
                //				Print("adxPeriod: " + adxPeriod);
                //				Print("adx[0]: " + adx[0]);

                bool adxCondtion = adx[0] > adxFilterFrom && adx[0] < adxFilterTo;

                bool smaBuyCondition = shortSMA[0] > mediumSMA[0] && mediumSMA[0] > longSMA[0];
                bool bbBuyCondition = Close[0] > bb.Upper[0];
                rsiBuyCondition[0] = (rsiBuyCondition[1] || rsiHtf[0] < rsiOversold) && rsiHtf[0] < rsiOverbought;
                if (rsiHtf[0] > rsiOverbought) rsiBuyCondition[0] = false;

                bool buyCondition = (!enableSmaFilter || smaBuyCondition) && (!enableRsiFilter || rsiBuyCondition[0])
                    && (!enableAdxFilter || adxCondtion) && (!enableBBFilter || bbBuyCondition);

                bool smaSellCondition = shortSMA[0] < mediumSMA[0] && mediumSMA[0] < longSMA[0];
                bool bbSellCondition = Close[0] < bb.Lower[0];
                rsiSellCondition[0] = (rsiSellCondition[1] || rsiHtf[0] > rsiOverbought) && rsiHtf[0] > rsiOversold;
                if (rsiHtf[0] < rsiOversold) rsiSellCondition[0] = false;

                //				Print("adx[0]: " + adx[0]);
                //				Print("rsiSellCondition[0]: " + rsiSellCondition[0]);

                bool sellCondition = (!enableSmaFilter || smaSellCondition) && (!enableRsiFilter || rsiSellCondition[0])
                    && (!enableAdxFilter || adxCondtion) && (!enableBBFilter || bbSellCondition);

                if (t_trade[0] && buyCondition && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atr[0];
                    SetStopLoss("", CalculationMode.Price, Close[0] - atrMultiplierForStopLoss * atrValue, false);
                    SetProfitTarget("", CalculationMode.Price, Close[0] + atrMultiplierForTakeProfit * atrValue);
                    EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Long");
                }

                if (t_trade[0] && sellCondition && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atr[0];
                    SetStopLoss("", CalculationMode.Price, Close[0] + atrMultiplierForStopLoss * atrValue, false);
                    SetProfitTarget("", CalculationMode.Price, Close[0] - atrMultiplierForTakeProfit * atrValue);
                    EnterShort(DefaultQuantity, Convert.ToString(CurrentBar) + " Short");
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private bool check_time(int T1, int T2)
        {
            bool result = false;
            int T = ToTime(Time[0]); // ex. 080000
            if (T1 > T2)
            {
                result = T >= T1 || T <= T2; // ex. T1 = 220000, T2 = 020000
            }
            else
            {
                result = T >= T1 && T <= T2;
            }
            return result;
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
        [Display(Name = "Trade Window Start", Order = 5, GroupName = "Time")]
        public int trade_start { set; get; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Window End", Order = 6, GroupName = "Time")]
        public int trade_end { set; get; }

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

        [NinjaScriptProperty]
        [Display(Name = "Enable Adx Filter", Order = 3, GroupName = "ADX")]
        public bool enableAdxFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Adx Period", Order = 3, GroupName = "ADX")]
        public int adxPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Adx From", Order = 3, GroupName = "ADX")]
        public int adxFilterFrom { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Adx To", Order = 3, GroupName = "ADX")]
        public int adxFilterTo { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Bollinger", Order = 3, GroupName = "BB")]
        public bool enableBBFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bollinger Length", Order = 3, GroupName = "BB")]
        public int bbLength { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bollinger Std. Dev.", Order = 3, GroupName = "BB")]
        public double bbStdDev { get; set; }

        private bool enablePercentTpSl = false;
        private double tpPercent = 0.8;
        private double slPercent = 0.3;

        //        private int atrMaLength = 26;
        //        private double tpAtr = 4;
        //        private double slAtr = 2;

        private bool enableSmaTpSl = false;

        [NinjaScriptProperty]
        [Display(Name = "Enable Atr Tp/Sl", Order = 3, GroupName = "Risk")]
        public bool enableAtrTpSl { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr Period", Order = 3, GroupName = "Risk")]
        public int atrPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for SL", Order = 3, GroupName = "Risk")]
        public double atrMultiplierForStopLoss { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for TP", Order = 3, GroupName = "Risk")]
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
