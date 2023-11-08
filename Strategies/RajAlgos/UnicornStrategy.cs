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
using NinjaTrader.NinjaScript.Indicators.RajIndicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class UnicornStrategy : Strategy
    {
        private Swing swingIndicator;
        private double bearishCandleLow;
        private int bearishCandleBar;
        private bool sslSwept;
        private int sslSweptBar;
        private List<SwingPoint> swingHighs = new List<SwingPoint>();
        private List<SwingPoint> swingLows = new List<SwingPoint>();
        private List<int> usedUnicorns = new List<int>();
        private List<TimePeriod> timePeriods;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Bearish strategy that enters on liquidity sweeps of Swing High with a close below the low of the prior bearish candle.";
                Name = "UnicornStrategy";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 60;
                IsFillLimitOnTouch = true;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 10;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                Profit_Target = 800;
                Stop_Loss = 500;
                swingSize = 10;
                gapSizeThreshold = 5;

                Time_2 = true;
                Start_Time_2 = DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_2 = DateTime.Parse("17:00", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                swingIndicator = Swing(1);

                timePeriods = new List<TimePeriod>
                {
                    new TimePeriod(Time_2, Start_Time_2, Stop_Time_2)
                };
            }
            else if (State == State.DataLoaded)
            {
                AddChartIndicator(swingIndicator);
                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < swingSize || CurrentBar < BarsRequiredToTrade) return;

            Draw.Text(this, "CurrentBarText" + CurrentBar, CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Black);
           

            AddSwingPoints();

            if (High[0] >= swingIndicator.SwingHigh[swingSize] && !CheckIfSwingHighIsTaken(High[0]))
            {
                Print("CurrentBar: " + CurrentBar);
                Print("High[0]:" + High[0]);
                Print("swingIndicator.SwingHigh[0]:" + swingIndicator.SwingHigh[swingSize]);
            }

            if (CheckIfSwingHighIsTaken(High[0]))
            //if (High[0] >= swingIndicator.High[0])
            {
                

                sslSweptBar = CurrentBar;

                int bearishCandleBarAgo = FindBearishCandleBefore(CurrentBar - 1);
                bearishCandleBar = CurrentBar - bearishCandleBarAgo;
                bearishCandleLow = Low[bearishCandleBarAgo];
            }

            if (CurrentBar > 110)
            {
                Print("bearishCandleBar: " + bearishCandleBar);
                Print("bearishCandleLow: " + bearishCandleLow);
                Print("sslSweptBar: " + sslSweptBar);
            }

            //            if (Close[0] < bearishCandleLow && (CurrentBar - bearishCandleBar) < 10 && sslSwept && (CurrentBar - sslSweptBar) < 5)
            if (Close[0] < bearishCandleLow && (CurrentBar - bearishCandleBar) < 20 && (CurrentBar - sslSweptBar) < 5
                && Position.MarketPosition == MarketPosition.Flat
                && !usedUnicorns.Contains(bearishCandleBar))
            {                
                FvgType? fvgType = isFVGPresent(CurrentBar - sslSweptBar);

                if (fvgType.HasValue && fvgType.Value == FvgType.Bearish)
                {
                    string tag = "BB" + bearishCandleBar;

                    if (DrawObjects.FirstOrDefault(o => o.Tag == tag) == null)
                    {
                        Draw.Line(this, tag, false, CurrentBar - bearishCandleBar, bearishCandleLow, -20, bearishCandleLow, Brushes.Red, DashStyleHelper.Solid, 2);
                        Draw.Text(this, tag + "Text", "-🦄", -22, bearishCandleLow, Brushes.Red);
                    }

                    foreach (var timePeriod in timePeriods)
                    {
                        if (timePeriod.isTimeConditionMet(Time[0]))
                        {
                            EnterShort("Bearish Unicorn Entry");
                            usedUnicorns.Add(bearishCandleBar);
                        }
                    }
                }
            }

            RemoveSweptSwingPoints();
        }

        private FvgType? isFVGPresent(int barsAgo)
        {
            for (int i = 0; i <= barsAgo; i++)
            {
                if (High[i] < Low[i + 2] && (Low[i + 2] - High[i]) >= gapSizeThreshold * TickSize)
                {
                    return FvgType.Bearish;
                }
                else if (Low[i] > High[i + 2] && (Low[i] - High[i + 2]) >= gapSizeThreshold * TickSize)
                {
                    return FvgType.Bullish;
                }
            }
            return null;
        }

        enum FvgType
        {
            Bearish,
            Bullish
        }

        private void AddSwingPoints()
        {
            if (High[0] >= swingIndicator.SwingHigh[swingSize])
            {
                swingHighs.Add(new SwingPoint { Price = High[0], BarIndex = CurrentBar, IsSwept = false });
            }

            if (Low[0] <= swingIndicator.SwingLow[swingSize])
            {
                swingLows.Add(new SwingPoint { Price = Low[0], BarIndex = CurrentBar, IsSwept = false });
            }
        }

        private void RemoveSweptSwingPoints()
        {
            for (int i = 0; i < swingHighs.Count; i++)
            {
                if (!swingHighs[i].IsSwept && High[0] > swingHighs[i].Price)
                {
                    swingHighs[i].IsSwept = true;
                }
            }

            for (int i = 0; i < swingLows.Count; i++)
            {
                if (!swingLows[i].IsSwept && Low[0] < swingLows[i].Price)
                    swingLows[i].IsSwept = true;
            }

            swingHighs.RemoveAll(swingHigh => swingHigh.IsSwept);
            swingLows.RemoveAll(swingLow => swingLow.IsSwept);
        }

        private int FindBearishCandleBefore(int barIndex)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (Close[i] < Open[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private bool CheckIfSwingHighIsTaken(double highValue)
        {
            for (int i = 0; i <= Math.Min(1, swingHighs.Count - 1); i++)
            {
                if (highValue >= swingHighs[i].Price && swingHighs[i].IsSwept == false)
                {
                    Print("swing high taken: " + swingHighs[i].Price);

                    return true;
                }
            }

            return false;
        }

        public class SwingPoint
        {
            public double Price { get; set; }
            public int BarIndex { get; set; }
            public bool IsSwept { get; set; }
        }

        private class TimePeriod
        {
            private bool useTimePeriod;
            private DateTime startTime;
            private DateTime stopTime;

            public TimePeriod(bool useTimePeriod, DateTime startTime, DateTime stopTime)
            {
                this.useTimePeriod = useTimePeriod;
                this.startTime = startTime;
                this.stopTime = stopTime;
            }

            public bool isTimeConditionMet(DateTime currentTime)
            {
                return useTimePeriod && (currentTime.TimeOfDay >= startTime.TimeOfDay && currentTime.TimeOfDay <= stopTime.TimeOfDay);
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Swing Size", Order = 1, GroupName = "Strategy")]
        public int swingSize
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "FVG Gap Size Threshold", Order = 1, GroupName = "Strategy")]
        public int gapSizeThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Profit Target (ticks)", Order = 12, GroupName = "Atm")]
        public int Profit_Target
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop Loss (ticks)", Order = 13, GroupName = "Atm")]
        public int Stop_Loss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_2", Order = 9, GroupName = "Time")]
        public bool Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_2", Description = "Start Time", Order = 10, GroupName = "Time")]
        public DateTime Start_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_2", Description = "End Time", Order = 11, GroupName = "Time")]
        public DateTime Stop_Time_2
        { get; set; }
        #endregion
    }
}
