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
    public class PossibleAgain : Strategy
    {
        private SwingRays2c ltfSwingRays;
        private SwingRays2c htfSwingRays;

        private Series<double> htfHighSweep;
        private Series<double> htfLowSweep;

        private EMA emaEntry;
        private EMA emaShort;
        private EMA emaLong;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"PossibleAgain";
                Name = "PossibleAgain";
                Calculate = Calculate.OnEachTick;
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

                HtfTimeFrame = 5;
                Strength = 4;
                EmaEntryPeriod = 100;
                EnableEmaEntry = true;
                EmaShortPeriod = 20;
                EmaLongPeriod = 50;

                TakeProfit = 300;
                StopLoss = 70;
                KeepBrokenLines = true; // defaulted to false to reduce overhead

                HtfSwingColor = Brushes.DodgerBlue;
                LtfSwingColor = Brushes.Fuchsia;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Minute, HtfTimeFrame);

                htfHighSweep = new Series<double>(this);
                htfLowSweep = new Series<double>(this);

                SetStopLoss(CalculationMode.Ticks, StopLoss);
                SetProfitTarget(CalculationMode.Ticks, TakeProfit);

                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                emaEntry = EMA(EmaEntryPeriod);
                AddChartIndicator(emaEntry);

                emaShort = EMA(EmaShortPeriod);
                emaShort.Plots[0].Brush = Brushes.HotPink;
                emaLong = EMA(EmaLongPeriod);
                //AddChartIndicator(emaShort);
                //AddChartIndicator(emaLong);

                ltfSwingRays = SwingRays2c(Closes[0], Strength, 1, KeepBrokenLines, 1);
                ltfSwingRays.SwingHighColor = LtfSwingColor;
                ltfSwingRays.SwingLowColor = LtfSwingColor;

                htfSwingRays = SwingRays2c(Closes[1], Strength, 0, KeepBrokenLines, 1);
                htfSwingRays.SwingHighColor = HtfSwingColor;
                htfSwingRays.SwingLowColor = HtfSwingColor;

                AddChartIndicator(ltfSwingRays);
                AddChartIndicator(htfSwingRays);
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

                //Print("Time inside strat: " + Time[0]);
                //Print("total high swings: " + swingRays2c.SwingHighRays.Count);
                //Print("total low swings: " + swingRays2c.SwingLowRays.Count);

                //Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                //Print("CurrentBar: " + CurrentBar);
                //Print("Time[0]: " + Time[0].ToString());

                // check for 5 min high sweep
                // check for 5 min low broken(not sweep)
                // check for 1 min high sweep
                // confirm trend (50 < 200)
                // enter short
                // wait till 5 min low broken

                // after 5 min sweep, store mss

                htfHighSweep[0] = htfHighSweep[1] == 1 || htfSwingRays.IsHighBroken[0] == 1 ? 1 : 0;
                //if (htfLowSweep[0] == 1) htfHighSweep[0] = 0;
                //if (htfHighSweep[0] == 1) Print("htfHighSweep[0]: " + htfHighSweep[0]);

                htfLowSweep[0] = htfLowSweep[1] == 1 || htfSwingRays.IsLowBroken[0] == 1 ? 1 : 0;
                //if (htfHighSweep[0] == 1) htfLowSweep[0] = 0;
                //if (htfLowSweep[0] == 1) Print("htfLowSweep[0]: " + htfLowSweep[0]);

                if (htfLowSweep[0] == 1 && EnableEmaEntry && High[0] > emaEntry[0] && ltfSwingRays.IsLowBroken[0] == 1)
                {
                    EnterLong();
                }
                else if (htfHighSweep[0] == 1 && EnableEmaEntry && Low[0] < emaEntry[0] && ltfSwingRays.IsHighBroken[0] == 1)
                {
                    EnterShort();
                }

                if (CrossBelow(emaShort, emaLong, 2)) // start of downtrend, exit longs
                {
                    htfLowSweep[0] = 0;
                    if (Position.MarketPosition == MarketPosition.Long) ExitLong();
                }
                else if (CrossAbove(emaShort, emaLong, 2)) // start of uptrend, exit shorts
                {
                    htfHighSweep[0] = 0;
                    if (Position.MarketPosition == MarketPosition.Short) ExitShort();
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Take Profit (ticks)", Description = "", Order = 1, GroupName = "ATM")]
        public int TakeProfit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Stop Loss (ticks)", Description = "", Order = 1, GroupName = "ATM")]
        public int StopLoss
        { get; set; }

        [Display(Name = "Htf Timeframe (mins)", Order = 1, GroupName = "Strategy")]
        public int HtfTimeFrame
        { get; set; }

        [Range(2, int.MaxValue)]
        [Display(Name = "Swing Strength", Description = "Number of bars before/after each pivot bar", Order = 2, GroupName = "Strategy")]
        public int Strength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Ema Entry filter", Order = 3, GroupName = "Strategy")]
        public bool EnableEmaEntry
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Ema (entry)", Order = 4, GroupName = "Strategy")]
        public int EmaEntryPeriod
        { get; set; }

        [Display(Name = "Ema short (exit)", Order = 5, GroupName = "Strategy")]
        public int EmaShortPeriod
        { get; set; }

        [Display(Name = "Ema long (exit)", Order = 6, GroupName = "Strategy")]
        public int EmaLongPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Keep broken lines", Description = "Show broken swing lines, beginning to end", Order = 3, GroupName = "Options")]
        public bool KeepBrokenLines
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Htf Swing Level color", Description = "Color for htf swing level rays/lines", Order = 4, GroupName = "Options")]
        public Brush HtfSwingColor
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Ltf Swing Level color", Description = "Color for ltf swing level rays/lines", Order = 5, GroupName = "Options")]
        public Brush LtfSwingColor
        { get; set; }

        #endregion
    }
}
