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
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using System.Xml.Linq;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class CaptainBacktest : Strategy
    {
        private DateTime priceRangeStart;
        private DateTime priceRangeEnd;
        private DateTime biasWindowStart;
        private DateTime biasWindowEnd;
        private DateTime tradeWindowStart;
        private DateTime tradeWindowEnd;

        private bool retracenOppCandleClose = true;
        private bool retraceNPrevHighLowTaken = true;
        private bool useStopOrder = false;

        private TimeWindow priceRangeWindow;
        private TimeWindow biasWindow;
        private TimeWindow tradeWindow;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Captain Backtest Model [TFO]";
                Name = "CaptainBacktest";
                Calculate = Calculate.OnPriceChange;
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

                riskPoints = 25;
                rewardPoints = 50;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                SetProfitTarget(CalculationMode.Ticks, rewardPoints / TickSize);
                SetStopLoss(CalculationMode.Ticks, riskPoints / TickSize);
            }
            else if (State == State.DataLoaded)
            {
                priceRangeStart = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                priceRangeEnd = DateTime.Parse("10:00", System.Globalization.CultureInfo.InvariantCulture);
                priceRangeWindow = new TimeWindow("Price Range", priceRangeStart, priceRangeEnd);

                biasWindowStart = DateTime.Parse("10:00", System.Globalization.CultureInfo.InvariantCulture);
                biasWindowEnd = DateTime.Parse("11:15", System.Globalization.CultureInfo.InvariantCulture);
                biasWindow = new TimeWindow("Bias window", biasWindowStart, biasWindowEnd);

                tradeWindowStart = DateTime.Parse("10:00", System.Globalization.CultureInfo.InvariantCulture);
                tradeWindowEnd = DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
                tradeWindow = new TimeWindow("Trade window", tradeWindowStart, tradeWindowEnd);
            }
        }

        private double preHigh = 0;
        private double preLow = 0;
        private bool? bias = null;
        private bool oppClose = false;
        private bool took_hl = false;
        private bool longTrade = false;
        private bool shortTrade = false;
        private bool entryForSession = false;

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                TimePeriod lastPeriod = priceRangeWindow.GetLastPeriod(Time[0]);
                if (Time[1] <= lastPeriod.StartTime && Time[0] >= lastPeriod.StartTime)
                {
                    preHigh = 0;
                    preLow = 0;
                    bias = null;
                    oppClose = false;
                    took_hl = false;
                    longTrade = false;
                    shortTrade = false;
                    entryForSession = false;
                }

                if (Time[1] <= lastPeriod.EndTime && Time[0] >= lastPeriod.EndTime) // calculate only when time window expires
                {
                    int startBarsAgo = Bars.GetBar(lastPeriod.StartTime);
                    int endBarsAgo = Bars.GetBar(lastPeriod.EndTime);
                    preHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];
                    preLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];
                }

                if (biasWindow.IsInWindow(Time[0]))
                {
                    if (High[0] > preHigh)
                        bias = true;
                    if (Low[0] < preLow)
                        bias = false;
                }

                if (tradeWindow.IsInWindow(Time[0]))
                {
                    if (!retracenOppCandleClose)
                    {
                        oppClose = true;
                    }
                    else
                    {
                        if (bias == true && Close[0] < Open[0])
                            oppClose = true;
                        if (bias == false && Close[0] > Open[0])
                            oppClose = true;
                    }
                    if (!retraceNPrevHighLowTaken)
                    {
                        took_hl = true;
                    }
                    else
                    {
                        if (bias == true && Low[0] < Low[1])
                            took_hl = true;
                        if (bias == false && High[0] > High[1])
                            took_hl = true;
                    }

                    if (bias == true && Close[0] > High[1] && oppClose && took_hl && !longTrade)
                        longTrade = true;
                    if (bias == false && Close[0] < Low[1] && oppClose && took_hl && !shortTrade)
                        shortTrade = true;

                    Print("entryForSession: " + entryForSession);
                    if (longTrade == true && !entryForSession)
                    {
                        entryForSession = true;
                        EnterLong();
                    }
                    else if (shortTrade == true && !entryForSession)
                    {
                        entryForSession = true;
                        EnterShort();
                    }
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private class TimeWindow
        {
            public string Name { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public int StartBar { get; set; }
            public bool IsEnabled { get; set; }

            public TimeWindow(string name, DateTime start, DateTime end)
            {
                Name = name;
                StartTime = start;
                EndTime = end;
            }

            public TimePeriod GetLastPeriod(DateTime currentTime)
            {
                var timePeriod = new TimePeriod();

                DateTime prevStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, StartTime.Hour, StartTime.Minute, 0);
                DateTime prevEndTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, EndTime.Hour, EndTime.Minute, 0);

                if (prevEndTime < prevStartTime)
                    prevEndTime = prevEndTime.AddDays(1);

                if (currentTime < prevEndTime)
                {
                    prevStartTime = prevStartTime.AddDays(-1);
                    prevEndTime = prevEndTime.AddDays(-1);
                }

                return new TimePeriod { StartTime = prevStartTime, EndTime = prevEndTime };
            }

            public bool IsInWindow(DateTime currentTime)
            {
                return currentTime.TimeOfDay >= StartTime.TimeOfDay && currentTime.TimeOfDay <= EndTime.TimeOfDay;
            }
        }

        private class TimePeriod
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Risk", Order = 3, GroupName = "Risk")]
        public double riskPoints { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reward", Order = 3, GroupName = "Risk")]
        public double rewardPoints { get; set; }

        #endregion
    }
}
