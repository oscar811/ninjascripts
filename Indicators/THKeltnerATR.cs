//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.TickHunterTA
{
	/// <summary>
	/// Keltner Channel. The Keltner Channel is a similar indicator to Bollinger Bands.
	/// Here the midline is a standard moving average with the upper and lower bands offset
	/// by the SMA of the difference between the high and low of the previous bars.
	/// The offset multiplier as well as the SMA period is configurable.
	/// </summary>
	public class THKeltnerATR : Indicator
	{
		private const string SystemVersion = "v1.066";
		private const string SystemName = "THKeltnerATR";
        private const string FullSystemName = SystemName + " - " + SystemVersion;

		private	EMA	emaValue;
		private SMA smaValue;
		private ATR atrValue;

		public override string DisplayName
		{
			get
			{
				if (State == State.SetDefaults)
					return FullSystemName;
				else if (ShowIndicatorName)
					return FullSystemName;
				else
					return "";
			}
		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = FullSystemName;
				Calculate = Calculate.OnPriceChange;

				IsOverlay = true;
				IsSuspendedWhileInactive = true;
				IsAutoScale = false;
				PaintPriceMarkers = false;

				UseExponentialMovingAverage = true;
				MAPeriod = 200;
				ATRPeriod = 200;
				OffsetMultiplier1 = 3.0;
				OffsetMultiplier2 = 6.0;
				OffsetMultiplier3 = 9.0;
				OffsetMultiplier4 = 12.0;

				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "KeltnerMidline");
				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1), PlotStyle.Line, "KeltnerUpperLevel1");
				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1), PlotStyle.Line, "KeltnerLowerLevel1");
				AddPlot(new Stroke(Brushes.MediumPurple, DashStyleHelper.Solid, 1), PlotStyle.Line, "KeltnerUpperLevel2");
				AddPlot(new Stroke(Brushes.MediumPurple, DashStyleHelper.Solid, 1), PlotStyle.Line, "KeltnerLowerLevel2");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerUpperLevel3");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerLowerLevel3");
				AddPlot(new Stroke(Brushes.RoyalBlue, 1), PlotStyle.Line, "KeltnerUpperLevel4");
				AddPlot(new Stroke(Brushes.RoyalBlue, 1), PlotStyle.Line, "KeltnerLowerLevel4");
			}
			else if (State == State.DataLoaded)
			{
				this.SetZOrder(-1);

				if (UseExponentialMovingAverage)
				{
					emaValue = EMA(Close, MAPeriod);
				}
				else
				{

					smaValue = SMA(Close, MAPeriod);
				}
				atrValue = ATR(ATRPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < MAPeriod)
				return;


			double middle	= (UseExponentialMovingAverage) ? emaValue[0] : smaValue[0];

			double offset1 = atrValue[0] * OffsetMultiplier1;
			double offset2 = atrValue[0] * OffsetMultiplier2;
			double offset3 = atrValue[0] * OffsetMultiplier3;
			double offset4 = atrValue[0] * OffsetMultiplier4;

			double upperLevel1	= middle + offset1;
			double lowerLevel1	= middle - offset1;
			double upperLevel2 = middle + offset2;
			double lowerLevel2 = middle - offset2;
			double upperLevel3 = middle + offset3;
			double lowerLevel3 = middle - offset3;
			double upperLevel4 = middle + offset4;
			double lowerLevel4 = middle - offset4;

			Midline[0] = middle;
			UpperLevel1[0] = upperLevel1;
			LowerLevel1[0] = lowerLevel1;
			UpperLevel2[0] = upperLevel2;
			LowerLevel2[0] = lowerLevel2;
			UpperLevel3[0] = upperLevel3;
			LowerLevel3[0] = lowerLevel3;
			UpperLevel4[0] = upperLevel4;
			LowerLevel4[0] = lowerLevel4;

		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[NinjaScriptProperty]
		[Display(Name = "ShowIndicatorName", GroupName = "0) Indicator Information", Order = 1)]
		public bool ShowIndicatorName
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "UseExponentialMovingAverage", GroupName="Parameters", Order=0)]
		public bool UseExponentialMovingAverage
		{
			get;
			set;

		}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MAPeriod", GroupName = "Parameters", Order = 1)]
		public int MAPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATRPeriod", GroupName = "Parameters", Order = 2)]
		public int ATRPeriod
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier1", GroupName = "Parameters", Order = 3)]
		public double OffsetMultiplier1
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier2", GroupName = "Parameters", Order = 4)]
		public double OffsetMultiplier2
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier3", GroupName = "Parameters", Order = 5)]
		public double OffsetMultiplier3
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier4", GroupName = "Parameters", Order = 6)]
		public double OffsetMultiplier4
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel1
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel1
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel2
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel2
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel3
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel3
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel4
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel4
		{
			get { return Values[8]; }
		}


		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunterTA.THKeltnerATR[] cacheTHKeltnerATR;
		public TickHunterTA.THKeltnerATR THKeltnerATR(string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			return THKeltnerATR(Input, indicatorName, showIndicatorName, useExponentialMovingAverage, mAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3, offsetMultiplier4);
		}

		public TickHunterTA.THKeltnerATR THKeltnerATR(ISeries<double> input, string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			if (cacheTHKeltnerATR != null)
				for (int idx = 0; idx < cacheTHKeltnerATR.Length; idx++)
					if (cacheTHKeltnerATR[idx] != null && cacheTHKeltnerATR[idx].IndicatorName == indicatorName && cacheTHKeltnerATR[idx].ShowIndicatorName == showIndicatorName && cacheTHKeltnerATR[idx].UseExponentialMovingAverage == useExponentialMovingAverage && cacheTHKeltnerATR[idx].MAPeriod == mAPeriod && cacheTHKeltnerATR[idx].ATRPeriod == aTRPeriod && cacheTHKeltnerATR[idx].OffsetMultiplier1 == offsetMultiplier1 && cacheTHKeltnerATR[idx].OffsetMultiplier2 == offsetMultiplier2 && cacheTHKeltnerATR[idx].OffsetMultiplier3 == offsetMultiplier3 && cacheTHKeltnerATR[idx].OffsetMultiplier4 == offsetMultiplier4 && cacheTHKeltnerATR[idx].EqualsInput(input))
						return cacheTHKeltnerATR[idx];
			return CacheIndicator<TickHunterTA.THKeltnerATR>(new TickHunterTA.THKeltnerATR(){ IndicatorName = indicatorName, ShowIndicatorName = showIndicatorName, UseExponentialMovingAverage = useExponentialMovingAverage, MAPeriod = mAPeriod, ATRPeriod = aTRPeriod, OffsetMultiplier1 = offsetMultiplier1, OffsetMultiplier2 = offsetMultiplier2, OffsetMultiplier3 = offsetMultiplier3, OffsetMultiplier4 = offsetMultiplier4 }, input, ref cacheTHKeltnerATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunterTA.THKeltnerATR THKeltnerATR(string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			return indicator.THKeltnerATR(Input, indicatorName, showIndicatorName, useExponentialMovingAverage, mAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3, offsetMultiplier4);
		}

		public Indicators.TickHunterTA.THKeltnerATR THKeltnerATR(ISeries<double> input , string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			return indicator.THKeltnerATR(input, indicatorName, showIndicatorName, useExponentialMovingAverage, mAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3, offsetMultiplier4);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunterTA.THKeltnerATR THKeltnerATR(string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			return indicator.THKeltnerATR(Input, indicatorName, showIndicatorName, useExponentialMovingAverage, mAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3, offsetMultiplier4);
		}

		public Indicators.TickHunterTA.THKeltnerATR THKeltnerATR(ISeries<double> input , string indicatorName, bool showIndicatorName, bool useExponentialMovingAverage, int mAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3, double offsetMultiplier4)
		{
			return indicator.THKeltnerATR(input, indicatorName, showIndicatorName, useExponentialMovingAverage, mAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3, offsetMultiplier4);
		}
	}
}

#endregion
