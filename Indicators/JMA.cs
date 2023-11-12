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
	public class JMA : Indicator
	{
		
		public	Series<double> e0;
		public	Series<double> e1;
		public	Series<double> e2;
		public	Series<double> jm;
		double phaseratio;
		double beta;
		double alpha;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "JMA";
				jurik_phase = 3;
				jurik_power = 1;
				len =  60;
				
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
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "jma");
			}
			else if (State == State.Configure)
			{
				e0 = new Series<double>(this);
				e1 = new Series<double>(this);
				e2 = new Series<double>(this);
				jm = new Series<double>(this);
				e0[0] = 0;
				e1[0] = 0;
				e2[0] = 0;
				jm[0] = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if(CurrentBar< len)
				return;
			
			if( jurik_phase < -100)
				phaseratio = 0.5;
			else if( jurik_phase > 100)
				phaseratio = 2.5;
			else
				phaseratio = (jurik_phase/100)+ 0.5;
			
			
			beta = 0.45 *(len -1)/(0.45*(len-1) +2	);
			alpha = Math.Pow(beta,jurik_power);
			
			
			e0[0] = (1- alpha )*Input[0] + alpha*(e0[1]);
			
			e1[0] = (Input[0] - e0[0] )*(1-beta) + beta*(e1[1]);
			e2[0] = (e0[0]+phaseratio*e1[0] -jm[1] )*Math.Pow(1-alpha, 2)  + Math.Pow(alpha,2)*e2[1];
			
			jm[0] = e2[0] + jm[1];
			Value[0] =  jm[0];
			
			
			
			
			
				
			
		}
		
		#region Properties
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "jurik_phase", GroupName = "NinjaScriptParameters", Order = 0)]
		public int jurik_phase
		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "jurik_power", GroupName = "NinjaScriptParameters", Order = 1)]
		public int jurik_power
		{ get; set; }
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "len", GroupName = "NinjaScriptParameters", Order = 2)]
		public int len
		{ get; set; }
		
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JMA[] cacheJMA;
		public JMA JMA(int jurik_phase, int jurik_power, int len)
		{
			return JMA(Input, jurik_phase, jurik_power, len);
		}

		public JMA JMA(ISeries<double> input, int jurik_phase, int jurik_power, int len)
		{
			if (cacheJMA != null)
				for (int idx = 0; idx < cacheJMA.Length; idx++)
					if (cacheJMA[idx] != null && cacheJMA[idx].jurik_phase == jurik_phase && cacheJMA[idx].jurik_power == jurik_power && cacheJMA[idx].len == len && cacheJMA[idx].EqualsInput(input))
						return cacheJMA[idx];
			return CacheIndicator<JMA>(new JMA(){ jurik_phase = jurik_phase, jurik_power = jurik_power, len = len }, input, ref cacheJMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JMA JMA(int jurik_phase, int jurik_power, int len)
		{
			return indicator.JMA(Input, jurik_phase, jurik_power, len);
		}

		public Indicators.JMA JMA(ISeries<double> input , int jurik_phase, int jurik_power, int len)
		{
			return indicator.JMA(input, jurik_phase, jurik_power, len);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JMA JMA(int jurik_phase, int jurik_power, int len)
		{
			return indicator.JMA(Input, jurik_phase, jurik_power, len);
		}

		public Indicators.JMA JMA(ISeries<double> input , int jurik_phase, int jurik_power, int len)
		{
			return indicator.JMA(input, jurik_phase, jurik_power, len);
		}
	}
}

#endregion
