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
using RSqueezeTypes;
#endregion

namespace RSqueezeTypes
{
		public enum RSqueezeStyle
		{
			BBSqueeze,
			PBFSqueeze,
			CounterTrend
		}
}

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RSqueeze : Indicator
	{
		private bool alertArmed = false; // sound alert control flag for COBC=false
		
        private double nBB = 2;
        private double nK = 1.5;
		private double bbs = 0;
		private int gaussperiod = 21;
		private int gausspoles = 3;
		private int upct, downct;

		private DonchianChannel DC;
		private LinReg			LR;
		private StdDev			SD;
		private ATR				atr_;
		private EMA				ema_;
		private CCI				cci_;
		
		private RGaussianFilter gf8, gf13, gf21, gf34, gf55, gf89, gfgg;
        private double d1, d2, d3, d4;
		
		private Series<double> lrm_;
		private Series<double> medianPriceOsc_;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"RSqueeze: Bollinger Band and PFB Squeeze Types.";
				Name						= "RSqueeze";
				
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= false;
				DrawHorizontalGridLines		= false;
				DrawVerticalGridLines		= false;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				
				SqueezeStyle				= RSqueezeStyle.BBSqueeze;
				MomentumLength				= 20;
				SqueezeLength				= 20;
				CCILength					= 13;
				GaussPolesPBF				= 3;
				GaussPolesCT				= 4;
				
				SqueezeDotBrush				= Brushes.Red;
				NormalDotBrush				= Brushes.Blue;
				PBFBullishDotBrush			= Brushes.Green;
				PBFBearishDotBrush			= Brushes.Red;
				HistAboveZeroRising			= Brushes.Lime;
				HistAboveZeroFalling		= Brushes.DarkSlateGray;
				HistBelowZeroFalling		= Brushes.Red;
				HistBelowZeroRising			= Brushes.DarkSlateGray;
				
				SoundAlertsOn				= false;
				BuySoundAlert				= "Alert3.wav";
				SellSoundAlert				= "Alert4.wav";
				
				AddPlot(new Stroke(Brushes.DarkSlateGray, 5), PlotStyle.Bar, "MomentumHistogram");
				AddPlot(new Stroke(NormalDotBrush, 2), PlotStyle.Dot, "SqueezeDots");
			}
			else if (State == State.Configure)
			{
				
				switch (SqueezeStyle)
				{
					case(RSqueezeStyle.BBSqueeze):
						lrm_ = new Series<double>(this);
						medianPriceOsc_ = new Series<double>(this);
						atr_ = ATR(SqueezeLength);
						SD = StdDev(SqueezeLength);
						DC = DonchianChannel(MomentumLength);
						LR = LinReg(medianPriceOsc_, MomentumLength);
						ema_ = EMA(MomentumLength);
						break;
					
					case (RSqueezeStyle.PBFSqueeze):
						gf8  = RGaussianFilter(Typical, GaussPolesPBF,  8);
						gf13 = RGaussianFilter(Typical, GaussPolesPBF, 13);
						gf21 = RGaussianFilter(Typical, GaussPolesPBF, 21);
						gf34 = RGaussianFilter(Typical, GaussPolesPBF, 34);
						gf55 = RGaussianFilter(Typical, GaussPolesPBF, 55);
						gf89 = RGaussianFilter(Typical, GaussPolesPBF, 89);
						gfgg = RGaussianFilter(Typical, gausspoles, gaussperiod);
						break;
						
					case (RSqueezeStyle.CounterTrend):
						cci_ = CCI(CCILength);
						gf8  = RGaussianFilter(Typical, GaussPolesCT,  8);
						gf13 = RGaussianFilter(Typical, GaussPolesCT, 13);
						gf21 = RGaussianFilter(Typical, GaussPolesCT, 21);
						gf34 = RGaussianFilter(Typical, GaussPolesCT, 34);
						gf55 = RGaussianFilter(Typical, GaussPolesCT, 55);
						gf89 = RGaussianFilter(Typical, GaussPolesCT, 89);
						gfgg = RGaussianFilter(Typical, gausspoles, gaussperiod);
						break;
						
					default:
						break;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			switch (SqueezeStyle)
			{
				case(RSqueezeStyle.BBSqueeze):
					if (CurrentBar < MomentumLength) return;
			
					// Squeeze Dots
					SqueezeDots[0] = 0;
					if ((nK*atr_[0])!= 0)
						bbs = (nBB*SD[0])/(nK*atr_[0]);
					else
						bbs = 1;
					
					if (bbs <= 1)
						PlotBrushes[1][0] = SqueezeDotBrush;
					else
						PlotBrushes[1][0] = NormalDotBrush;
					
			
					// Momentum Histogram
					medianPriceOsc_[0] = Close[0]-((DC.Mean[0] + ema_[0]) / 2.0);
					lrm_[0] = LR[0];
					MomentumHistogram[0] = lrm_[0];
			
					// Histogram Colors
					if (lrm_[0] > 0)
					{
						if (lrm_[0] > lrm_[1])
							 PlotBrushes[0][0] = HistAboveZeroRising;
						else
							PlotBrushes[0][0] = HistAboveZeroFalling;
					}
					else
					{
						if (lrm_[0] < lrm_[1])
							PlotBrushes[0][0] = HistBelowZeroFalling;
						else
							PlotBrushes[0][0] = HistBelowZeroRising;
					}

                    //Print("PlotBrushes[1][0]: " + PlotBrushes[1][0]);

					break;
					
				case(RSqueezeStyle.PBFSqueeze):
					if (CurrentBar < gaussperiod) return;
					
					// Squeeze Dots
					SqueezeDots[0] = 0;
					if (gfgg[0] > gfgg[1])
						PlotBrushes[1][0] = PBFBullishDotBrush;
					else
						PlotBrushes[1][0] = PBFBearishDotBrush;
					
					//Momentum Histogram
                    d1 = gf8[0]  - gf21[0];
                    d2 = gf13[0] - gf34[0];
                    d3 = gf21[0] - gf55[0];
                    d4 = gf34[0] - gf89[0];
					MomentumHistogram[0] = (d1 + d2 + d3 + d4) / 4;
					
					// Histogram Colors
					if (MomentumHistogram[0] > 0)
					{
						if (MomentumHistogram[0] > MomentumHistogram[1]) 
							PlotBrushes[0][0] = HistAboveZeroRising;
						else
							PlotBrushes[0][0] = HistAboveZeroFalling;
					}
					else
					{
						if (MomentumHistogram[0] < MomentumHistogram[1]) 
							PlotBrushes[0][0] = HistBelowZeroFalling;
						else
							PlotBrushes[0][0] = HistBelowZeroRising;
					}
					break;
					
				case(RSqueezeStyle.CounterTrend):
					if (CurrentBar < gaussperiod) return;
					
					// Squeeze Dots
					SqueezeDots[0] = 0;
					if (gfgg[0] > gfgg[1])
						PlotBrushes[1][0] = PBFBullishDotBrush;
					else
						PlotBrushes[1][0] = PBFBearishDotBrush;
					
					//Momentum Histogram
                    d1 = gf8[0]  - gf21[0];
                    d2 = gf13[0] - gf34[0];
                    d3 = gf21[0] - gf55[0];
                    d4 = gf34[0] - gf89[0];
					MomentumHistogram[0] = (d1 + d2 + d3 + d4) / 4;
					
					// Histogram Colors
					if (MomentumHistogram[0] > 0)
					{
						if (MomentumHistogram[0] > MomentumHistogram[1]) 
							PlotBrushes[0][0] = HistAboveZeroRising;
						else
							PlotBrushes[0][0] = HistAboveZeroFalling;
					}
					else
					{
						if (MomentumHistogram[0] < MomentumHistogram[1]) 
							PlotBrushes[0][0] = HistBelowZeroFalling;
						else
							PlotBrushes[0][0] = HistBelowZeroRising;
					}
					
					if(cci_[1]>50 && cci_[0]<=50 && MomentumHistogram[0]>0 && MomentumHistogram[1]>0 && MomentumHistogram[2]>0) { PlotBrushes[0][0] = HistBelowZeroRising; downct = 1; }
					if(downct==1 && MomentumHistogram[0]>0 && cci_[0]<100) { PlotBrushes[0][0] = HistBelowZeroRising; }
					if(downct==1 && cci_[1]<100 && cci_[0]>100) { PlotBrushes[0][0] = HistAboveZeroRising; downct=0;}
					if(downct==1 && MomentumHistogram[0]<0) downct=0;
						
					if(cci_[1]<-50 && cci_[0]>-50 && MomentumHistogram[0]<0 && MomentumHistogram[1]<0 && MomentumHistogram[2]<0) { PlotBrushes[0][0] = HistAboveZeroFalling; upct = 1; }
					if(upct==1 && MomentumHistogram[0]>0) upct = 0;
					if(upct==1 && MomentumHistogram[0]<0 && cci_[0]>-100) { PlotBrushes[0][0] = HistAboveZeroFalling; }
					if(upct==1 && cci_[1]>-100 && cci_[0]<-100) { PlotBrushes[0][0] = HistBelowZeroFalling; upct=0; }
					break;
					
				default:
					break;
			}
			
			// Sound Alert
			if (SoundAlertsOn && (State != State.Historical))
			{
				if (IsFirstTickOfBar) alertArmed = true;
					
				if (alertArmed)
				{
					try
					{
						if (((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistAboveZeroRising)&&(PlotBrushes[1][1] == SqueezeDotBrush))
						||  ((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistAboveZeroRising)&&(PlotBrushes[0][1] != HistAboveZeroRising)))
						{
							PlaySound(string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, BuySoundAlert));
							alertArmed=false; 
						}
						else if (((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistBelowZeroFalling)&&(PlotBrushes[1][1] == SqueezeDotBrush))
							 ||  ((PlotBrushes[1][0] == NormalDotBrush)&&(PlotBrushes[0][0] == HistBelowZeroFalling)&&(PlotBrushes[0][1] != HistBelowZeroFalling)))
						{
							PlaySound(string.Format( @"{0}sounds\{1}", NinjaTrader.Core.Globals.InstallDir, SellSoundAlert));
							alertArmed=false; 
						}
					}
					catch(Exception sae){Print("RSqueeze:OnBarUpdate() Sound Alert Exception Thrown = " + sae.ToString() + "Bar Number = " + CurrentBar); return;}
				}
			}
		}

		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="RSqueeze Style", Description="Squeeze Indicator Style", Order=1, GroupName="Parameters")]
		public RSqueezeStyle SqueezeStyle
		{ get; set; }

		[Range(1, int.MaxValue)]
		[XmlIgnore]
		[Display(Name="MomentumLength", Description="Momentum Histogram Length", Order=1, GroupName="BB Squeeze")]
		public int MomentumLength
		{ get; set; }

		[Range(1, int.MaxValue)]
		[XmlIgnore]
		[Display(Name="SqueezeLength", Description="Volatility Bands Length", Order=2, GroupName="BB Squeeze")]
		public int SqueezeLength
		{ get; set; }

		[XmlIgnore]
		[Display(Name="SqueezeDotBrush", Description="Dot Color to indicate a Volatilty Squeeze", Order=3, GroupName="BB Squeeze")]
		public Brush SqueezeDotBrush
		{ get; set; }

		[Browsable(false)]
		public string SqueezeDotBrushSerializable
		{
			get { return Serialize.BrushToString(SqueezeDotBrush); }
			set { SqueezeDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="NormalDotBrush", Description="Normal Volatility Dot Color", Order=4, GroupName="BB Squeeze")]
		public Brush NormalDotBrush
		{ get; set; }

		[Browsable(false)]
		public string NormalDotBrushSerializable
		{
			get { return Serialize.BrushToString(NormalDotBrush); }
			set { NormalDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Range(1, 4)]
		[Display(Name="Gaussian Filter Poles PBF", Description="Gaussian Filter Poles for PBF Style", Order=1, GroupName="PBF")]
		public int GaussPolesPBF
		{ get; set; }

		[XmlIgnore]
		[Display(Name="PBFBullishDotBrush", Description="Dot Color for Bullish PBF Direction", Order=2, GroupName="PBF")]
		public Brush PBFBullishDotBrush
		{ get; set; }

		[Browsable(false)]
		public string PBFBullishDotBrushSerializable
		{
			get { return Serialize.BrushToString(PBFBullishDotBrush); }
			set { PBFBullishDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="PBFBearishDotBrush", Description="Dot Color for Bearish PBF Direction", Order=3, GroupName="PBF")]
		public Brush PBFBearishDotBrush
		{ get; set; }

		[Browsable(false)]
		public string PBFBearishDotBrushSerializable
		{
			get { return Serialize.BrushToString(PBFBearishDotBrush); }
			set { PBFBearishDotBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Range(1, int.MaxValue)]
		[Display(Name="CCI Period", Description="CCI Length for CounterTrend Style", Order=1, GroupName="PBF Counter Trend")]
		public int CCILength
		{ get; set; }

		[XmlIgnore]
		[Range(1, 4)]
		[Display(Name="Gaussian Filter Poles CT", Description="Gaussian Filter Poles for CounterTrend Style", Order=2, GroupName="PBF Counter Trend")]
		public int GaussPolesCT
		{ get; set; }

		[XmlIgnore]
		[Display(Name="HistAboveZeroRising", Description="Momentum Histogram Above Zero and Rising", Order=3, GroupName="Histogram Brushes")]
		public Brush HistAboveZeroRising
		{ get; set; }

		[Browsable(false)]
		public string HistAboveZeroRisingSerializable
		{
			get { return Serialize.BrushToString(HistAboveZeroRising); }
			set { HistAboveZeroRising = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistAboveZeroFalling", Description="Momentum Histogram Above Zero and Falling", Order=4, GroupName="Histogram Brushes")]
		public Brush HistAboveZeroFalling
		{ get; set; }

		[Browsable(false)]
		public string HistAboveZeroFallingSerializable
		{
			get { return Serialize.BrushToString(HistAboveZeroFalling); }
			set { HistAboveZeroFalling = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistBelowZeroFalling", Description="Momentum Histogram Below Zero and Falling", Order=5, GroupName="Histogram Brushes")]
		public Brush HistBelowZeroFalling
		{ get; set; }

		[Browsable(false)]
		public string HistBelowZeroFallingSerializable
		{
			get { return Serialize.BrushToString(HistBelowZeroFalling); }
			set { HistBelowZeroFalling = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="HistBelowZeroRising", Description="Momentum Histogram Below Zero and Rising", Order=6, GroupName="Histogram Brushes")]
		public Brush HistBelowZeroRising
		{ get; set; }

		[Browsable(false)]
		public string HistBelowZeroRisingSerializable
		{
			get { return Serialize.BrushToString(HistBelowZeroRising); }
			set { HistBelowZeroRising = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="SoundAlertsOn", Description="Enables Sound Alerts", Order=1, GroupName="Sound")]
		public bool SoundAlertsOn
		{ get; set; }

		[XmlIgnore]
		[Display(Name="BuySoundAlert", Description="Buy Sound Alert .wav filename", Order=2, GroupName="Sound")]
		public string BuySoundAlert
		{ get; set; }

		[XmlIgnore]
		[Display(Name="SellSoundAlert", Description="Sell Sound Alert .wav filename", Order=3, GroupName="Sound")]
		public string SellSoundAlert
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MomentumHistogram
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SqueezeDots
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RSqueeze[] cacheRSqueeze;
		public RSqueeze RSqueeze(RSqueezeStyle squeezeStyle)
		{
			return RSqueeze(Input, squeezeStyle);
		}

		public RSqueeze RSqueeze(ISeries<double> input, RSqueezeStyle squeezeStyle)
		{
			if (cacheRSqueeze != null)
				for (int idx = 0; idx < cacheRSqueeze.Length; idx++)
					if (cacheRSqueeze[idx] != null && cacheRSqueeze[idx].SqueezeStyle == squeezeStyle && cacheRSqueeze[idx].EqualsInput(input))
						return cacheRSqueeze[idx];
			return CacheIndicator<RSqueeze>(new RSqueeze(){ SqueezeStyle = squeezeStyle }, input, ref cacheRSqueeze);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSqueeze RSqueeze(RSqueezeStyle squeezeStyle)
		{
			return indicator.RSqueeze(Input, squeezeStyle);
		}

		public Indicators.RSqueeze RSqueeze(ISeries<double> input , RSqueezeStyle squeezeStyle)
		{
			return indicator.RSqueeze(input, squeezeStyle);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSqueeze RSqueeze(RSqueezeStyle squeezeStyle)
		{
			return indicator.RSqueeze(Input, squeezeStyle);
		}

		public Indicators.RSqueeze RSqueeze(ISeries<double> input , RSqueezeStyle squeezeStyle)
		{
			return indicator.RSqueeze(input, squeezeStyle);
		}
	}
}

#endregion
