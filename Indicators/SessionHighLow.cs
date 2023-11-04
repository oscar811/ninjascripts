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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SessionHighLow : Indicator
	{
        private TimeWindow window;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"SessionHighLow";
				Name										= "SessionHighLow";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                StartTime1      = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime1        = DateTime.Parse("19:30", System.Globalization.CultureInfo.InvariantCulture);

				AddPlot(Brushes.Transparent,"Session_High");
                AddPlot(Brushes.Transparent,"Session_Low");
			}
			else if (State == State.Configure)
			{
                window = new TimeWindow(1, StartTime1, EndTime1, Session_High, Session_Low);

				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
			try
			{
	            if (CurrentBar < 20)
	                return;

				DateTime estTime = Time[0];

                if (estTime.TimeOfDay >= window.StartTime.TimeOfDay && estTime.TimeOfDay <= window.EndTime.TimeOfDay)
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
                        window.HighPrices[0] = High[0];
                        window.LowPrices[0] = Low[0];
                    }
                    else
                    {
                        window.HighPrices[0] = Math.Max(window.HighPrices[1], High[0]);
                        window.LowPrices[0] = Math.Min(window.LowPrices[1], Low[0]);

                        int barsSinceStart = CurrentBar - window.StartBar;

                        RemoveDrawObject("MyBox" + window.StartBar.ToString());
                        Draw.Rectangle(this, "MyBox" + window.StartBar.ToString(), true, barsSinceStart, window.HighPrices[0], 0, window.LowPrices[0], Brushes.Blue, Brushes.Transparent, 50);
                    }
                }
				else 
				{
					if (window.HighPrices[1] > 0) window.HighPrices[0] = window.HighPrices[1];
					if (window.LowPrices[1] > 0) window.LowPrices[0] = window.LowPrices[1];
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
            public int Index { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public Series<double> HighPrices { get; set; }
            public Series<double>  LowPrices { get; set; }

            public int StartBar { get; set; }
            public bool IsActive { get; set; }

            public TimeWindow(int index, DateTime start, DateTime end, Series<double> highPrices, Series<double> lowPrices)
            {
                Index = index;
                StartTime = start;
                EndTime = end;
                HighPrices = highPrices;
                LowPrices = lowPrices;
            }
        }

		#region Properties

		[Browsable(true)]
        [XmlIgnore]
        public Series<double> Session_High
        {
            get { return Values[0]; }
        }
		
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Session_Low
        {
            get { return Values[1]; }
        }        

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Session start", Order = 3, GroupName = "TimeWindows")]
        public DateTime StartTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Session end", Order = 4, GroupName = "TimeWindows")]
        public DateTime EndTime1 { get; set; }
        

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SessionHighLow[] cacheSessionHighLow;
		public SessionHighLow SessionHighLow(DateTime startTime1, DateTime endTime1)
		{
			return SessionHighLow(Input, startTime1, endTime1);
		}

		public SessionHighLow SessionHighLow(ISeries<double> input, DateTime startTime1, DateTime endTime1)
		{
			if (cacheSessionHighLow != null)
				for (int idx = 0; idx < cacheSessionHighLow.Length; idx++)
					if (cacheSessionHighLow[idx] != null && cacheSessionHighLow[idx].StartTime1 == startTime1 && cacheSessionHighLow[idx].EndTime1 == endTime1 && cacheSessionHighLow[idx].EqualsInput(input))
						return cacheSessionHighLow[idx];
			return CacheIndicator<SessionHighLow>(new SessionHighLow(){ StartTime1 = startTime1, EndTime1 = endTime1 }, input, ref cacheSessionHighLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SessionHighLow SessionHighLow(DateTime startTime1, DateTime endTime1)
		{
			return indicator.SessionHighLow(Input, startTime1, endTime1);
		}

		public Indicators.SessionHighLow SessionHighLow(ISeries<double> input , DateTime startTime1, DateTime endTime1)
		{
			return indicator.SessionHighLow(input, startTime1, endTime1);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SessionHighLow SessionHighLow(DateTime startTime1, DateTime endTime1)
		{
			return indicator.SessionHighLow(Input, startTime1, endTime1);
		}

		public Indicators.SessionHighLow SessionHighLow(ISeries<double> input , DateTime startTime1, DateTime endTime1)
		{
			return indicator.SessionHighLow(input, startTime1, endTime1);
		}
	}
}

#endregion
