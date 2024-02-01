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
using NinjaTrader.NinjaScript.Strategies.RajAlgos;
#endregion

// Modified 12/5/17 to add support for using indicator as the input series.
namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class SwingRays2c : Indicator
    {
        private Stack SwingHighRays { get; set; }        /*	Last Entry represents the most recent swing, i.e. 				*/
        private Stack SwingLowRays { get; set; }         /*	swingHighRays are sorted descedingly by price and vice versa	*/

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SwingRays2c";
                Description = @"Plots horizontal rays at swing highs and lows and removes them once broken";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                Strength = 5;
                KeepBrokenLines = false; // defaulted to false to reduce overhead
                SwingHighColor = Brushes.DodgerBlue;
                SwingLowColor = Brushes.Fuchsia;
                LineWidth = 1;
            }
            else if (State == State.Configure)
            {
                SwingHighRays = new Stack();
                SwingLowRays = new Stack();

                AddPlot(Brushes.Transparent, "IsHighBroken");
                AddPlot(Brushes.Transparent, "IsHighSwept");
                AddPlot(Brushes.Transparent, "IsLowBroken");
                AddPlot(Brushes.Transparent, "IsLowSwept");
            }
            else if (State != State.DataLoaded)
            {

            }
        }

        protected override void OnBarUpdate()
        {
            /*			Swing break determination is on TickByTick basis, Swing creation not			*/			
            /*			IRay's Anchor member gives the price at which the ray is draw...				*/

            try
            {
                if (CurrentBar < 2 * Strength + 2)
                    return;

                if (IsFirstTickOfBar)
                {   /*	Highs - Check whether we have a new swing peaking at CurrentBar - Strength - 1	 	*/
                    int uiHistory = 1;

                    bool bIsSwingHigh = true;

                    while (uiHistory <= 2 * Strength + 1 && bIsSwingHigh)
                    {
                        if (uiHistory != Strength + 1 && (Input is Indicator ? Input[uiHistory] : High[uiHistory]) > (Input is Indicator ? Input[Strength + 1] : High[Strength + 1]) - double.Epsilon)
                        {
                            bIsSwingHigh = false;
                        }
                        else
                        {
                            uiHistory++;
                        }
                    }

                    if (bIsSwingHigh)
                    {
                        Ray newRay = Draw.Ray(this, "highRay" + (CurrentBar - Strength - 1), false, Strength + 1, (Input is Indicator ? Input[Strength + 1] : High[Strength + 1]), 0, (Input is Indicator ? Input[Strength + 1] : High[Strength + 1]), SwingHighColor, DashStyleHelper.Dash, LineWidth);
                        //SwingHighRays.Push(newRay);                     /*	Store Ray for future removal	*/
                        SwingHighRays.Push(new SwingLevel(High[Strength + 1], newRay));  /*	Store Ray for future removal	*/
                    }

                    /*	Low - Check whether we have a new swing with a bottom at CurrentBar - Strength - 1	*/
                    bool bIsSwingLow = true;

                    uiHistory = 1;

                    while (uiHistory <= 2 * Strength + 1 && bIsSwingLow)
                    {
                        if (uiHistory != Strength + 1 && (Input is Indicator ? Input[uiHistory] : Low[uiHistory]) < (Input is Indicator ? Input[Strength + 1] : Low[Strength + 1]) + double.Epsilon)
                        {
                            bIsSwingLow = false;
                        }
                        else
                        {
                            uiHistory++;
                        }
                    }

                    if (bIsSwingLow)
                    {
                        Ray newRay = Draw.Ray(this, "lowRay" + (CurrentBar - Strength - 1), false, Strength + 1, (Input is Indicator ? Input[Strength + 1] : Low[Strength + 1]), 0, (Input is Indicator ? Input[Strength + 1] : Low[Strength + 1]), SwingLowColor, DashStyleHelper.Dash, LineWidth);
                        //SwingLowRays.Push(newRay);                      /*	Store Ray for future removal	*/
                        SwingLowRays.Push(new SwingLevel(Low[Strength + 1], newRay));  /*	Store Ray for future removal	*/
                    }
                }


                /*	Check the break of some swing	*/
                /*	High swings first...	*/
                Ray tmpRay = null;

                if (SwingHighRays.Count != 0)
                {
                    tmpRay = ((SwingLevel)SwingHighRays.Peek()).Ray;
                }

                while (SwingHighRays.Count != 0 && (Input is Indicator ? Input[0] : High[0] + ToleranceTicks) > tmpRay.StartAnchor.Price)
                {
                    RemoveDrawObject(tmpRay.Tag);

                    if (KeepBrokenLines)
                    {   /*	Draw a line for the broken swing */
                        int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + Strength + 1;          /*	When did the ray being removed start?  Had to account for strength */
                        Draw.Line(this, "highLine" + (CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingHighColor, DashStyleHelper.Dot, LineWidth);
                    }

                    SwingHighRays.Pop();
                    Values[0][0] = 1.0;
                    if (Close[0] < tmpRay.StartAnchor.Price) Values[1][0] = 1.0;

                    if (SwingHighRays.Count != 0)
                    {
                        tmpRay = ((SwingLevel)SwingHighRays.Peek()).Ray;
                    }
                }

                /*		Low swings follow...	*/
                if (SwingLowRays.Count != 0)
                {
                    tmpRay = ((SwingLevel)SwingLowRays.Peek()).Ray;
                }

                while (SwingLowRays.Count != 0 && (Input is Indicator ? Input[0] : Low[0] - ToleranceTicks) < tmpRay.StartAnchor.Price)
                {
                    RemoveDrawObject(tmpRay.Tag);

                    if (KeepBrokenLines)
                    {   /*	Draw a line for the broken swing */
                        int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + Strength + 1;          /*	When did the ray being removed start?  Had to account for strength	*/
                        Draw.Line(this, "lowLine" + (CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingLowColor, DashStyleHelper.Dot, LineWidth);
                    }

                    SwingLowRays.Pop();
                    Values[2][0] = 1.0;
                    if (Close[0] > tmpRay.StartAnchor.Price) Values[3][0] = 1.0;

                    if (SwingLowRays.Count != 0)
                    {
                        tmpRay = ((SwingLevel)SwingLowRays.Peek()).Ray;
                    }
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties

        [Range(2, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Swing Strength", Description = "Number of bars before/after each pivot bar", Order = 1, GroupName = "Parameters")]
        public int Strength
        { get; set; }

        [Range(0, 10)]
        [NinjaScriptProperty]
        [Display(Name = "Tolerance Ticks", Description = "Threshold Ticks", Order = 2, GroupName = "Parameters")]
        public int ToleranceTicks
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Keep broken lines", Description = "Show broken swing lines, beginning to end", Order = 3, GroupName = "Options")]
        public bool KeepBrokenLines
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Swing High color", Description = "Color for swing high rays/lines", Order = 4, GroupName = "Options")]
        public Brush SwingHighColor
        { get; set; }

        [Browsable(false)]
        public string SwingHighColorSerializable
        {
            get { return Serialize.BrushToString(SwingHighColor); }
            set { SwingHighColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Swing Low color", Description = "Color for swing low rays/lines", Order = 5, GroupName = "Options")]
        public Brush SwingLowColor
        { get; set; }

        [Browsable(false)]
        public string SwingLowColorSerializable
        {
            get { return Serialize.BrushToString(SwingLowColor); }
            set { SwingLowColor = Serialize.StringToBrush(value); }
        }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Line width", Description = "Thickness of swing lines", Order = 6, GroupName = "Options")]
        public int LineWidth
        { get; set; }

        public Series<double> IsHighBroken { get { return Values[0]; } }
        public Series<double> IsHighSwept { get { return Values[1]; } }

        public Series<double> IsLowBroken { get { return Values[2]; } }
        public Series<double> IsLowSwept { get { return Values[3]; } }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.SwingRays2c[] cacheSwingRays2c;
		public RajIndicators.SwingRays2c SwingRays2c(int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			return SwingRays2c(Input, strength, toleranceTicks, keepBrokenLines, lineWidth);
		}

		public RajIndicators.SwingRays2c SwingRays2c(ISeries<double> input, int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			if (cacheSwingRays2c != null)
				for (int idx = 0; idx < cacheSwingRays2c.Length; idx++)
					if (cacheSwingRays2c[idx] != null && cacheSwingRays2c[idx].Strength == strength && cacheSwingRays2c[idx].ToleranceTicks == toleranceTicks && cacheSwingRays2c[idx].KeepBrokenLines == keepBrokenLines && cacheSwingRays2c[idx].LineWidth == lineWidth && cacheSwingRays2c[idx].EqualsInput(input))
						return cacheSwingRays2c[idx];
			return CacheIndicator<RajIndicators.SwingRays2c>(new RajIndicators.SwingRays2c(){ Strength = strength, ToleranceTicks = toleranceTicks, KeepBrokenLines = keepBrokenLines, LineWidth = lineWidth }, input, ref cacheSwingRays2c);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.SwingRays2c SwingRays2c(int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2c(Input, strength, toleranceTicks, keepBrokenLines, lineWidth);
		}

		public Indicators.RajIndicators.SwingRays2c SwingRays2c(ISeries<double> input , int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2c(input, strength, toleranceTicks, keepBrokenLines, lineWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.SwingRays2c SwingRays2c(int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2c(Input, strength, toleranceTicks, keepBrokenLines, lineWidth);
		}

		public Indicators.RajIndicators.SwingRays2c SwingRays2c(ISeries<double> input , int strength, int toleranceTicks, bool keepBrokenLines, int lineWidth)
		{
			return indicator.SwingRays2c(input, strength, toleranceTicks, keepBrokenLines, lineWidth);
		}
	}
}

#endregion
