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
	public class RGaussianFilter : Indicator
	{
		double _w;
		double _aa, _a2, _a3, _a4, _c1, _c2, _c3, _c4;
		double _b;
		double r, y;
		
		const double Pi = 22 / 7;
		double Sqrt2 = Math.Sqrt(2.0);

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Gaussian Filter Ported to NT8";
				Name								= "RGaussianFilter";
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= false;
				DrawVerticalGridLines				= false;
				PaintPriceMarkers					= false;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= true;
				
				Poles					= 2;
				Period					= 20;
				
				AddPlot(Brushes.DarkViolet, "Gauss");
			}
			
			else if (State == State.Configure )
			{
              	_w = 2 * Pi / Period;
                _b = (1 - Math.Cos(_w)) / (Math.Pow(Sqrt2, 2.0 / Poles) - 1);
				if (Period == 1)
					_aa = 1.0;
				else
                	_aa = -_b + Math.Sqrt(_b * (_b + 2));
                _c1 = 1.0 - _aa;
                _c2 = _c1 * _c1;
                _c3 = _c2 * _c1;
                _c4 = _c3 * _c1;
                _a2 = _aa * _aa;
                _a3 = _a2 * _aa;
                _a4 = _a3 * _aa;
			}
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < Poles)
            {
				Value[0] = Input[0];
                return;
            }

            if (IsFirstTickOfBar)
            {
				switch (Poles)
				{
					case 1:
						r = _c1*Value[1];
						break;
					case 2:
						r = 2 *_c1*Value[1] - _c2*Value[2];
						break;
					case 3:
						r = 3*_c1*Value[1] - 3*_c2*Value[2] + _c3*Value[3];
						break;
					case 4:
						r = 4*_c1*Value[1] - 6*_c2*Value[2] + 4*_c3*Value[3] - _c4*Value[4];
						break;
					default:
						break;
				}
			}
			
           switch (Poles)
            {
                case 1:
                    y = _aa * Input[0];
					break;
                case 2:
                    y = _a2 * Input[0];
					break;
                case 3:
                    y = _a3 * Input[0];
					break;
                case 4:
                    y = _a4 * Input[0];
					break;
				default:
					break;
            }
            Value[0] = y + r;
		}

		#region Properties
		
		[Range(1, 4)]
		[NinjaScriptProperty]
		[Display(Name="Poles", Order=1, GroupName="Parameters")]
		public int Poles
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Order=2, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Gauss
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
		private RGaussianFilter[] cacheRGaussianFilter;
		public RGaussianFilter RGaussianFilter(int poles, int period)
		{
			return RGaussianFilter(Input, poles, period);
		}

		public RGaussianFilter RGaussianFilter(ISeries<double> input, int poles, int period)
		{
			if (cacheRGaussianFilter != null)
				for (int idx = 0; idx < cacheRGaussianFilter.Length; idx++)
					if (cacheRGaussianFilter[idx] != null && cacheRGaussianFilter[idx].Poles == poles && cacheRGaussianFilter[idx].Period == period && cacheRGaussianFilter[idx].EqualsInput(input))
						return cacheRGaussianFilter[idx];
			return CacheIndicator<RGaussianFilter>(new RGaussianFilter(){ Poles = poles, Period = period }, input, ref cacheRGaussianFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RGaussianFilter RGaussianFilter(int poles, int period)
		{
			return indicator.RGaussianFilter(Input, poles, period);
		}

		public Indicators.RGaussianFilter RGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.RGaussianFilter(input, poles, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RGaussianFilter RGaussianFilter(int poles, int period)
		{
			return indicator.RGaussianFilter(Input, poles, period);
		}

		public Indicators.RGaussianFilter RGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.RGaussianFilter(input, poles, period);
		}
	}
}

#endregion
