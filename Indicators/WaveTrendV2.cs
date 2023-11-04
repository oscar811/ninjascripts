// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
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
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// WaveTrend Oscillator with crossover indications.
	/// Version 1.0 2017 03 07
	/// by CPuzz
	/// 
	/// Version 2 2019 08-20
	/// Modified by NT support, added letters to draw method tag names to elimninate duplicate tag name errors
	/// </summary>
	public class WaveTrendV2 : Indicator
	{
		#region Variables
		
		#region Parameter and UI Variables
		//Properties Variables - Relevant to UI
		//
		public int	chanlen					= 10;
		public int	avglen					= 21;
		
		private double obupper					= 60;
		private double oblower					= 53;
		private double osupper					= -53;
		private double oslower					= -60;
		
		private bool zonedisplay			= true;
		private int zoneopacity				= 30;
		
		private Brush obfill				= Brushes.PaleVioletRed;
		private Brush obborder				= Brushes.Transparent;
				
		private Brush osfill				= Brushes.CornflowerBlue;
		private Brush osborder				= Brushes.Transparent;
		
		private bool codots					= true;
		private bool pbcoloring				= true;
		
		private bool zerocrosses			= true;
		private bool zlpb					= true;
		private bool fastavgcoloring		= true;
								
		private Brush cobull				= Brushes.SlateBlue;
		private Brush cobear				= Brushes.PaleVioletRed;
		private Brush zerocrossbullcolor	= Brushes.Green;
		private Brush zerocrossbearcolor	= Brushes.DarkGoldenrod;
		
		//private bool directionfiltering		= true;
		
		#endregion
		
		#region Internal Variables for Indicator
		//Code internal variables - Used only for indicator calculations
		//
		private bool above = true;
		
		private Series<double> AP;
		private Series<double> ESA;
		private Series<double> Dabsolute;
		private Series<double> D;
		private Series<double> CI;
		
		private Series<double>		levelOverBoughtUpper;
		private Series<double>		levelOverBoughtLower;
		private Series<double>		levelOverSoldUpper;
		private Series<double>		levelOverSoldLower;
		
		//private int bias;
		
		#endregion
		
		#endregion


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "WaveTrend is a fast moving MACD based oscillator that is quick to react on crossovers.";
				Name						= "WaveTrendV2";
				IsSuspendedWhileInactive	= true;
				//Ninjatrader only displays indicator calculation on most recent bar if CalculateOnBarclose = false
				Calculate					= Calculate.OnBarClose;
                IsOverlay					= false;
                DisplayInDataBox			= false;
                DrawOnPricePanel			= false;
                //DrawHorizontalGridLines		= true;
                //DrawVerticalGridLines		= true;
                //PaintPriceMarkers			= true;
                ScaleJustification			= ScaleJustification.Right;
				

				AddPlot(new Stroke(Brushes.Teal, 1), PlotStyle.Line, "WTFast");		//Plot 0, TCI
				AddPlot(new Stroke(Brushes.Black, 1), PlotStyle.Line, "WTSlow");	//Plot 1
				AddLine(Brushes.DarkSlateGray, 0, "Zero Line");							//Zero Line

			}
			else if (State == State.Configure)
			{
				AP			= new Series<double>(this);
				ESA			= new Series<double>(this);
				Dabsolute	= new Series<double>(this);
				D			= new Series<double>(this);
				CI			= new Series<double>(this);
				
				if (ZoneDisplay != false)
				{
					// NinjaTrader Series object can only store double
					//		Int cannot be used in Series to save memory
					levelOverBoughtUpper		= new Series<double>(this);
					levelOverBoughtLower		= new Series<double>(this);
				}
				
			}
		}

		
		protected override void OnBarUpdate()
		{
		// Zero out data series arrays on the first bar of input
			if (CurrentBar == 0) {	
				AP[0]			= Input[0];
				ESA[0]			= Input[0];
				Dabsolute[0]	= 0;
				D[0]			= 0;
				CI[0]			= 0;
				
				return;
			}
			
			else {
			// Main indicator Math calculations
				
				// Pine Script calculation	
				//
				//ap = hlc3 
				//esa = ema(ap, n1)
		    	//d = ema(abs(ap - esa), n1)
				//ci = (ap - esa) / (0.015 * d)
				//tci = ema(ci, n2)
				//wt1 = tci
				//wt2 = sma(wt1,4)
				
				AP[0]			= ((High[0] + Low[0] + Close[0] / 3));
				ESA[0]			= (EMA(AP, chanlen))[0];
				Dabsolute[0]	= (Math.Abs(AP[0]-ESA[0]));
				D[0]			= EMA(Dabsolute, chanlen)[0];
				CI[0]			= (AP[0] - ESA[0]) / (0.015 * D[0]);
				// TCI Wave Trend Fast dots
				Values[0][0]	= EMA(CI, avglen)[0];
				// WT2 Wave Trend Slow line
				Values[1][0]	= SMA(Values[0], 4)[0];
				
				#region Zone Display
				// Display Colored Zone Ranges
				//    Do nothing if option is disabled to save compute cycles
			
				if (ZoneDisplay != false)
				{
					DrawOnPricePanel = false;
					Draw.RegionHighlightY(this, "OverBoughtZone", true, OverBoughtUpper, OverBoughtLower, OverboughtBorder, OverboughtFill, ZoneOpacity);
					Draw.RegionHighlightY(this, "OverSoldZone", true, OverSoldUpper, OverSoldLower, OversoldBorder, OversoldFill, ZoneOpacity);
				}
				
				#endregion
				
				#region ZeroLine Crossover Triangles
				// ZeroLine Crossover Triangles
				//
				if (ZeroCrossover != false) {
					DrawOnPricePanel = false;
					if(CrossAbove(Values[0], 0, 1))	{
						//Draw.Square(this, CurrentBar.ToString()+"", true, 0, 0, ZeroCrossBullColor);
						Draw.TriangleUp(this, "A"+CurrentBar.ToString(), true, 0, 0, ZeroCrossBullColor);
						//Draw.TriangleUp(this, CurrentBar.ToString()+"", true, 0, 0, false, "TriangleSpringGreen");
					}
					else if (CrossBelow(Values[0], 0, 1)) {
						//Draw.Square(this, CurrentBar.ToString()+"", true, 0, 0, ZeroCrossBearColor);
						Draw.TriangleDown(this, "B"+CurrentBar.ToString(), true, 0, 0, ZeroCrossBearColor);
						//Draw.TriangleDown(this, CurrentBar.ToString()+"", true, 0, 0, false, "TriangleOrange");
					}
				}
				#endregion
				
				#region ZeroLine Paintbar
				// ZeroLine Paintbar Coloring on Crossover
			
				if (ZeroLinePaintbar != false) {
					// Bullish Color
					if(CrossAbove(Values[0], 0, 1)) {
						// Up candle, color only the outline
						if (Close[0] >= Open[0]) {
							CandleOutlineBrush = ZeroCrossBullColor;
						}
						else {
						// Down candle, Color both outline and body
							CandleOutlineBrush = ZeroCrossBullColor;
							BarBrush = ZeroCrossBullColor;
						}
					}
					// Bearish Color
					else if (CrossBelow(Values[0], 0, 1)) {
						if (Close[0] <= Open[0]) {
							CandleOutlineBrush = ZeroCrossBearColor;
							BarBrush = ZeroCrossBearColor;
						}
						else {
							CandleOutlineBrush = ZeroCrossBearColor;
						}
					}
				}
				
				#endregion
				
				#region Crossover Dots
				// Crossover dots
				//
				if (CrossoverDots != false) {
					DrawOnPricePanel = false;
					if(CrossAbove(Values[0], Values[1], 1))	{
						//DrawDot(CurrentBar.ToString()+"", true, 0, Values[1][0], cobull);
						Draw.Dot(this, "C"+CurrentBar.ToString(), true, 0, Values[1][0], CrossoverBull);
						//Draw.Dot(this, CurrentBar.ToString()+"", true, 0, Values[1][0], false, "DotCornflowerBlue");
					}
					else if (CrossBelow(Values[0], Values[1], 1)) {
						//DrawDot(CurrentBar.ToString()+"", true, 0, Values[1][0], cobear);
						Draw.Dot(this, "D"+CurrentBar.ToString(), true, 0, Values[1][0], CrossoverBear);
						//Draw.Dot(this, CurrentBar.ToString()+"", true, 0, Values[1][0], false, "DotPaleVioletRed");
					}
				}
				
				#endregion
				
				#region Crossover Paintbar
				// Crossover Paintbar Coloring
				// Written for Candle charts - Up Candles paint the bar outline and Down Candle bodies are painted
				//    Additional logic can be added here for Bar charts to paint the bar on Up bars
				if (PaintBarColoring != false) {
					// Bullish Color
					if(CrossAbove(Values[0], Values[1], 1)) {
						// Up candle, color only the outline
						if (Close[0] >= Open[0]) {
							CandleOutlineBrush = CrossoverBull;
						}
						else {
						// Down candle, Color both outline and body
							CandleOutlineBrush = CrossoverBull;
							BarBrush = CrossoverBull;
						}
					}
					// Bearish Color
					else if (CrossBelow(Values[0], Values[1], 1)) {
						if (Close[0] <= Open[0]) {
							CandleOutlineBrush = CrossoverBear;
							BarBrush = CrossoverBear;
						}
						else {
							CandleOutlineBrush = CrossoverBear;
						}
					}
				}
				#endregion
				
				#region Fast Average Coloring
				// Fast Average Under/Over Coloring
				if (FastAverageColoring != false) {
					DrawOnPricePanel = false;
					if(CrossAbove(Values[0], Values[1], 1))	{
						above = true;
					}
					else if (CrossBelow(Values[0], Values[1], 1)) {
						above = false;
					}
					if(above) {
						PlotBrushes[0][0] = CrossoverBull;
					}
					else {
						PlotBrushes[0][0] = CrossoverBear;
					}
				}
				#endregion
				

			}
		}	
		
			
		

		#region Properties

		#region Main Parameters
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[0]; }
		}
		
		// ----------------------- Parameter Properties that make up the core formula calculation ---------------------
	
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Channel Length", Description="Channel Length. Generally do not change from default of 10.", Order=1, GroupName="Parameters")]
		public int ChannelLength
		{
			get { return chanlen; }
			set { chanlen = value; }
		}
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Average Length", Description="Slower Average Length. Generally do not change from default of 21.", Order=2, GroupName="Parameters")]
		public int AverageLength
		{
			get { return avglen; }
			set { avglen = value; }
		}
		
		#endregion
		
		#region Zones
		// -------------------------- Zones Toggle Configuration -----------------------------------------
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Zone Display", Description="Toggle display of Zone Ranges", Order=1, GroupName="Zones")]
		public bool ZoneDisplay
        {
            get { return zonedisplay; }
            set { zonedisplay = value; }
        }
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace

		[Range(1, 100)]
		[Display(Name="Zone Opacity", Description="Opacity level for zones...1=light 100=dark", Order=2, GroupName="Zones")]
		public int ZoneOpacity
        {
            get { return zoneopacity; }
            set { zoneopacity = value; }
        }

		// ----------------------- Overbought Zone -------------------------
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(-10, int.MaxValue)]
		[Display(Name="Overbought Upper", Description="Upper level of Overbought zone. Generally do not change from default of 60.", Order=3, GroupName="Zones")]
		public double OverBoughtUpper
		{
			get { return obupper; }
			set { obupper = value; }
		}
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(-10, int.MaxValue)]
		[Display(Name="Overbought Lower", Description="Lower level of Overbought zone. Generally do not change from default of 53.", Order=4, GroupName="Zones")]
		public double OverBoughtLower
		{
			get { return oblower; }
			set { oblower = value; }
		}
		
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Overbought Border", Description="Overbought zone border color", Order=5, GroupName="Zones")]
		public Brush OverboughtBorder
		{ 
			get { return obborder;}
			set { obborder = value;}
		}
		// Serialize color object so it can be saved within a workspace
		[Browsable(false)]
		public string OverboughtBorderSerializable
		{
			get { return Serialize.BrushToString(obborder); }
			set { obborder = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Overbought Fill", Description="Overbought zone fill color", Order=6, GroupName="Zones")]
		public Brush OverboughtFill
		{ 
			get { return obfill;}
			set { obfill = value;}
		}
		// Serialize color object so it can be saved within a workspace
		[Browsable(false)]
		public string OverboughtFillSerializable
		{
			get { return Serialize.BrushToString(obfill); }
			set { obfill = Serialize.StringToBrush(value); }
		}	
		
		
		// ----------------------- Oversold Zone -------------------------
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(-100, int.MaxValue)]
		[Display(Name="Oversold Upper", Description="Upper level of Oversold zone. Generally do not change from default of -53.", Order=7, GroupName="Zones")]
		public double OverSoldUpper
		{
			get { return osupper; }
			set { osupper = value; }
		}
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Range(-100, int.MaxValue)]
		[Display(Name="Oversold Lower", Description="Lower level of Oversold zone. Generally do not change from default of -60.", Order=8, GroupName="Zones")]
		public double OverSoldLower
		{
			get { return oslower; }
			set { oslower = value; }
		}
		
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Oversold Border", Description="Oversold zone border color", Order=9, GroupName="Zones")]
		public Brush OversoldBorder
		{ 
			get { return osborder;}
			set { osborder = value;}
		}
		// Serialize color object so it can be saved within a workspace
		[Browsable(false)]
		public string OversoldBorderSerializable
		{
			get { return Serialize.BrushToString(osborder); }
			set { osborder = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Oversold Fill", Description="Oversold zone fill color", Order=10, GroupName="Zones")]
		public Brush OversoldFill
		{ 
			get { return osfill;}
			set { osfill = value;}
		}
		// Serialize color object so it can be saved within a workspace
		[Browsable(false)]
		public string OversoldFillSerializable
		{
			get { return Serialize.BrushToString(osfill); }
			set { osfill = Serialize.StringToBrush(value); }
		}	
		#endregion
		
		#region Crossover Dots
		// ------------------------- Crossover Dots -----------------------------------
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Crossover Dots", Description="Toggle Oscillator Crossover Dots for crossover events.", Order=1, GroupName="Crossover")]
		public bool CrossoverDots
        {
            get { return codots; }
            set { codots = value; }
        }
		
		// ------------------------- Crossover Colors --------------------------------
		[Display(Name="Bull Cross", Description="Bullish Crossover color for Dots, Paintbar and Fast Average.", Order=2, GroupName="Crossover")]
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		public Brush CrossoverBull
        {
            get { return cobull; }
            set { cobull = value; }
        }
		// Serialize color object so it can be saved within a workspace
		// Only a custom pallate color would need to be serialized (stored by the .NET XML Serializer)
		[Browsable(false)]
		public string CrossoverBullSerialize
		{
			get { return Serialize.BrushToString(cobull);}
			set { cobull = Serialize.StringToBrush(value);}
		}
		
		[Display(Name="Bear Cross", Description="Bearish Crossover color for Dots, Paintbar and Fast Average.", Order=3, GroupName="Crossover")]
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		public Brush CrossoverBear
        {
            get { return cobear; }
            set { cobear = value; }
        }
		// Serialize color object so it can be saved within a workspace
		// Only a custom pallate color would need to be serialized (stored by the .NET XML Serializer)
		[Browsable(false)]
		public string CrossoverBearSerialize
		{
			get { return Serialize.BrushToString(cobear);}
			set { cobear = Serialize.StringToBrush(value);}
		}
		
		#endregion
		
		#region Paintbar on Crossover
		// ----------------------- Paint Bar on Crossover --------------------------------------
		
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
 		[Display(Name="Paint Bar Coloring", Description="Toggle Paint Bar Coloring for crossover events.", Order=4, GroupName="Crossover")]
		public bool PaintBarColoring
        {
            get { return pbcoloring; }
            set { pbcoloring = value; }
        }
		#endregion
		
		#region Fast Average Coloring
		// ----------------------- Fast Average Coloring ---------------------------------------
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Fast Average Coloring", Description="Toggle Fast Average over/under Coloring.", Order=5, GroupName="Crossover")]
		public bool FastAverageColoring
        {
            get { return fastavgcoloring; }
            set { fastavgcoloring = value; }
        }
		#endregion
		
		#region Zero Line Crossover
		// ------------------------- Zero Line Crossover Triangles -----------------------------------
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Zero Cross Triangles", Description="Toggle Oscillator ZeroLine Crossover Triangles.", Order=6, GroupName="Crossover")]
		public bool ZeroCrossover
        {
            get { return zerocrosses; }
            set { zerocrosses = value; }
        }
		
		// ------------------------- Zero Line Paintbar Coloring --------------------------------------
		//[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		[Display(Name="Zero Cross Paintbars", Description="Toggle Oscillator ZeroLine Crossover Paintbars.", Order=7, GroupName="Crossover")]
		public bool ZeroLinePaintbar
        {
            get { return zlpb; }
            set { zlpb = value; }
        }
		
		// ------------------------- Zero Line Crossover Colors -----------------------------------
		[Display(Name="Zero Cross Bull Color", Description="ZeroLine Bull Crossover color for Triangles. Normally Transparent to match Up candles.", Order=8, GroupName="Crossover")]
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		public Brush ZeroCrossBullColor
        {
            get { return zerocrossbullcolor; }
            set { zerocrossbullcolor = value; }
        }
		// Serialize color object so it can be saved within a workspace
		// Only a custom pallate color would need to be serialized (stored by the .NET XML Serializer)
		[Browsable(false)]
		public string ZeroCrossBullSerialize
		{
			get { return Serialize.BrushToString(zerocrossbullcolor);}
			set { zerocrossbullcolor = Serialize.StringToBrush(value);}
		}
		
		[Display(Name="Zero Cross Bear Color", Description="ZeroLine Bear Crossover color for Triangles.", Order=9, GroupName="Crossover")]
		[XmlIgnore()]		// ensures that the property will NOT be saved/recovered as part of a chart template or workspace
		public Brush ZeroCrossBearColor
        {
            get { return zerocrossbearcolor; }
            set { zerocrossbearcolor = value; }
        }
		// Serialize color object so it can be saved within a workspace
		// Only a custom pallate color would need to be serialized (stored by the .NET XML Serializer)
		[Browsable(false)]
		public string ZeroCrossBearSerialize
		{
			get { return Serialize.BrushToString(zerocrossbearcolor);}
			set { zerocrossbearcolor = Serialize.StringToBrush(value);}
		}
		
		#endregion
		
		#endregion
	}

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WaveTrendV2[] cacheWaveTrendV2;
		public WaveTrendV2 WaveTrendV2(int channelLength, int averageLength)
		{
			return WaveTrendV2(Input, channelLength, averageLength);
		}

		public WaveTrendV2 WaveTrendV2(ISeries<double> input, int channelLength, int averageLength)
		{
			if (cacheWaveTrendV2 != null)
				for (int idx = 0; idx < cacheWaveTrendV2.Length; idx++)
					if (cacheWaveTrendV2[idx] != null && cacheWaveTrendV2[idx].ChannelLength == channelLength && cacheWaveTrendV2[idx].AverageLength == averageLength && cacheWaveTrendV2[idx].EqualsInput(input))
						return cacheWaveTrendV2[idx];
			return CacheIndicator<WaveTrendV2>(new WaveTrendV2(){ ChannelLength = channelLength, AverageLength = averageLength }, input, ref cacheWaveTrendV2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WaveTrendV2 WaveTrendV2(int channelLength, int averageLength)
		{
			return indicator.WaveTrendV2(Input, channelLength, averageLength);
		}

		public Indicators.WaveTrendV2 WaveTrendV2(ISeries<double> input , int channelLength, int averageLength)
		{
			return indicator.WaveTrendV2(input, channelLength, averageLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WaveTrendV2 WaveTrendV2(int channelLength, int averageLength)
		{
			return indicator.WaveTrendV2(Input, channelLength, averageLength);
		}

		public Indicators.WaveTrendV2 WaveTrendV2(ISeries<double> input , int channelLength, int averageLength)
		{
			return indicator.WaveTrendV2(input, channelLength, averageLength);
		}
	}
}

#endregion
