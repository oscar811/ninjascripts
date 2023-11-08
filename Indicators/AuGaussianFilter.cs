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
	public class AuGaussianFilter : Indicator
	{
        #region Variables
        private int period = 20;
        private int poles = 3;
        private double alpha = 0;
        private double beta = 0;
        private double alpha2 = 0;
        private double alpha3 = 0;
        private double alpha4 = 0;
        private double coeff1 = 0;
        private double coeff2 = 0;
        private double coeff3 = 0;
        private double coeff4 = 0;
        private double recurrentPart = 0;

        private bool showPaintBars = true;
        private Brush upColor = Brushes.Lime;
        private Brush neutralColor = Brushes.Tan;
        private Brush downColor = Brushes.Red;
        private int opacity = 4;
        private int alphaBarClr = 0;
        private bool showPlot = true;

        #endregion

        // This is an implementation of the 1-pole, 2-pole, 3-pole and 4-pole Gaussian Filters, as published by John F. Ehlers
        // in his publication "Gaussian and Other Low Lag Filters".
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This is an implementation of the 1-pole, 2-pole, 3-pole and 4-pole Gaussian Filters, as published by John F. Ehlers";
				Name										= "AuGaussianFilter";
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

                AddPlot(new Stroke(Brushes.DeepSkyBlue, 2), PlotStyle.Line, "Gaussian");
                AddPlot(new Stroke() { Brush = Brushes.Transparent, Width = 1, DashStyleHelper = DashStyleHelper.Solid }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            double Pi = Math.PI;
            double Sq2 = Math.Sqrt(2.0);

            //OnStartup
            if (CurrentBar < 1)
            {
                alphaBarClr = 25 * opacity;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;

                beta = (1 - Math.Cos(2 * Pi / period)) / (Math.Pow(Sq2, 2.0 / Poles) - 1);

                if (period == 1)
                    alpha = 1.0;
                else
                    alpha = -beta + Math.Sqrt(beta * (beta + 2));

                alpha2 = alpha * alpha;
                alpha3 = alpha2 * alpha;
                alpha4 = alpha3 * alpha;
                coeff1 = 1.0 - alpha;
                coeff2 = coeff1 * coeff1;
                coeff3 = coeff2 * coeff1;
                coeff4 = coeff3 * coeff1;
            }
            else
            {
                if (CurrentBar < Poles)
                {
                    Gaussian[0] = Input[0];
                    return;
                }

                if (IsFirstTickOfBar)
                {
                    if (Poles == 1)
                        recurrentPart = coeff1 * Value[1];
                    else if (Poles == 2)
                        recurrentPart = 2 * coeff1 * Value[1] - coeff2 * Value[2];
                    else if (Poles == 3)
                        recurrentPart = 3 * coeff1 * Value[1] - 3 * coeff2 * Value[2] + coeff3 * Value[3];
                    else if (Poles == 4)
                        recurrentPart = 4 * coeff1 * Value[1] - 6 * coeff2 * Value[2] + 4 * coeff3 * Value[3] - coeff4 * Value[4];
                }
                if (Poles == 1)
                    Gaussian[0] = alpha * Input[0] + recurrentPart;
                else if (Poles == 2)
                    Gaussian[0] = alpha2 * Input[0] + recurrentPart;
                else if (Poles == 3)
                    Gaussian[0] = alpha3 * Input[0] + recurrentPart;
                else if (Poles == 4)
                    Gaussian[0] = alpha4 * Input[0] + recurrentPart;

                Trend[0] = 0;
                if (Gaussian[0] > Gaussian[1])
                    Trend[0] = 1;
                else if (Gaussian[0] < Gaussian[1])
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
        public Series<double> Gaussian
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
        [Display(Name = "# Poles", Description = "Number of Poles", Order = 0, GroupName = "Gen. Parameters")]
        public int Poles
        {
            get { return poles; }
            set { poles = Math.Min(Math.Max(1, value), 4); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period", Order = 1, GroupName = "Gen. Parameters")]
        public int Period
        {
            get { return period; }
            set { period = period = Math.Max(1, value); }
        }

        [Display(Name = "Show PaintBars", Description = "Show paint bars on price panel", Order = 2, GroupName = "Gen. Parameters")]
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

        [Display(Name = "Show Plot", Description = "Show plot of the Zero-Lagging Heiken-Ashi TEMA", Order = 4, GroupName = "Plot Colors")]
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
		private AuGaussianFilter[] cacheAuGaussianFilter;
		public AuGaussianFilter AuGaussianFilter(int poles, int period)
		{
			return AuGaussianFilter(Input, poles, period);
		}

		public AuGaussianFilter AuGaussianFilter(ISeries<double> input, int poles, int period)
		{
			if (cacheAuGaussianFilter != null)
				for (int idx = 0; idx < cacheAuGaussianFilter.Length; idx++)
					if (cacheAuGaussianFilter[idx] != null && cacheAuGaussianFilter[idx].Poles == poles && cacheAuGaussianFilter[idx].Period == period && cacheAuGaussianFilter[idx].EqualsInput(input))
						return cacheAuGaussianFilter[idx];
			return CacheIndicator<AuGaussianFilter>(new AuGaussianFilter(){ Poles = poles, Period = period }, input, ref cacheAuGaussianFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuGaussianFilter AuGaussianFilter(int poles, int period)
		{
			return indicator.AuGaussianFilter(Input, poles, period);
		}

		public Indicators.AuGaussianFilter AuGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.AuGaussianFilter(input, poles, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuGaussianFilter AuGaussianFilter(int poles, int period)
		{
			return indicator.AuGaussianFilter(Input, poles, period);
		}

		public Indicators.AuGaussianFilter AuGaussianFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.AuGaussianFilter(input, poles, period);
		}
	}
}

#endregion
