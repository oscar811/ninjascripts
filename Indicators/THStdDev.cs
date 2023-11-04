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
	/// The indicator calculates the standard deviation from the input series over the selected lookback period. 
	/// This indicator uses a faster algorithm than the standard deviation indicator that ships with NinjaTrader.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 0)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Set up", 30)]
	[Gui.CategoryOrder("Visual", 40)]
	[Gui.CategoryOrder("Plots", 50)]
	[Gui.CategoryOrder("Version", 80)]
	public class THStdDev : Indicator
	{
		private const string SystemVersion = "v1.016";
		private const string SystemName = "THStdDev";
		private const string FullSystemName = SystemName + " - " + SystemVersion;

		private double 				priorMean			= 0.0;
		private double				priorSquareMean		= 0.0;
		private double				diff				= 0.0;
		private Series<double>		squares;
		private Series<double>		mean;
		private Series<double>		squareMean;

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
				IsSuspendedWhileInactive	= true;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "Std Dev");

				this.Period = 14;
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= Period;
			}
			else if (State == State.DataLoaded)
			{
				squares = new Series<double>(this, Period < 250 ? MaximumBarsLookBack.TwoHundredFiftySix : MaximumBarsLookBack.Infinite);
				mean = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				squareMean = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
			}	
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar == 0)
			{
				squares[0] = Input[0]*Input[0];
				mean[0] = Input[0];
				squareMean[0] = squares[0];
				StdDev[0] = 0.0;
				return;
			}
			if(IsFirstTickOfBar)
			{
				priorMean = mean[1];
				priorSquareMean = squareMean[1];
			}	
			squares[0] = Input[0]*Input[0];
			if(CurrentBar < Period)
			{
				mean[0] = (CurrentBar * priorMean + Input[0])/(CurrentBar + 1);
				squareMean[0] = (CurrentBar * priorSquareMean + squares[0])/(CurrentBar + 1);
			}
			else
			{
				mean[0] = priorMean + (Input[0] - Input[Period])/ Period;
				squareMean[0] = priorSquareMean + (squares[0] - squares[Period])/ Period;
			}	
			diff = squareMean[0] - mean[0]*mean[0];
			if(diff > 0) StdDev[0] = Math.Sqrt(diff);
			else StdDev[0] = 0.0;
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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Parameters", Order = 0)]
		[Range(1, int.MaxValue)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StdDev
		{
			get { return Values[0]; }
		}

		
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunterTA.THStdDev[] cacheTHStdDev;
		public TickHunterTA.THStdDev THStdDev(string indicatorName, int period)
		{
			return THStdDev(Input, indicatorName, period);
		}

		public TickHunterTA.THStdDev THStdDev(ISeries<double> input, string indicatorName, int period)
		{
			if (cacheTHStdDev != null)
				for (int idx = 0; idx < cacheTHStdDev.Length; idx++)
					if (cacheTHStdDev[idx] != null && cacheTHStdDev[idx].IndicatorName == indicatorName && cacheTHStdDev[idx].Period == period && cacheTHStdDev[idx].EqualsInput(input))
						return cacheTHStdDev[idx];
			return CacheIndicator<TickHunterTA.THStdDev>(new TickHunterTA.THStdDev(){ IndicatorName = indicatorName, Period = period }, input, ref cacheTHStdDev);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunterTA.THStdDev THStdDev(string indicatorName, int period)
		{
			return indicator.THStdDev(Input, indicatorName, period);
		}

		public Indicators.TickHunterTA.THStdDev THStdDev(ISeries<double> input , string indicatorName, int period)
		{
			return indicator.THStdDev(input, indicatorName, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunterTA.THStdDev THStdDev(string indicatorName, int period)
		{
			return indicator.THStdDev(Input, indicatorName, period);
		}

		public Indicators.TickHunterTA.THStdDev THStdDev(ISeries<double> input , string indicatorName, int period)
		{
			return indicator.THStdDev(input, indicatorName, period);
		}
	}
}

#endregion
