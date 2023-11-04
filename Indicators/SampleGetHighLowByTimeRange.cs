//
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SampleGetHighLowByTimeRange : Indicator
	{ 
		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description					= @"Determines the highest high and lowest low in a specified time range";
				Name						= "Sample get high low by time range";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification 			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				StartHour					= 9;
				StartMinute					= 30;
				EndHour						= 10;
				EndMinute					= 15;
				AddPlot(Brushes.Green, "HighestHigh");
				AddPlot(Brushes.Red, "LowestLow");
			}
		}
		
		private DateTime startDateTime;
		private DateTime endDateTime;
		protected override void OnBarUpdate()
		{
            //if (CurrentBar < 150)
            //    return;
            // Check to make sure the end time is not earlier than the start time
            if (EndHour < StartHour)
                return;


            //Do not calculate the high or low value when the ending time of the desired range is less than the current time of the bar being processed
            if (ToTime(EndHour, EndMinute, 0) > ToTime(Time[0]))
                return;

            DateTime StartTime1 = new DateTime();
            DateTime EndTime1 = new DateTime();

            //if (startDateTime.Date != Time[0].Date)
            //{
            //    StartTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 18, 0, 0);
            //    EndTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 19, 0, 0);
            //}

            //If the stored date time date is not the same date as the bar time date, create a new DateTime object
            if (startDateTime.Date != Time[0].Date)
            {
                StartTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, StartHour, StartMinute, 0);
                EndTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, EndHour, EndMinute, 0);
            }

            // Calculate the number of bars ago for the start and end bars of the specified time range
            int startBarsAgo = Bars.GetBar(StartTime1);
			int endBarsAgo = Bars.GetBar(EndTime1);
			 
			/* Now that we have the start and end bars ago values for the specified time range we can calculate the highest high for this range
			 
			Note: We add 1 to the period range for MAX and MIN to compensate for the difference between "period" logic and "bars ago" logic.
			"Period" logic means exactly how many bars you want to check including the current bar.
			"Bars ago" logic means how many bars we are going to go backwards. The current bar is not counted because on that bar we aren't going back any bars so it would be "bars ago = 0" */
			double highestHigh = MAX(High, endBarsAgo - startBarsAgo  + 1)[CurrentBar - endBarsAgo];
			 
			// Now that we have the start and end bars ago values for the specified time range we can calculate the lowest low for this range
			double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

			// Set the plot values
			HighestHigh[0] = highestHigh;
			LowestLow[0] = lowestLow;			
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HighestHigh
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowestLow
		{
			get { return Values[1]; }
		}
		
		
		[Range(0,23)]
		[NinjaScriptProperty]
		[Display(Name="Start hour", Description = "Enter start hour, Military time format 0 - 23", Order=1, GroupName="Parameters")]
		public int StartHour
		{ get; set; }

		[Range(0, 59)]
		[NinjaScriptProperty]
		[Display(Name="Start minute", Description = "Enter start minute(s) 0 - 59",Order=2, GroupName="Parameters")]
		public int StartMinute
		{ get; set; }

		[Range(0, 23)]
		[NinjaScriptProperty]
		[Display(Name="End hour", Description = "Enter end hour, Military time format 0 - 23",Order=3, GroupName="Parameters")]
		public int EndHour
		{ get; set; }

		[Range(0, 59)]
		[NinjaScriptProperty]
		[Display(Name="End minute",Description = " Enter end minute(s) 0 - 59", Order=4, GroupName="Parameters")]
		public int EndMinute
		{ get; set; }

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleGetHighLowByTimeRange[] cacheSampleGetHighLowByTimeRange;
		public SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return SampleGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(ISeries<double> input, int startHour, int startMinute, int endHour, int endMinute)
		{
			if (cacheSampleGetHighLowByTimeRange != null)
				for (int idx = 0; idx < cacheSampleGetHighLowByTimeRange.Length; idx++)
					if (cacheSampleGetHighLowByTimeRange[idx] != null && cacheSampleGetHighLowByTimeRange[idx].StartHour == startHour && cacheSampleGetHighLowByTimeRange[idx].StartMinute == startMinute && cacheSampleGetHighLowByTimeRange[idx].EndHour == endHour && cacheSampleGetHighLowByTimeRange[idx].EndMinute == endMinute && cacheSampleGetHighLowByTimeRange[idx].EqualsInput(input))
						return cacheSampleGetHighLowByTimeRange[idx];
			return CacheIndicator<SampleGetHighLowByTimeRange>(new SampleGetHighLowByTimeRange(){ StartHour = startHour, StartMinute = startMinute, EndHour = endHour, EndMinute = endMinute }, input, ref cacheSampleGetHighLowByTimeRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SampleGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SampleGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SampleGetHighLowByTimeRange(Input, startHour, startMinute, endHour, endMinute);
		}

		public Indicators.SampleGetHighLowByTimeRange SampleGetHighLowByTimeRange(ISeries<double> input , int startHour, int startMinute, int endHour, int endMinute)
		{
			return indicator.SampleGetHighLowByTimeRange(input, startHour, startMinute, endHour, endMinute);
		}
	}
}

#endregion
