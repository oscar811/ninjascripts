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
        private bool enableSmaFilter = true;
        private int shortSmaPeriod = 7;
        private int mediumSmaPeriod = 14;
        private int longSmaPeriod = 21;

        private bool enableAdxFilter = true;
        private int adxFilterFrom = 15;
        private int adxFilterTo = 50;

        private bool enableBBFilter = true;
        private int bbLength = 14;
        private int bbStdDev = 1;

        private bool enableRsiFilter = false;
        private int rsiPeriod = 14;
        private int rsiOverbought = 70;
        private int rsiOversold = 30;

        private bool enablePercentTpSl = false;
        private double tpPercent = 0.8;
        private double slPercent = 0.3;

        private double enableAtrTpSl = true;
        private int atrMaLength = 26;
        private double tpAtr = 4;
        private double slAtr = 2;

        private bool enableSmaTpSl = false;

        private SMA shortSMA;
        private SMA mediumSMA;
        private SMA longSMA;
        private RSI rsi;
        private BB bb;

        private Series<bool> smaBuyCondition;
        private Series<bool> smaSellCondition;
        private Series<bool> rsiBuyCondition;
        private Series<bool> rsiSellCondition;
        private Series<bool> adxBuyCondition;
        private Series<bool> adxSellCondition;

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

                EnableAtm = false;
                AtmStrategyTemplateId = "your atm";

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
                atrMaLength = 26;
                tpAtr = 4;
                slAtr = 2;

                enableSmaTpSl = false;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                shortSMA = new SMA(shortSmaPeriod);
                mediumSMA = new SMA(mediumSmaPeriod);
                longSMA = new SMA(longSmaPeriod);

                // adx
                // bb

                rsi = new RSI(rsiPeriod);

                AddPlot(Brushes.Red, "shortSMA");
                AddPlot(Brushes.Green, "mediumSMA");
                AddPlot(Brushes.Yello, "longSMA");
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

                smaSellCondition[0] = shortSMA < mediumSMA && shortSMA < longSMA;
                rsiSellCondition[0] = (rsiSellCondition[1] || rsi[0] > rsiOverbought) && rsi[0] > rsiOversold;
                // bbSellCondition[0] = close < bbLower
                sellCondition[0] = (!enableSmaFilter || smaSellCondition[0]) && (!enableRsiFilter || rsiSellCondition[0]) && (!enableAdxFilter) && (!enableBBFilter)

                if (buyCondition[0] && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atrIndicator[0];
                    SetStopLoss("", CalculationMode.Price, Close[0] - atrMultiplierForStopLoss * atrValue, false);
                    SetProfitTarget("", CalculationMode.Price, Close[0] + atrMultiplierForTakeProfit * atrValue);
                    EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Long");
                }

                if (sellCondition[0] && Position.MarketPosition == MarketPosition.Flat)
                {
                    double atrValue = atrIndicator[0];
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
        [Display(Name = "Atr Period", Order = 3, GroupName = "ATM")]
        public int atrPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for SL", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForStopLoss { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for TP", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForTakeProfit { get; set; }


        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asian Start", Order = 1, GroupName = "Time")]
        public DateTime Start_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asian End", Order = 2, GroupName = "Time")]
        public DateTime Stop_Time_1
        { get; set; }

        #endregion
    }
}
