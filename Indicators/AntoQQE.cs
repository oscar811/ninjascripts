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
	public class AntoQQE : Indicator
	{
		Series<double> Rsi;
		Series<double> RsiMa;
		Series<double> AtrRsi;
		Series<double> MatrRsi;
		Series<double> dar;
		Series<double> shortband;
		Series<double> longband;
		Series<double> trend;
		
		double newShortband =0.0;
		double newLongband =0.0;
		

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AntoQQE";
				RSI_Period = 6;
				SF = 6;
				QQE = 4.238;
				Threshhold = 10;
				thicknes = 4; 
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
//				AddPlot(new Stroke(Brushes.Blue, 3), PlotStyle.Line, "plot1t");
//				AddPlot(new Stroke(Brushes.Orange, 3), PlotStyle.Line, "plot2t");
//				AddPlot(new Stroke(Brushes.DodgerBlue, thicknes),	PlotStyle.Bar,	"plot3t");
//				AddLine(Brushes.Lime,					Threshhold,				"Up");
//				AddLine(Brushes.Red,					-Threshhold,				"Down");
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
			}
			else if (State == State.Configure)
			{
				Rsi = new Series<double>(this);
				RsiMa = new Series<double>(this);
				AtrRsi = new Series<double>(this);
				MatrRsi = new Series<double>(this);
				dar = new Series<double>(this);
				
				shortband  = new Series<double>(this);
				longband = new Series<double>(this);
				trend = new Series<double>(this);
				
				AddPlot(new Stroke(Brushes.Black, 3), PlotStyle.Line, "plot1t");
				AddPlot(new Stroke(Brushes.Black, 3), PlotStyle.Line, "plot2t");
				//AddPlot(new Stroke(Brushes.Blue, 3), PlotStyle.Line, "plot1t");
				//AddPlot(new Stroke(Brushes.Orange, 3), PlotStyle.Line, "plot2t");
				AddPlot(new Stroke(Brushes.DodgerBlue, thicknes),	PlotStyle.Bar,	"plot3t");
				AddLine(Brushes.Blue,					Threshhold,				"Up");
				AddLine(Brushes.Red,					-Threshhold,				"Down");
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if(CurrentBar == 1){				
				RsiMa[0] = 0;
				longband[0] = 0;
				shortband[0] = 0;
				trend[0] = 0;
			}

			if(CurrentBar < 2*RSI_Period -1)
				return;
			
			Rsi[0] =  RSI(Close, RSI_Period,1)[0];
			RsiMa[0] = EMA(Rsi, SF)[0];
			AtrRsi[0] = Math.Abs(RsiMa[1]-RsiMa[0]) ;
			MatrRsi[0] = EMA(AtrRsi,2*RSI_Period -1 )[0];
			dar[0] = EMA(MatrRsi, 2*RSI_Period -1)[0]*QQE;
			
			
			newShortband = RsiMa[0]  + dar[0];
			newLongband =  RsiMa[0]  - dar[0];
			
			if(RsiMa[1] > longband[1] && RsiMa[0] > longband[1] ){				
				
				longband[0] = Math.Max(longband[1],newLongband);
				
			}
			else{				
				
				longband[0] = newLongband;
				
			}
			
			if(RsiMa[1] < shortband[1] && RsiMa[0] < shortband[1] ){				
				
				shortband[0] = Math.Min(shortband[1],newShortband);
				
			}
			else{				
				
				shortband[0] = newShortband;
				
			}
			
			if((RsiMa[0] > longband[1] &&  RsiMa[1] < longband[1])   | ( RsiMa[0] < longband[1] && RsiMa[1] > longband[1])){				
				trend[0] = 1;
				}
			else if((RsiMa[0] > shortband[1] && RsiMa[1] < shortband[1])   | (RsiMa[0] < shortband[1] && RsiMa[1] > shortband[1])){				
				trend[0] = -1;
				}
			
			if(trend[1] != 0){	
					trend[0] =  trend[1];
				
				}
				else
					trend[0] = 1;


			if(trend[0] == 1){				
			    Rsi_index1[0] = longband[0]-50;
				FastAtrrsi1[0]  = RsiMa[0]-50;
				
				}
			else{				
			    Rsi_index1[0] = shortband[0]-50;
				FastAtrrsi1[0]  = RsiMa[0]-50;
				}
			
			
			hist[0] =  RsiMa[0]-50;
			if(hist[0] < 10 && hist[0] > -10  ){
				PlotBrushes[2][0] = Brushes.Gray ;
			    BarBrush = Brushes.Orange;
				CandleOutlineBrushes[0] = BarBrush;
				}
			else if (hist[0] >= 10 ){
				PlotBrushes[2][0] = Brushes.Blue ;
				BarBrush = Brushes.Lime;
				CandleOutlineBrushes[0] = BarBrush; 
				}
			else{
				PlotBrushes[2][0] = Brushes.Red ;
				BarBrush = Brushes.Red;
				CandleOutlineBrushes[0] = BarBrush;
				}
			
		}
		
