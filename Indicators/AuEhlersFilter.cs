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
	public class AuEhlersFilter : Indicator
	{
        #region Variables
        private int period = 15;
        private int length = 0;
        private int count = 0;
        private int lookback = 0;
        private double distance2 = 0.0;
        private double Num = 0.0;
        private double SumCoef = 0.0;
        private List<double> Coef = new List<double>();
        private TMA Smooth;

        private bool showPaintBars = true;
        private Brush upColor = Brushes.Lime;
        private Brush neutralColor = Brushes.Tan;
        private Brush downColor = Brushes.Red;
        private int opacity = 4;
        private int alphaBarClr = 0;
        private bool showPlot = true;
        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"AuEhlersFilter, see paper https://c.forex-tsd.com/forum/76/ehlers_-_non_linear_filters.pdf";
                Name										= "AuEhlersFilter";
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

                ShowTransparentPlotsInDataBox = true;

                AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "EhlersFilter");
                AddPlot(new Stroke() { Brush = Brushes.Transparent }, PlotStyle.Dot, "Trend");

                if (Period > 256)
                    MaximumBarsLookBack = MaximumBarsLookBack.Infinite;
            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            //OnStartup
            if (CurrentBar < 1)
            {
                Smooth = TMA(Math.Min(period, 4));
                for (int i = 0; i < period; i++)
                    Coef.Add(0.0);
            }
            else
            {
                if (CurrentBar < 2)
                {
                    EhlersFilter[0] = Smooth[0];
                    return;
                }

                if (CurrentBar < 2 * period - 1)
                    length = CurrentBar / 2 + 1;
                else
                    length = period;

                for (count = 0; count < length; count++)
                {
                    distance2 = 0;
                    for (lookback = 1; lookback < length; lookback++)
                        distance2 = distance2 + (Smooth[count] - Smooth[count + lookback]) * (Smooth[count] - Smooth[count + lookback]);
                    Coef[count] = distance2;
                }

                Num = 0.0;
                SumCoef = 0.0;

                for (count = 0; count < length; count++)
                {
                    Num = Num + Coef[count] * Smooth[count];
                    SumCoef = SumCoef + Coef[count];
                }

                if (SumCoef != 0)
                    EhlersFilter[0] = Num / SumCoef;
                else if (period > 1)
                    EhlersFilter[0] = EhlersFilter[1];
                else
                    EhlersFilter[0] = Smooth[count];
            }

            if (CurrentBar < 1)
            {
                alphaBarClr = 25 * opacity;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;
            }
            else
            {
                Trend[0] = 0;
                if (Value[0] > Value[1])
                    Trend[0] = 1;
                else if (Value[0] < Value[1])
                    Trend[0] = -1;

                if (showPlot)
                {
                    if (Trend[0] == 1)
                        PlotBrushes[0][0] = upColor;
                    else if (Trend[0] == -1)
                        PlotBrushes[0][0] = downColor;
                    else if (Trend[0] == 0)
                        PlotBrushes[0][0] = neutralColor;
                }

                if (showPaintBars)
                {
                    if (Trend[0] == 1)
                    {
                        BarBrushes[0] = upColor;
                        CandleOutlineBrushes[0] = upColor;
                    }
                    else if (Trend[0] == -1)
                    {
                        BarBrushes[0] = downColor;
                        CandleOutlineBrushes[0] = downColor;
                    }
                    else
                    {
                        BarBrushes[0] = neutralColor;
                        CandleOutlineBrushes[0] = neutralColor;
                    }

                    if (Close[0] > Open[0])
                    {
                        byte g = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).B;

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)alphaBarClr, r, g, b));
                    }
                }
            }

        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EhlersFilter
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Trend
        {
            get { return Values[1]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period", Order = 0, GroupName = "Gen. Parameters")]
        public int Period
        {
            get { return period; }
            set { period = period = Math.Max(1, value); }
        }

        [Display(Name = "Show PaintBars", Description = "Show paint bars on price panel", Order = 1, GroupName = "Gen. Parameters")]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }

        [XmlIgnore]
        [Display(Name = "Average Chop Mode", Description = "Select color for neutral average", Order = 0, GroupName = "Plot Colors")]
        public Brush NeutralColor
        {
            get { return neutralColor; }
            set { neutralColor = value; }
        }

        [Browsable(false)]
        public string NeutralColorSerialize
        {
            get { return Serialize.BrushToString(neutralColor); }
            set { neutralColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Average Falling", Description = "Select color for falling average", Order = 1, GroupName = "Plot Colors")]
        public Brush DownColor
        {
            get { return downColor; }
            set { downColor = value; }
        }

        [Browsable(false)]
        public string DownColorSerialize
        {
            get { return Serialize.BrushToString(downColor); }
            set { downColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Average Rising", Description = "Select color for rising average", Order = 2, GroupName = "Plot Colors")]
        public Brush UpColor
        {
            get { return upColor; }
            set { upColor = value; }
        }

        [Browsable(false)]
        public string UpColorSerialize
        {
            get { return Serialize.BrushToString(upColor); }
            set { upColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Upclose Opacity", Description = "When paint bars are activated, this parameter sets the opacity of the upclose bars", Order = 3, GroupName = "Plot Colors")]
        public int Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        [Display(Name = "Show Plot", Description = "Show plot", Order = 4, GroupName = "Plot Colors")]
        public bool ShowPlot
        {
            get { return showPlot; }
            set { showPlot = value; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AuEhlersFilter[] cacheAuEhlersFilter;
		public AuEhlersFilter AuEhlersFilter(int period)
		{
			return AuEhlersFilter(Input, period);
		}

		public AuEhlersFilter AuEhlersFilter(ISeries<double> input, int period)
		{
			if (cacheAuEhlersFilter != null)
				for (int idx = 0; idx < cacheAuEhlersFilter.Length; idx++)
					if (cacheAuEhlersFilter[idx] != null && cacheAuEhlersFilter[idx].Period == period && cacheAuEhlersFilter[idx].EqualsInput(input))
						return cacheAuEhlersFilter[idx];
			return CacheIndicator<AuEhlersFilter>(new AuEhlersFilter(){ Period = period }, input, ref cacheAuEhlersFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuEhlersFilter AuEhlersFilter(int period)
		{
			return indicator.AuEhlersFilter(Input, period);
		}

		public Indicators.AuEhlersFilter AuEhlersFilter(ISeries<double> input , int period)
		{
			return indicator.AuEhlersFilter(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuEhlersFilter AuEhlersFilter(int period)
		{
			return indicator.AuEhlersFilter(Input, period);
		}

		public Indicators.AuEhlersFilter AuEhlersFilter(ISeries<double> input , int period)
		{
			return indicator.AuEhlersFilter(input, period);
		}
	}
}

#endregion
