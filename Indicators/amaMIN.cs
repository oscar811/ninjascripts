//+----------------------------------------------------------------------------------------------+
//| Copyright © <2017>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The indicator calculates the minimum value for the selected input series over the lookback period.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 0)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Set up", 30)]
	[Gui.CategoryOrder("Visual", 40)]
	[Gui.CategoryOrder("Plots", 50)]
	[Gui.CategoryOrder("Version", 80)]
	public class amaMIN : Indicator
	{
		private int					period						= 14;
		private int					lookback					= 0;
		private int					index						= 0;
		private int					priorIndex					= 0;
		private double				min 						= 0.0;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.0  -  July 31, 2017";
		private Series<int>			minIndex;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The indicator calculates the minimum value for the selected input series over the lookback period.";
				Name						= "amaMIN";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "MinValue");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = period;
			}
			else if (State == State.DataLoaded)
			{
				minIndex = new Series<int>(this, MaximumBarsLookBack.Infinite);
			}	
			else if (State == State.Historical)
			{
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
            if(CurrentBar == 0 || period == 1)
			{
				minIndex[0] = 0;
				MinValue[0] = Input[0];
				return;
			}	
			else if (CurrentBar < period)	
			{	
				if(IsFirstTickOfBar)
				{	
					lookback = Math.Min(period, CurrentBar + 1);
					index = 1;
					min = Input[1];
					for(int i = 2; i < lookback; i++)
					{
						if(Input[i] < min)
						{	
							index = i;
							min = Input[i];
						}
					}
				}
				if(Input[0] < min)
				{	
					minIndex[0] = 0;
					MinValue[0] = Input[0];
				}
				else
				{
					minIndex[0] = index;
					MinValue[0] = min;
				}	
			}
			else
			{
				if(IsFirstTickOfBar)
				{
					priorIndex = MinIndex[1];
					if(priorIndex < period - 1)
					{
						index = priorIndex + 1;
						min = MinValue[1];
					}
					else
					{
						index = 1;
						min = Input[1];
						for(int i = 2; i < period; i++)
						{
							if(Input[i] < min)
							{	
								index = i;
								min = Input[i];
							}
						}
					}
				}	
				if(Input[0] < min)
				{	
					minIndex[0] = 0;
					MinValue[0] = Input[0];
				}
				else
				{
					minIndex[0] = index;
					MinValue[0] = min;
				}	
			}	
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MinValue
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> MinIndex
		{
			get { return minIndex; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous
		
		public override string FormatPriceMarker(double price)
		{
			if(indicatorIsOnPricePanel)
				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			else
				return base.FormatPriceMarker(price);
		}			
		#endregion	
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaMIN[] cacheamaMIN;
		public LizardIndicators.amaMIN amaMIN(int period)
		{
			return amaMIN(Input, period);
		}

		public LizardIndicators.amaMIN amaMIN(ISeries<double> input, int period)
		{
			if (cacheamaMIN != null)
				for (int idx = 0; idx < cacheamaMIN.Length; idx++)
					if (cacheamaMIN[idx] != null && cacheamaMIN[idx].Period == period && cacheamaMIN[idx].EqualsInput(input))
						return cacheamaMIN[idx];
			return CacheIndicator<LizardIndicators.amaMIN>(new LizardIndicators.amaMIN(){ Period = period }, input, ref cacheamaMIN);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaMIN amaMIN(int period)
		{
			return indicator.amaMIN(Input, period);
		}

		public Indicators.LizardIndicators.amaMIN amaMIN(ISeries<double> input , int period)
		{
			return indicator.amaMIN(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaMIN amaMIN(int period)
		{
			return indicator.amaMIN(Input, period);
		}

		public Indicators.LizardIndicators.amaMIN amaMIN(ISeries<double> input , int period)
		{
			return indicator.amaMIN(input, period);
		}
	}
}

#endregion
