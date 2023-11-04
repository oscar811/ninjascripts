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
        private List<TimeWindow> timeWindows = new List<TimeWindow>();
        private int barsCurrent;
        private int barsAtLine1;

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

                StartTime1 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime1 = DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
                StartTime2 = DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime2 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime3 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime3 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime4 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                EndTime4 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);

                AddPlot(Brushes.Transparent, "Asian_High");
                AddPlot(Brushes.Transparent, "Asian_Low");
                AddPlot(Brushes.Transparent, "London_High");
                AddPlot(Brushes.Transparent, "London_Low");
                AddPlot(Brushes.Transparent, "Ny_Am_High");
                AddPlot(Brushes.Transparent, "Ny_Am_Low");
                AddPlot(Brushes.Transparent, "Ny_Pm_High");
                AddPlot(Brushes.Transparent, "Ny_Pm_Low");
            }
            else if (State == State.Configure)
            {
                //lastDate = Core.Globals.MinDate;

                timeWindows.Add(new TimeWindow("Asian Session", StartTime1, EndTime1, Asian_High, Asian_Low));
                //timeWindows.Add(new TimeWindow("London Session", StartTime2, EndTime2, London_High, London_Low));
                //timeWindows.Add(new TimeWindow("Ny Am Session", StartTime3, EndTime3, Ny_Am_High, Ny_Pm_Low));
                //timeWindows.Add(new TimeWindow("Ny Pm Session", StartTime4, EndTime4, Ny_Pm_High, Ny_Pm_Low));
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();

                // sessionIterator = new Data.SessionIterator(Bars);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 10)
                    return;

                DateTime estTime = Time[0];
                int i = 0;

                foreach (var window in timeWindows)
                {
                    Print("window.StartTime: " + window.StartTime);
                    Print("window.EndTime: " + window.EndTime);
                    Print("CurrentBar: " + CurrentBar);                    

                    int startBarsAgo = Bars.GetBar(window.StartTime);
                    int endBarsAgo = Bars.GetBar(window.EndTime);
					
					Print("startBarsAgo: " + startBarsAgo);
					Print("endBarsAgo: " + endBarsAgo);

                    double highestHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo + 1];
                    double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo + 1];

                    window.HighPrices[0] = highestHigh;
                    window.LowPrices[0] = lowestLow;

                    // draw line from midnight to current
                    //Draw.Line(this, window.name + " High", false, (barsCurrent - barsAtLine5), my5MinHigh, -1, my5MinHigh, min5Color, min5Dash, min5Width);
                    Draw.Line(this, window.Name + " High", false, window.StartTime, window.HighPrices[0], window.EndTime, window.HighPrices[0], "test");


                    // if (estTime.TimeOfDay >= window.StartTime.TimeOfDay && estTime.TimeOfDay <= window.EndTime.TimeOfDay)
                    // {
                    //     window.IsActive = true;
                    // }
                    // else
                    // {
                    //     window.StartBar = 0;
                    //     window.IsActive = false;
                    // }

                    // if (window.IsActive)
                    // {
                    //     if (window.StartBar == 0)
                    //     {
                    //         window.StartBar = CurrentBar;
                    //         window.HighPrices[0] = High[0];
                    //         window.LowPrices[0] = Low[0];
                    //     }
                    //     else
                    //     {
                    //         window.HighPrices[0] = Math.Max(window.HighPrices[1], High[0]);
                    //         window.LowPrices[0] = Math.Min(window.LowPrices[1], Low[0]);

                    //         int barsSinceStart = CurrentBar - window.StartBar;

                    //         RemoveDrawObject("MyBox" + window.StartBar.ToString());
                    //         Draw.Rectangle(this, "MyBox" + window.StartBar.ToString(), true, barsSinceStart, window.HighPrices[0], 0, window.LowPrices[0], Brushes.Blue, Brushes.Transparent, 50);
                    //     }
                    // }
                    // else
                    // {
                    //     if (window.HighPrices[1] > 0) window.HighPrices[0] = window.HighPrices[1];
                    //     if (window.LowPrices[1] > 0) window.LowPrices[0] = window.LowPrices[1];

                    //     Print("window.HighPrices[0] : " + window.HighPrices[0]);
                    // }

                    i++;
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

            public Series<double> HighPrices { get; set; }
            public Series<double> LowPrices { get; set; }

            public int StartBar { get; set; }
            public bool IsActive { get; set; }

            public TimeWindow(string name, DateTime start, DateTime end, Series<double> highPrices, Series<double> lowPrices)
            {
                name = name;
                StartTime = start;
                EndTime = end;
                HighPrices = highPrices;
                LowPrices = lowPrices;
            }
        }

        #region Properties

        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Asian_High
        {
            get { return Values[0]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Asian_Low
        {
            get { return Values[1]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> London_High
        {
            get { return Values[2]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> London_Low
        {
            get { return Values[3]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Am_High
        {
            get { return Values[4]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Am_Low
        {
            get { return Values[5]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Pm_High
        {
            get { return Values[6]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Pm_Low
        {
            get { return Values[7]; }
        }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia start", Order = 3, GroupName = "TimeWindows")]
        public DateTime StartTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia end", Order = 4, GroupName = "TimeWindows")]
        public DateTime EndTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London start", Order = 5, GroupName = "TimeWindows")]
        public DateTime StartTime2 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London end", Order = 6, GroupName = "TimeWindows")]
        public DateTime EndTime2 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am start", Order = 7, GroupName = "TimeWindows")]
        public DateTime StartTime3 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am end", Order = 8, GroupName = "TimeWindows")]
        public DateTime EndTime3 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm start", Order = 9, GroupName = "TimeWindows")]
        public DateTime StartTime4 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm end", Order = 10, GroupName = "TimeWindows")]
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
		public RajIndicators.SessionLevels SessionLevels(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			return SessionLevels(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4);
		}

		public RajIndicators.SessionLevels SessionLevels(ISeries<double> input, DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			if (cacheSessionLevels != null)
				for (int idx = 0; idx < cacheSessionLevels.Length; idx++)
					if (cacheSessionLevels[idx] != null && cacheSessionLevels[idx].StartTime1 == startTime1 && cacheSessionLevels[idx].EndTime1 == endTime1 && cacheSessionLevels[idx].StartTime2 == startTime2 && cacheSessionLevels[idx].EndTime2 == endTime2 && cacheSessionLevels[idx].StartTime3 == startTime3 && cacheSessionLevels[idx].EndTime3 == endTime3 && cacheSessionLevels[idx].StartTime4 == startTime4 && cacheSessionLevels[idx].EndTime4 == endTime4 && cacheSessionLevels[idx].EqualsInput(input))
						return cacheSessionLevels[idx];
			return CacheIndicator<RajIndicators.SessionLevels>(new RajIndicators.SessionLevels(){ StartTime1 = startTime1, EndTime1 = endTime1, StartTime2 = startTime2, EndTime2 = endTime2, StartTime3 = startTime3, EndTime3 = endTime3, StartTime4 = startTime4, EndTime4 = endTime4 }, input, ref cacheSessionLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.SessionLevels SessionLevels(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4);
		}

		public Indicators.RajIndicators.SessionLevels SessionLevels(ISeries<double> input , DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.SessionLevels SessionLevels(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4);
		}

		public Indicators.RajIndicators.SessionLevels SessionLevels(ISeries<double> input , DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4)
		{
			return indicator.SessionLevels(input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4);
		}
	}
}

#endregion
