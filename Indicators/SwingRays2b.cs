#region Using declarations
using System;
using System.Collections;
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

// Converted 06/19/2017.  Added sound selectors and seperated sound from alert (so only hear sound once per bar)

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SwingRays2b : Indicator
		
		// Modified 12/5/17 to add support for using indicator as the input series.
	{		
		private Stack swingHighRays;		/*	Last Entry represents the most recent swing, i.e. 				*/
		private Stack swingLowRays;			/*	swingHighRays are sorted descedingly by price and vice versa	*/
		private int soundBar	= 0;		// to prevent multiple sounds when Calculate = OnEachTick or OnPriceChange, so one sound per bar only.		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Plots horizontal rays at swing highs and lows and removes them once broken";
				Name										= "SwingRays2b";
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
				Strength									= 5;
				EnableAlerts								= false;
				KeepBrokenLines								= false; // defaulted to false to reduce overhead
				SwingHighColor								= Brushes.DodgerBlue;
				SwingLowColor								= Brushes.Fuchsia;
				Upfile										= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.Wav";
				Downfile 									= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert3.Wav";	
				LineWidth									= 1;
			}
			else if (State == State.Configure)
			{
				swingHighRays  = new Stack ();   			
				swingLowRays   = new Stack ();				
			}
		}

		protected override void OnBarUpdate()
		{
			/*			Swing break determination is on TickByTick basis, Swing creation not			*/
			/*			IRay's Anchor member gives the price at which the ray is draw...				*/
			
			if (CurrentBar == 0)
			{
				if (ChartPanel.PanelIndex != 0)  // support for placing indicator into indicator panel V1.02
				{
					DrawOnPricePanel = false;  // move draw objects to indicator panel
				}
			}			

			if ( CurrentBar < 2 * Strength + 2 )
				return;
			
			if ( IsFirstTickOfBar )
			{	/*	Highs - Check whether we have a new swing peaking at CurrentBar - Strength - 1	 	*/
				int uiHistory = 1;
				
				bool bIsSwingHigh = true;
				
				while ( uiHistory <= 2*Strength + 1 && bIsSwingHigh )
				{
					if ( uiHistory != Strength + 1 && (Input is Indicator ? Input[uiHistory] : High[ uiHistory ]) > (Input is Indicator ? Input[Strength+1] : High[ Strength + 1 ]) - double.Epsilon )
					{
						bIsSwingHigh = false;
					}
					else
					{
						uiHistory++;
					}
				}
		
				if ( bIsSwingHigh )
				{
					Ray newRay = Draw.Ray(this,  "highRay" + (CurrentBar-Strength - 1), false, Strength + 1, (Input is Indicator ? Input[Strength+1] : High[ Strength + 1]), 0, (Input is Indicator ? Input[Strength+1] : High[ Strength + 1]), SwingHighColor, DashStyleHelper.Dash, LineWidth); 
					swingHighRays.Push(newRay);  					/*	Store Ray for future removal	*/
				}

				/*	Low - Check whether we have a new swing with a bottom at CurrentBar - Strength - 1	*/
				bool bIsSwingLow = true;
				
				uiHistory = 1;
				
				while (uiHistory  <= 2*Strength + 1 && bIsSwingLow )
				{
					if ( uiHistory  != Strength + 1 && (Input is Indicator ? Input[uiHistory] : Low[uiHistory]) < (Input is Indicator ? Input[Strength +1] : Low[ Strength + 1 ]) + double.Epsilon )
					{
						bIsSwingLow = false;
					}
					else
					{
						uiHistory++;
					}
				}
		
				if ( bIsSwingLow )
				{
					Ray newRay = Draw.Ray(this, "lowRay" + (CurrentBar - Strength - 1), false, Strength + 1, (Input is Indicator ? Input[Strength+1] : Low[ Strength + 1]), 0, (Input is Indicator ? Input[Strength+1] : Low[ Strength + 1]), SwingLowColor, DashStyleHelper.Dash, LineWidth); 
					swingLowRays.Push(newRay);  					/*	Store Ray for future removal	*/
				}
			}


			/*	Check the break of some swing	*/
			/*	High swings first...	*/
			Ray tmpRay = null;

			if ( swingHighRays.Count != 0 )
			{
				tmpRay = (Ray)swingHighRays.Peek();
			}
			
			while ( swingHighRays.Count != 0 && (Input is Indicator ? Input[0] : High[0]) > tmpRay.StartAnchor.Price)
			{
				RemoveDrawObject(tmpRay.Tag);  
				
				if (SoundsOn && soundBar != CurrentBar)
				{
					PlaySound (Upfile);
					soundBar = CurrentBar;
				}	
				
				if (EnableAlerts) 
				{
					Alert("SwHiAlert", Priority.Low, "Swing High at " + tmpRay.StartAnchor.Price + " broken","", 5, Brushes.White, Brushes.Red);
				}
				
				if ( KeepBrokenLines )
				{	/*	Draw a line for the broken swing */
					int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar  + Strength + 1;			/*	When did the ray being removed start?  Had to account for strength */
					Draw.Line(this, "highLine"+(CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingHighColor, DashStyleHelper.Dot, LineWidth);
				}
				
				swingHighRays.Pop();
				
				if( swingHighRays.Count != 0 )
				{
					tmpRay = (Ray)swingHighRays.Peek();
				}
			}
			
			/*		Low swings follow...	*/
			if ( swingLowRays.Count != 0 )
			{
				tmpRay = (Ray)swingLowRays.Peek();
			}
		
			while ( swingLowRays.Count != 0 && (Input is Indicator ? Input[0] : Low[0]) < tmpRay.StartAnchor.Price )
			{
				RemoveDrawObject(tmpRay.Tag);  
				
				if (SoundsOn && soundBar != CurrentBar)
				{
					PlaySound (Downfile);
					soundBar = CurrentBar;
				}				
				
				if (EnableAlerts) 
				{
					Alert("SwHiAlert", Priority.Low, "Swing Low at " + tmpRay.StartAnchor.Price + " broken","", 5, Brushes.White, Brushes.Red);
				}
				
				if ( KeepBrokenLines )
				{	/*	Draw a line for the broken swing */
					int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + Strength + 1;			/*	When did the ray being removed start?  Had to account for strength	*/
					Draw.Line(this, "lowLine"+(CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingLowColor, DashStyleHelper.Dot, LineWidth);
				}
				
				swingLowRays.Pop();
				
				if ( swingLowRays.Count != 0 )
				{
					tmpRay =(Ray) swingLowRays.Peek();
				}
			}
		}

		#region Properties
		[Range(2, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Swing Strength", Description="Number of bars before/after each pivot bar", Order=1, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Enable Alert log msgs", Description="Prints alert messages in the (New>Alert log) window when swings are broken", Order=2, GroupName="Parameters")]
		public bool EnableAlerts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Keep broken lines", Description="Show broken swing lines, beginning to end", Order=3, GroupName="Parameters")]
		public bool KeepBrokenLines
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Swing High color", Description="Color for swing high rays/lines", Order=4, GroupName="Options")]
		public Brush SwingHighColor
		{ get; set; }

		[Browsable(false)]
		public string SwingHighColorSerializable
		{
			get { return Serialize.BrushToString(SwingHighColor); }
			set { SwingHighColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Swing Low color", Description="Color for swing low rays/lines", Order=5, GroupName="Options")]
		public Brush SwingLowColor
		{ get; set; }

		[Browsable(false)]
		public string SwingLowColorSerializable
		{
			get { return Serialize.BrushToString(SwingLowColor); }
			set { SwingLowColor = Serialize.StringToBrush(value); }
		}	
		
		[Display(Name="Alert Sound On", Description="Play sounds when swing line broken", Order=1, GroupName="Options")]
		public bool SoundsOn
		{ get; set; }

		[Display(Name="Swing High broken sound", Description="Enter sound file path/name", Order=2, GroupName="Options")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string Upfile
		{ get; set; }

		[Display(Name="Swing Low broken sound", Description="Enter sound file path/name", Order=3, GroupName="Options")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string Downfile
		{ get; set; }
		
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Line width", Description="Thickness of swing lines", Order=6, GroupName="Options")]
		public int LineWidth
		{ get; set; }		
		
		
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SwingRays2b[] cacheSwingRays2b;
		public SwingRays2b SwingRays2b(int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			return SwingRays2b(Input, strength, enableAlerts, keepBrokenLines, lineWidth);
		}

		public SwingRays2b SwingRays2b(ISeries<double> input, int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			if (cacheSwingRays2b != null)
				for (int idx = 0; idx < cacheSwingRays2b.Length; idx++)
					if (cacheSwingRays2b[idx] != null && cacheSwingRays2b[idx].Strength == strength && cacheSwingRays2b[idx].EnableAlerts == enableAlerts && cacheSwingRays2b[idx].KeepBrokenLines == keepBrokenLines && cacheSwingRays2b[idx].LineWidth == lineWidth && cacheSwingRays2b[idx].EqualsInput(input))
						return cacheSwingRays2b[idx];
			return CacheIndicator<SwingRays2b>(new SwingRays2b(){ Strength = strength, EnableAlerts = enableAlerts, KeepBrokenLines = keepBrokenLines, LineWidth = lineWidth }, input, ref cacheSwingRays2b);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SwingRays2b SwingRays2b(int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2b(Input, strength, enableAlerts, keepBrokenLines, lineWidth);
		}

		public Indicators.SwingRays2b SwingRays2b(ISeries<double> input , int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2b(input, strength, enableAlerts, keepBrokenLines, lineWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SwingRays2b SwingRays2b(int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2b(Input, strength, enableAlerts, keepBrokenLines, lineWidth);
		}

		public Indicators.SwingRays2b SwingRays2b(ISeries<double> input , int strength, bool enableAlerts, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2b(input, strength, enableAlerts, keepBrokenLines, lineWidth);
		}
	}
}

#endregion
