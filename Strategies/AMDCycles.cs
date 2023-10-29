using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
using System.ComponentModel;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace NinjaTrader.NinjaScript.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using NinjaTrader.Cbi;
    using NinjaTrader.Gui.Chart;
    using NinjaTrader.NinjaScript.Indicators;

    /// <summary>
    /// The `AMDCycles` class is a strategy in NinjaTrader that defines multiple time windows and tracks the high and low prices within each window. It also draws rectangles on the chart to visualize the price range of each window.
    /// </summary>
    public class AMDCycles : Strategy
    {
        private List<TimeWindow> timeWindows = new List<TimeWindow>();

        private double bbUp;
        private double bbDn;

        private SMA smaHigh;
        private StdDev stdDevHigh;
        private SMA smaLow;
        private StdDev stdDevLow;
        private double superBollinger;
        private Brush superBollingerColor;

        private class TimeWindow
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public double HighPrice { get; set; }
            public double LowPrice { get; set; }
            public int StartBar { get; set; }
            public bool IsActive { get; set; }

            public TimeWindow(TimeSpan start, TimeSpan end)
            {
                StartTime = start;
                EndTime = end;
            }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "AMDCycles.";
                Name = "AMDCycles";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;

                StartTime1 = new TimeSpan(18, 0, 0);
                EndTime1 = new TimeSpan(19, 30, 0);

                StartTime2 = new TimeSpan(19, 30, 0);
                EndTime2 = new TimeSpan(21, 0, 0);

                StartTime3 = new TimeSpan(21, 0, 0);
                EndTime3 = new TimeSpan(22, 30, 0);

                StartTime4 = new TimeSpan(22, 30, 0);
                EndTime4 = new TimeSpan(23, 59, 0);

                StartTime5 = new TimeSpan(0, 1, 0);
                EndTime5 = new TimeSpan(1, 30, 0);

                StartTime6 = new TimeSpan(1, 30, 0);
                EndTime6 = new TimeSpan(3, 0, 0);

                StartTime7 = new TimeSpan(3, 0, 0);
                EndTime7 = new TimeSpan(4, 30, 0);

                StartTime8 = new TimeSpan(4, 30, 0);
                EndTime8 = new TimeSpan(6, 0, 0);

                StartTime9 = new TimeSpan(6, 0, 0);
                EndTime9 = new TimeSpan(7, 30, 0);

                StartTime10 = new TimeSpan(7, 30, 0);
                EndTime10 = new TimeSpan(9, 0, 0);

                StartTime11 = new TimeSpan(9, 0, 0);
                EndTime11 = new TimeSpan(10, 30, 0);

                StartTime12 = new TimeSpan(10, 30, 0);
                EndTime12 = new TimeSpan(12, 0, 0);

                StartTime13 = new TimeSpan(12, 0, 0);
                EndTime13 = new TimeSpan(13, 30, 0);

                StartTime14 = new TimeSpan(13, 30, 0);
                EndTime14 = new TimeSpan(15, 0, 0);

                StartTime15 = new TimeSpan(15, 0, 0);
                EndTime15 = new TimeSpan(16, 30, 0);

                StartTime16 = new TimeSpan(16, 30, 0);
                EndTime16 = new TimeSpan(18, 0, 0);


                Period = 20;
                Multiplier = 2;
                superBollingerColor = Brushes.Red;

                TP = 100;
                SL = 50;
            }
            else if (State == State.Configure)
            {
                timeWindows.Add(new TimeWindow(StartTime1, EndTime1));
                timeWindows.Add(new TimeWindow(StartTime2, EndTime2));
                timeWindows.Add(new TimeWindow(StartTime3, EndTime3));
                timeWindows.Add(new TimeWindow(StartTime4, EndTime4));
                timeWindows.Add(new TimeWindow(StartTime5, EndTime5));
                timeWindows.Add(new TimeWindow(StartTime6, EndTime6));
                timeWindows.Add(new TimeWindow(StartTime7, EndTime7));
                timeWindows.Add(new TimeWindow(StartTime8, EndTime8));
                timeWindows.Add(new TimeWindow(StartTime9, EndTime9));
                timeWindows.Add(new TimeWindow(StartTime10, EndTime10));
                timeWindows.Add(new TimeWindow(StartTime11, EndTime11));
                timeWindows.Add(new TimeWindow(StartTime12, EndTime12));
                timeWindows.Add(new TimeWindow(StartTime13, EndTime13));
                timeWindows.Add(new TimeWindow(StartTime14, EndTime14));
                timeWindows.Add(new TimeWindow(StartTime15, EndTime15));
                timeWindows.Add(new TimeWindow(StartTime16, EndTime16));

                SetProfitTarget(CalculationMode.Ticks, TP, true);
                SetTrailStop(CalculationMode.Ticks, SL);				
            }
            else if (State == State.DataLoaded)
            {
                smaHigh = SMA(High, Period);
                stdDevHigh = StdDev(High, Period);
                smaLow = SMA(Low, Period);
                stdDevLow = StdDev(Low, Period);

                ClearOutputWindow();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20)
                return;

            bbUp = smaHigh[0] + stdDevHigh[0] * Multiplier;
            bbDn = smaLow[0] - stdDevLow[0] * Multiplier;

            bool longCondition = CrossAbove(Close, superBollinger, 1);
            bool shortCondition = CrossBelow(Close, superBollinger, 1);

            if (longCondition)
            {
                superBollinger = bbDn;
                superBollingerColor = Brushes.Lime;
            }
            else if (shortCondition)
            {
                superBollinger = bbUp;
                superBollingerColor = Brushes.Red;
            }
            else if (double.IsNaN(bbDn) || double.IsNaN(bbUp))
            {
                superBollinger = 0.0;
            }
            else
            {
                superBollinger = Close[0] > superBollinger ? Math.Max(superBollinger, bbDn) : Close[0] < superBollinger ? Math.Min(superBollinger, bbUp) : superBollinger;
            }


            DateTime estTime = Time[0];

            foreach (var window in timeWindows)
            {
                if (estTime.TimeOfDay >= window.StartTime && estTime.TimeOfDay <= window.EndTime)
                {
                    window.IsActive = true;
                }
                else
                {
                    window.StartBar = 0;
                    window.IsActive = false;
                }

                if (window.IsActive)
                {
                    if (window.StartBar == 0)
                    {
                        window.StartBar = CurrentBar;
                        window.HighPrice = High[0];
                        window.LowPrice = Low[0];
                    }

                    window.HighPrice = Math.Max(window.HighPrice, High[0]);
                    window.LowPrice = Math.Min(window.LowPrice, Low[0]);

                    int barsSinceStart = CurrentBar - window.StartBar;

                    RemoveDrawObject("MyBox" + window.StartBar.ToString());
                    Draw.Rectangle(this, "MyBox" + window.StartBar.ToString(), true, barsSinceStart, window.HighPrice, 0, window.LowPrice, Brushes.Blue, Brushes.Transparent, 50);

                    if (Position.MarketPosition == MarketPosition.Flat && longCondition)
                    {
                        EnterLong();
                    }
                    else if (Position.MarketPosition == MarketPosition.Flat && shortCondition)
                    {
                        EnterShort();
                    }
                }
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "TP in ticks", Order = 1, GroupName = "Strategy")]
        public int TP { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "SL in ticks", Order = 2, GroupName = "Strategy")]
        public int SL { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period", Order = 1, GroupName = "BollingerBand")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Multiplier", Order = 2, GroupName = "BollingerBand")]
        public double Multiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia A start", Order = 3, GroupName = "TimeWindows")]
        public TimeSpan StartTime1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia A end", Order = 4, GroupName = "TimeWindows")]
        public TimeSpan EndTime1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia M start", Order = 5, GroupName = "TimeWindows")]
        public TimeSpan StartTime2 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia M end", Order = 6, GroupName = "TimeWindows")]
        public TimeSpan EndTime2 { get; set; }
        [NinjaScriptProperty]
        [Display(Name = "Asia D start", Order = 7, GroupName = "TimeWindows")]
        public TimeSpan StartTime3 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia D end", Order = 8, GroupName = "TimeWindows")]
        public TimeSpan EndTime3 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia R start", Order = 9, GroupName = "TimeWindows")]
        public TimeSpan StartTime4 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia R end", Order = 10, GroupName = "TimeWindows")]
        public TimeSpan EndTime4 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London A start", Order = 11, GroupName = "TimeWindows")]
        public TimeSpan StartTime5 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London A end", Order = 12, GroupName = "TimeWindows")]
        public TimeSpan EndTime5 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London M start", Order = 13, GroupName = "TimeWindows")]
        public TimeSpan StartTime6 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London M end", Order = 14, GroupName = "TimeWindows")]
        public TimeSpan EndTime6 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London D start", Order = 15, GroupName = "TimeWindows")]
        public TimeSpan StartTime7 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London D end", Order = 16, GroupName = "TimeWindows")]
        public TimeSpan EndTime7 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London R start", Order = 17, GroupName = "TimeWindows")]
        public TimeSpan StartTime8 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London R end", Order = 18, GroupName = "TimeWindows")]
        public TimeSpan EndTime8 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM A start", Order = 19, GroupName = "TimeWindows")]
        public TimeSpan StartTime9 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM A end", Order = 20, GroupName = "TimeWindows")]
        public TimeSpan EndTime9 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM M start", Order = 21, GroupName = "TimeWindows")]
        public TimeSpan StartTime10 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM M end", Order = 22, GroupName = "TimeWindows")]
        public TimeSpan EndTime10 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM D start", Order = 23, GroupName = "TimeWindows")]
        public TimeSpan StartTime11 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM D end", Order = 24, GroupName = "TimeWindows")]
        public TimeSpan EndTime11 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM R start", Order = 25, GroupName = "TimeWindows")]
        public TimeSpan StartTime12 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM R end", Order = 26, GroupName = "TimeWindows")]
        public TimeSpan EndTime12 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM A start", Order = 27, GroupName = "TimeWindows")]
        public TimeSpan StartTime13 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM A end", Order = 28, GroupName = "TimeWindows")]
        public TimeSpan EndTime13 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM M start", Order = 29, GroupName = "TimeWindows")]
        public TimeSpan StartTime14 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM M end", Order = 30, GroupName = "TimeWindows")]
        public TimeSpan EndTime14 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM D start", Order = 31, GroupName = "TimeWindows")]
        public TimeSpan StartTime15 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM D end", Order = 32, GroupName = "TimeWindows")]
        public TimeSpan EndTime15 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM R start", Order = 33, GroupName = "TimeWindows")]
        public TimeSpan StartTime16 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM R end", Order = 34, GroupName = "TimeWindows")]
        public TimeSpan EndTime16 { get; set; }


        #endregion
    }
}
