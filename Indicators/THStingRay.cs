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
	public class THStingRay : Indicator
	{
		private const string SystemVersion = "v1.044";
		private const string SystemName = "THStingRay";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		public override string DisplayName
		{
			get { return FullSystemName; }

		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = FullSystemName;

				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = true;
				ShowTransparentPlotsInDataBox = true;
				DrawOnPricePanel = true;
				PaintPriceMarkers = false;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;

				CreeperPeriod1 = 5;
				CreeperPeriod2 = 5;
				CreeperPeriod3 = 5;

				RushLinePeriod = 5;
				DriftLine1Period = 5;
				DriftLine2Period = 5;
				DriftLine3Period = 5;
				DriftLine4Period = 5;

				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Dash, 2), PlotStyle.Line, "SwingHighLine");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Dot, 2), PlotStyle.Line, "SwingHighBrokenLine");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Dash, 2), PlotStyle.Line, "SwingHighNonCLine");

				AddPlot(new Stroke(Brushes.ForestGreen, DashStyleHelper.Dash, 2), PlotStyle.Line, "SwingLowLine");
				AddPlot(new Stroke(Brushes.ForestGreen, DashStyleHelper.Dot, 2), PlotStyle.Line, "SwingLowBrokenLine");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Dash, 2), PlotStyle.Line, "SwingLowNonCLine");

				AddPlot(Brushes.Transparent, "SwingHigh5Level");
				AddPlot(Brushes.Transparent, "SwingHigh4Level");
				AddPlot(Brushes.Transparent, "SwingHigh3Level");
				AddPlot(Brushes.Transparent, "SwingHigh2Level");
				AddPlot(Brushes.Transparent, "SwingHigh1Level");

				AddPlot(Brushes.Transparent, "SwingLow1Level");
				AddPlot(Brushes.Transparent, "SwingLow2Level");
				AddPlot(Brushes.Transparent, "SwingLow3Level");
				AddPlot(Brushes.Transparent, "SwingLow4Level");
				AddPlot(Brushes.Transparent, "SwingLow5Level");

				AddPlot(Brushes.Transparent, "RecentHigherHigh");
				AddPlot(Brushes.Transparent, "RecentLowerLow");
				AddPlot(Brushes.Transparent, "CreeperCycleCount");
				AddPlot(Brushes.Transparent, "SurgeBarCount");
				AddPlot(Brushes.Transparent, "SurgeBarCycleTickSpeed");
				AddPlot(Brushes.Transparent, "Gush1BarCount");
				AddPlot(Brushes.Transparent, "Gush1BarCycleTickSpeed");
				AddPlot(Brushes.Transparent, "Flow1BarCount");
				AddPlot(Brushes.Transparent, "Flow1BarCycleTickSpeed");
				AddPlot(Brushes.Transparent, "Flow2BarCount");
				AddPlot(Brushes.Transparent, "Flow2BarCycleTickSpeed");
				AddPlot(Brushes.Transparent, "Flow3BarCount");
				AddPlot(Brushes.Transparent, "Flow3BarCycleTickSpeed");
				AddPlot(Brushes.Transparent, "Flow4BarCount");
				AddPlot(Brushes.Transparent, "Flow4BarCycleTickSpeed");
			}
			else if (State == State.Configure)
			{

			}
			else if (State == State.DataLoaded)
			{

			}
		}

		protected override void OnBarUpdate()
		{
			SwingHighLine[0] = 0;
			SwingHighBrokenLine[0] = 0;
			SwingHighNonCLine[0] = 0;
			SwingLowLine[0] = 0;
			SwingLowBrokenLine[0] = 0;
			SwingLowNonCLine[0] = 0;

			this.SwingHigh1Level[0] = 0;
			this.SwingHigh2Level[0] = 0;
			this.SwingHigh3Level[0] = 0;
			this.SwingHigh4Level[0] = 0;
			this.SwingHigh5Level[0] = 0;

			this.SwingLow1Level[0] = 0;
			this.SwingLow2Level[0] = 0;
			this.SwingLow3Level[0] = 0;
			this.SwingLow4Level[0] = 0;
			this.SwingLow5Level[0] = 0;

			RecentHigherHigh[0] = 0;
			RecentLowerLow[0] = 0;
			CreeperCycleCount[0] = 0;
			SurgeBarCount[0] = 0;
			SurgeBarCycleTickSpeed[0] = 0;
			Gush1BarCount[0] = 0;
			Gush1BarCycleTickSpeed[0] = 0;
			Flow1BarCount[0] = 0;
			Flow1BarCycleTickSpeed[0] = 0;
			Flow2BarCount[0] = 0;
			Flow2BarCycleTickSpeed[0] = 0;
			Flow3BarCount[0] = 0;
			Flow3BarCycleTickSpeed[0] = 0;
			Flow4BarCount[0] = 0;
			Flow4BarCycleTickSpeed[0] = 0;
		}

		public int GetStingRaySysCheck()
		{
			const int SysCheckValue = -1;
			return SysCheckValue;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[Range(2, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Strength", GroupName = "Parameters", Order = 0)]
		public int Strength
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "KeepBrokenLines", GroupName = "Parameters", Order = 1)]
		public bool KeepBrokenLines
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "EnableAlerts", GroupName = "Parameters", Order = 2)]
		public bool EnableAlerts
		{ get; set; }


		[Display(Name = "SoundsOn", GroupName = "Parameters", Order = 5)]
		public bool SoundsOn
		{ get; set; }

		[Display(Name = "Upfile", GroupName = "Parameters", Order = 6)]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter = "Wav Files (*.wav)|*.wav")]
		public string Upfile
		{ get; set; }

		[Display(Name = "Downfile", GroupName = "Parameters", Order = 7)]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter = "Wav Files (*.wav)|*.wav")]
		public string Downfile
		{ get; set; }


		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "CreeperPeriod1", GroupName = "Parameters", Order = 10)]
		public int CreeperPeriod1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "CreeperPeriod2", GroupName = "Parameters", Order = 11)]
		public int CreeperPeriod2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "CreeperPeriod3", GroupName = "Parameters", Order = 12)]
		public int CreeperPeriod3
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "RushLinePeriod", GroupName = "Parameters", Order = 13)]
		public int RushLinePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "DriftLine1Period", GroupName = "Parameters", Order = 14)]
		public int DriftLine1Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "DriftLine2Period", GroupName = "Parameters", Order = 15)]
		public int DriftLine2Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "DriftLine3Period", GroupName = "Parameters", Order = 16)]
		public int DriftLine3Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "DriftLine4Period", GroupName = "Parameters", Order = 17)]
		public int DriftLine4Period
		{ get; set; }



		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHighLine
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHighBrokenLine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHighNonCLine
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLowLine
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLowBrokenLine
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLowNonCLine
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHigh5Level
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHigh4Level
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHigh3Level
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHigh2Level
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingHigh1Level
		{
			get { return Values[10]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLow1Level
		{
			get { return Values[11]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLow2Level
		{
			get { return Values[12]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLow3Level
		{
			get { return Values[13]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLow4Level
		{
			get { return Values[14]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SwingLow5Level
		{
			get { return Values[15]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RecentHigherHigh
		{
			get { return Values[16]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RecentLowerLow
		{
			get { return Values[17]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CreeperCycleCount
		{
			get { return Values[18]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SurgeBarCount
		{
			get { return Values[19]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SurgeBarCycleTickSpeed
		{
			get { return Values[20]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Gush1BarCount
		{
			get { return Values[21]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Gush1BarCycleTickSpeed
		{
			get { return Values[22]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow1BarCount
		{
			get { return Values[23]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow1BarCycleTickSpeed
		{
			get { return Values[24]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow2BarCount
		{
			get { return Values[25]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow2BarCycleTickSpeed
		{
			get { return Values[26]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow3BarCount
		{
			get { return Values[27]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow3BarCycleTickSpeed
		{
			get { return Values[28]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow4BarCount
		{
			get { return Values[29]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Flow4BarCycleTickSpeed
		{
			get { return Values[30]; }
		}


		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunterTA.THStingRay[] cacheTHStingRay;
		public TickHunterTA.THStingRay THStingRay(string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			return THStingRay(Input, indicatorName, strength, keepBrokenLines, enableAlerts, creeperPeriod1, creeperPeriod2, creeperPeriod3, rushLinePeriod, driftLine1Period, driftLine2Period, driftLine3Period, driftLine4Period);
		}

		public TickHunterTA.THStingRay THStingRay(ISeries<double> input, string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			if (cacheTHStingRay != null)
				for (int idx = 0; idx < cacheTHStingRay.Length; idx++)
					if (cacheTHStingRay[idx] != null && cacheTHStingRay[idx].IndicatorName == indicatorName && cacheTHStingRay[idx].Strength == strength && cacheTHStingRay[idx].KeepBrokenLines == keepBrokenLines && cacheTHStingRay[idx].EnableAlerts == enableAlerts && cacheTHStingRay[idx].CreeperPeriod1 == creeperPeriod1 && cacheTHStingRay[idx].CreeperPeriod2 == creeperPeriod2 && cacheTHStingRay[idx].CreeperPeriod3 == creeperPeriod3 && cacheTHStingRay[idx].RushLinePeriod == rushLinePeriod && cacheTHStingRay[idx].DriftLine1Period == driftLine1Period && cacheTHStingRay[idx].DriftLine2Period == driftLine2Period && cacheTHStingRay[idx].DriftLine3Period == driftLine3Period && cacheTHStingRay[idx].DriftLine4Period == driftLine4Period && cacheTHStingRay[idx].EqualsInput(input))
						return cacheTHStingRay[idx];
			return CacheIndicator<TickHunterTA.THStingRay>(new TickHunterTA.THStingRay(){ IndicatorName = indicatorName, Strength = strength, KeepBrokenLines = keepBrokenLines, EnableAlerts = enableAlerts, CreeperPeriod1 = creeperPeriod1, CreeperPeriod2 = creeperPeriod2, CreeperPeriod3 = creeperPeriod3, RushLinePeriod = rushLinePeriod, DriftLine1Period = driftLine1Period, DriftLine2Period = driftLine2Period, DriftLine3Period = driftLine3Period, DriftLine4Period = driftLine4Period }, input, ref cacheTHStingRay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunterTA.THStingRay THStingRay(string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			return indicator.THStingRay(Input, indicatorName, strength, keepBrokenLines, enableAlerts, creeperPeriod1, creeperPeriod2, creeperPeriod3, rushLinePeriod, driftLine1Period, driftLine2Period, driftLine3Period, driftLine4Period);
		}

		public Indicators.TickHunterTA.THStingRay THStingRay(ISeries<double> input , string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			return indicator.THStingRay(input, indicatorName, strength, keepBrokenLines, enableAlerts, creeperPeriod1, creeperPeriod2, creeperPeriod3, rushLinePeriod, driftLine1Period, driftLine2Period, driftLine3Period, driftLine4Period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunterTA.THStingRay THStingRay(string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			return indicator.THStingRay(Input, indicatorName, strength, keepBrokenLines, enableAlerts, creeperPeriod1, creeperPeriod2, creeperPeriod3, rushLinePeriod, driftLine1Period, driftLine2Period, driftLine3Period, driftLine4Period);
		}

		public Indicators.TickHunterTA.THStingRay THStingRay(ISeries<double> input , string indicatorName, int strength, bool keepBrokenLines, bool enableAlerts, int creeperPeriod1, int creeperPeriod2, int creeperPeriod3, int rushLinePeriod, int driftLine1Period, int driftLine2Period, int driftLine3Period, int driftLine4Period)
		{
			return indicator.THStingRay(input, indicatorName, strength, keepBrokenLines, enableAlerts, creeperPeriod1, creeperPeriod2, creeperPeriod3, rushLinePeriod, driftLine1Period, driftLine2Period, driftLine3Period, driftLine4Period);
		}
	}
}

#endregion