#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Rsi_index1
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FastAtrrsi1
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> hist
		{
			get { return Values[2]; }
		}
		
		
		
		[NinjaScriptProperty]
        [Description("RSi Period")]
        [Category("Parameters")]		
		[Display(Name="Rsi Period", Order=0, GroupName="Parameters")]
		public int RSI_Period	
		{ get; set; }	
		
		[NinjaScriptProperty]
        [Description("SF")]
        [Category("Parameters")]		
		[Display(Name="SF", Order=1, GroupName="Parameters")]
		public int SF
		{ get; set; }		
		
		[NinjaScriptProperty]
        [Description("QQE")]
        [Category("Parameters")]		
		[Display(Name="QQE", Order=2, GroupName="Parameters")]
		public double QQE
		{ get; set; }		
		
		[NinjaScriptProperty]
        [Description("threshold")]
        [Category("Parameters")]		
		[Display(Name="Threshold", Order=3, GroupName="Parameters")]
		public int Threshhold
		{ get; set; }
		
		
		[NinjaScriptProperty]
        [Description("thicknes histogram")]
        [Category("Parameters")]		
		[Display(Name="thicknes histogram", Order=4, GroupName="Parameters")]
		public int thicknes
		{ get; set; }

		
#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AntoQQE[] cacheAntoQQE;
		public AntoQQE AntoQQE(int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			return AntoQQE(Input, rSI_Period, sF, qQE, threshhold, thicknes);
		}

		public AntoQQE AntoQQE(ISeries<double> input, int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			if (cacheAntoQQE != null)
				for (int idx = 0; idx < cacheAntoQQE.Length; idx++)
					if (cacheAntoQQE[idx] != null && cacheAntoQQE[idx].RSI_Period == rSI_Period && cacheAntoQQE[idx].SF == sF && cacheAntoQQE[idx].QQE == qQE && cacheAntoQQE[idx].Threshhold == threshhold && cacheAntoQQE[idx].thicknes == thicknes && cacheAntoQQE[idx].EqualsInput(input))
						return cacheAntoQQE[idx];
			return CacheIndicator<AntoQQE>(new AntoQQE(){ RSI_Period = rSI_Period, SF = sF, QQE = qQE, Threshhold = threshhold, thicknes = thicknes }, input, ref cacheAntoQQE);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AntoQQE AntoQQE(int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			return indicator.AntoQQE(Input, rSI_Period, sF, qQE, threshhold, thicknes);
		}

		public Indicators.AntoQQE AntoQQE(ISeries<double> input , int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			return indicator.AntoQQE(input, rSI_Period, sF, qQE, threshhold, thicknes);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AntoQQE AntoQQE(int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			return indicator.AntoQQE(Input, rSI_Period, sF, qQE, threshhold, thicknes);
		}

		public Indicators.AntoQQE AntoQQE(ISeries<double> input , int rSI_Period, int sF, double qQE, int threshhold, int thicknes)
		{
			return indicator.AntoQQE(input, rSI_Period, sF, qQE, threshhold, thicknes);
		}
	}
}

#endregion
