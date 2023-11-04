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
	public class RMA : Indicator
	{
        private SMA SMA1;
        //private Series<double> Sum;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"RMA";
				Name										= "RMA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                Length = 9;
                AddPlot(Brushes.Red, "RMA");
            }
			else if (State == State.Configure)
			{
                ClearOutputWindow();

                SMA1 = SMA(Input, Length);                
			}
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < Length)
                return;
            
            double alpha = 1.0 / Length;
            Value[0] = Value.Count == 0 ? SMA1[0] : alpha * Input[0] + (1 - alpha) * Value[1];
		}

        [NinjaScriptProperty]
        [Display(Name = "Length", Order = 1, GroupName = "Parameters")]
        public int Length { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.RMA[] cacheRMA;
		public RajIndicators.RMA RMA(int length)
		{
			return RMA(Input, length);
		}

		public RajIndicators.RMA RMA(ISeries<double> input, int length)
		{
			if (cacheRMA != null)
				for (int idx = 0; idx < cacheRMA.Length; idx++)
					if (cacheRMA[idx] != null && cacheRMA[idx].Length == length && cacheRMA[idx].EqualsInput(input))
						return cacheRMA[idx];
			return CacheIndicator<RajIndicators.RMA>(new RajIndicators.RMA(){ Length = length }, input, ref cacheRMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.RMA RMA(int length)
		{
			return indicator.RMA(Input, length);
		}

		public Indicators.RajIndicators.RMA RMA(ISeries<double> input , int length)
		{
			return indicator.RMA(input, length);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.RMA RMA(int length)
		{
			return indicator.RMA(Input, length);
		}

		public Indicators.RajIndicators.RMA RMA(ISeries<double> input , int length)
		{
			return indicator.RMA(input, length);
		}
	}
}

#endregion
