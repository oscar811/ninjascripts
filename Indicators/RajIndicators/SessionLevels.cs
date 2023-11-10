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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class SessionLevels : Indicator
    {
        private List<TimeWindow> TimeWindows = new List<TimeWindow>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"SessionLevels";
                Name = "SessionLevels";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event.
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                EnableAsian = true;
                StartTime1 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime1 = DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
                EnableLondon = true;
                StartTime2 = DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime2 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                EnableNyAm = true;
                StartTime3 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime3 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                EnableNyPm = true;
                StartTime4 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime4 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);

                ShowMidnight_Open = true;
                ShowDay_Open = true;
                ShowEight_Thirty_Open = true;

                AddPlot(Brushes.Red, "Asian_High");
                AddPlot(Brushes.Red, "Asian_Low");
                AddPlot(Brushes.Blue, "London_High");
                AddPlot(Brushes.Blue, "London_Low");
                AddPlot(Brushes.Green, "Ny_Am_High");
                AddPlot(Brushes.Green, "Ny_Am_Low");
                AddPlot(Brushes.HotPink, "Ny_Pm_High");
                AddPlot(Brushes.HotPink, "Ny_Pm_Low");

                AddPlot(Brushes.Red, "Midnight_Open");
                AddPlot(Brushes.Blue, "Day_Open_at_18");
                AddPlot(Brushes.Blue, "Day_Open_at_22");
            }
            else if (State == State.Configure)
            {
                
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();

                TimeWindows.Add(new TimeWindow("Asian Session", StartTime1, EndTime1, Asian_High, Asian_Low, EnableAsian));
                TimeWindows.Add(new TimeWindow("London Session", StartTime2, EndTime2, London_High, London_Low, EnableLondon));
                TimeWindows.Add(new TimeWindow("Ny Am Session", StartTime3, EndTime3, Ny_Am_High, Ny_Pm_Low, EnableNyAm));
                TimeWindows.Add(new TimeWindow("Ny Pm Session", StartTime4, EndTime4, Ny_Pm_High, Ny_Pm_Low, EnableNyPm));
            }
        }

        private bool isFirstDay = true;
        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 10)
                    return;

                DateTime estTime = Time[0];
                int i = 0;

                if (Bars.IsFirstBarOfSession)
                {
                    if (isFirstDay)
                    {
                        isFirstDay = false;
                        return;
                    }
                }

                foreach (var window in TimeWindows)
                {
                    //Print("window.Name: " + window.Name);
                    //Print("window.IsEnabled:" + window.IsEnabled);
                    //Print("window.StartTime: " + window.StartTime);
                    //Print("window.EndTime: " + window.EndTime);
                    //Print("Time[0]: " + Time[0]);
                    //Print("CurrentBar: " + CurrentBar);

                    if (!window.IsEnabled)
                        continue;

                    if (window.IsInWindow(Time[0]))
                        continue;

                    TimePeriod lastPeriod = window.GetLastPeriod(Time[0]);                    

                    if (Time[0] > lastPeriod.EndTime)
                    {
                        int startBarsAgo = Bars.GetBar(lastPeriod.StartTime);
                        int endBarsAgo = Bars.GetBar(lastPeriod.EndTime);

                        /* Now that we have the start and end bars ago values for the specified time range we can calculate the highest high for this range

                        Note: We add 1 to the period range for MAX and MIN to compensate for the difference between "period" logic and "bars ago" logic.
                        "Period" logic means exactly how many bars you want to check including the current bar.
                        "Bars ago" logic means how many bars we are going to go backwards. The current bar is not counted because on that bar we aren't going back any bars so it would be "bars ago = 0" */
                        double highestHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

                        // Now that we have the start and end bars ago values for the specified time range we can calculate the lowest low for this range
                        double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

                        // Set the plot values
                        //HighestHigh[0] = highestHigh;
                        //LowestLow[0] = lowestLow;

                        window.HighPrices[0] = highestHigh;
                        window.LowPrices[0] = lowestLow;
                    }

                    i++;
                }

                if (ShowMidnight_Open)
                {
                    DateTime midnight = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 0, 0, 0);
                    int midnightBarsAgo = Bars.GetBar(midnight);
                    Midnight_Open[0] = Close[CurrentBar - midnightBarsAgo];
                }

                if (ShowDay_Open)
                {
                    DateTime dayOpen = GetPreviousDate(new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 18, 0, 0));
                    int dayOpenBarsAgo = Bars.GetBar(dayOpen);
                    Day_Open_at_18[0] = Close[CurrentBar - dayOpenBarsAgo];
                }
                
                if (ShowEight_Thirty_Open)
                {
                    DateTime eightOpen = GetPreviousDate(new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 8, 30, 0));
                    int eightOpenBarsAgo = Bars.GetBar(eightOpen);
                    Eight_Thirty_Open[0] = Close[CurrentBar - eightOpenBarsAgo];
                }                
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private DateTime GetPreviousDate(DateTime dateTime)
        {
            DateTime prevDate = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, dateTime.Hour, dateTime.Minute, 0);
            if (dateTime.Hour == 0 && dateTime.Minute == 0) prevDate = prevDate.AddDays(1);

            if (Time[0].TimeOfDay < new TimeSpan(prevDate.Hour, prevDate.Minute, 0))
            {
                prevDate = prevDate.AddDays(-1);
            }

            return prevDate;
        }

        private class TimeWindow
        {
            public string Name { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public Series<double> HighPrices { get; set; }
            public Series<double> LowPrices { get; set; }

            public int StartBar { get; set; }
            public bool IsEnabled { get; set; }

            public TimeWindow(string name, DateTime start, DateTime end, Series<double> highPrices, Series<double> lowPrices, bool enabled)
            {
                Name = name;
                StartTime = start;
                EndTime = end;
                HighPrices = highPrices;
                LowPrices = lowPrices;
                IsEnabled = enabled;
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

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_High
        {
            get { return Values[0]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_Low
        {
            get { return Values[1]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_High
        {
            get { return Values[2]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_Low
        {
            get { return Values[3]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_High
        {
            get { return Values[4]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_Low
        {
            get { return Values[5]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_High
        {
            get { return Values[6]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_Low
        {
            get { return Values[7]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Midnight Open", Order = 1, GroupName = "Imp Levels")]
        public bool ShowMidnight_Open { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Midnight_Open
        {
            get { return Values[8]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Day Open", Order = 2, GroupName = "Imp Levels")]
        public bool ShowDay_Open { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Day_Open_at_18
        {
            get { return Values[9]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show 8:30 Open", Order = 3, GroupName = "Imp Levels")]
        public bool ShowEight_Thirty_Open { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Eight_Thirty_Open
        {
            get { return Values[10]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Asian", Order = 1, GroupName = "Time windows")]
        public bool EnableAsian { get; set; }
        
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia start", Order = 2, GroupName = "Time windows")]
        public DateTime StartTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia end", Order = 3, GroupName = "Time windows")]
        public DateTime EndTime1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London", Order = 4, GroupName = "Time windows")]
        public bool EnableLondon { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London start", Order = 5, GroupName = "Time windows")]
        public DateTime StartTime2 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London end", Order = 6, GroupName = "Time windows")]
        public DateTime EndTime2 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY AM", Order = 7, GroupName = "Time windows")]
        public bool EnableNyAm { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am start", Order = 8, GroupName = "Time windows")]
        public DateTime StartTime3 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am end", Order = 9, GroupName = "Time windows")]
        public DateTime EndTime3 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY PM", Order = 10, GroupName = "Time windows")]
        public bool EnableNyPm { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm start", Order = 11, GroupName = "Time windows")]
        public DateTime StartTime4 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm end", Order = 12, GroupName = "Time windows")]
        public DateTime EndTime4 { get; set; }

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.SessionLevels[] cacheSessionLevels;
		public RajIndicators.SessionLevels SessionLevels(bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			return SessionLevels(Input, showMidnight_Open, showDay_Open, showEight_Thirty_Open, enableAsian, startTime1, endTime1, enableLondon, startTime2, endTime2, enableNyAm, startTime3, endTime3, enableNyPm, startTime4, endTime4);
		}

		public RajIndicators.SessionLevels SessionLevels(ISeries<double> input, bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			if (cacheSessionLevels != null)
				for (int idx = 0; idx < cacheSessionLevels.Length; idx++)
					if (cacheSessionLevels[idx] != null && cacheSessionLevels[idx].ShowMidnight_Open == showMidnight_Open && cacheSessionLevels[idx].ShowDay_Open == showDay_Open && cacheSessionLevels[idx].ShowEight_Thirty_Open == showEight_Thirty_Open && cacheSessionLevels[idx].EnableAsian == enableAsian && cacheSessionLevels[idx].StartTime1 == startTime1 && cacheSessionLevels[idx].EndTime1 == endTime1 && cacheSessionLevels[idx].EnableLondon == enableLondon && cacheSessionLevels[idx].StartTime2 == startTime2 && cacheSessionLevels[idx].EndTime2 == endTime2 && cacheSessionLevels[idx].EnableNyAm == enableNyAm && cacheSessionLevels[idx].StartTime3 == startTime3 && cacheSessionLevels[idx].EndTime3 == endTime3 && cacheSessionLevels[idx].EnableNyPm == enableNyPm && cacheSessionLevels[idx].StartTime4 == startTime4 && cacheSessionLevels[idx].EndTime4 == endTime4 && cacheSessionLevels[idx].EqualsInput(input))
						return cacheSessionLevels[idx];
			return CacheIndicator<RajIndicators.SessionLevels>(new RajIndicators.SessionLevels(){ ShowMidnight_Open = showMidnight_Open, ShowDay_Open = showDay_Open, ShowEight_Thirty_Open = showEight_Thirty_Open, EnableAsian = enableAsian, StartTime1 = startTime1, EndTime1 = endTime1, EnableLondon = enableLondon, StartTime2 = startTime2, EndTime2 = endTime2, EnableNyAm = enableNyAm, StartTime3 = startTime3, EndTime3 = endTime3, EnableNyPm = enableNyPm, StartTime4 = startTime4, EndTime4 = endTime4 }, input, ref cacheSessionLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.SessionLevels SessionLevels(bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(Input, showMidnight_Open, showDay_Open, showEight_Thirty_Open, enableAsian, startTime1, endTime1, enableLondon, startTime2, endTime2, enableNyAm, startTime3, endTime3, enableNyPm, startTime4, endTime4);
		}

		public Indicators.RajIndicators.SessionLevels SessionLevels(ISeries<double> input , bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(input, showMidnight_Open, showDay_Open, showEight_Thirty_Open, enableAsian, startTime1, endTime1, enableLondon, startTime2, endTime2, enableNyAm, startTime3, endTime3, enableNyPm, startTime4, endTime4);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.SessionLevels SessionLevels(bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(Input, showMidnight_Open, showDay_Open, showEight_Thirty_Open, enableAsian, startTime1, endTime1, enableLondon, startTime2, endTime2, enableNyAm, startTime3, endTime3, enableNyPm, startTime4, endTime4);
		}

		public Indicators.RajIndicators.SessionLevels SessionLevels(ISeries<double> input , bool showMidnight_Open, bool showDay_Open, bool showEight_Thirty_Open, bool enableAsian, DateTime startTime1, DateTime endTime1, bool enableLondon, DateTime startTime2, DateTime endTime2, bool enableNyAm, DateTime startTime3, DateTime endTime3, bool enableNyPm, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(input, showMidnight_Open, showDay_Open, showEight_Thirty_Open, enableAsian, startTime1, endTime1, enableLondon, startTime2, endTime2, enableNyAm, startTime3, endTime3, enableNyPm, startTime4, endTime4);
		}
	}
}

#endregion
