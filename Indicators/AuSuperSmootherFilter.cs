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
	public class AuSuperSmootherFilter : Indicator
	{
        #region Variables
        private int period = 20;
        private int poles = 3;
        private double a1 = 0;
        private double b1 = 0;
        private double c1 = 0;
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

        // This is an implementation of the 2-pole and 3-pole Super Smoother Filters, as published by 
        // John F. Ehlers in his book "Cybernetic Analysis for Stocks and Futures".
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This is an implementation of the 2-pole and 3-pole Super Smoother Filters";
				Name										= "AuSuperSmootherFilter";
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

                AddPlot(new Stroke(Brushes.DeepSkyBlue, 2), PlotStyle.Line, "SuperSmoother");
                AddPlot(new Stroke() { Brush = Brushes.Transparent, Width = 1, DashStyleHelper = DashStyleHelper.Solid }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            double pi = Math.PI;
            double sq2 = Math.Sqrt(2.0);
            double sq3 = Math.Sqrt(3.0);

            //OnStartup
            if (CurrentBar < 1)
            {
                alphaBarClr = 25 * opacity;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;

                if (Poles == 2)
                {
                    a1 = Math.Exp(-sq2 * pi / period);
                    b1 = 2 * a1 * Math.Cos(sq2 * pi / period);
                    coeff2 = b1;
                    coeff3 = -a1 * a1;
                    coeff1 = 1 - coeff2 - coeff3;
                }
                else if (Poles == 3)
                {
                    a1 = Math.Exp(-pi / Period);
                    b1 = 2 * a1 * Math.Cos(sq3 * pi / period);
                    c1 = a1 * a1;
                    coeff2 = b1 + c1;
                    coeff3 = -(c1 + b1 * c1);
                    coeff4 = c1 * c1;
                    coeff1 = 1 - coeff2 - coeff3 - coeff4;
                }
            }
            else
            {
                if (CurrentBar < Poles)
                {
                    SuperSmoother[0] = Input[0];
                    return;
                }

                if (IsFirstTickOfBar)
                {
                    if (Poles == 2)
                        recurrentPart = coeff2 * Value[1] + coeff3 * Value[2];
                    else if (Poles == 3)
                        recurrentPart = coeff2 * Value[1] + coeff3 * Value[2] + coeff4 * Value[3];
                }

                SuperSmoother[0] = recurrentPart + coeff1 * Input[0];

                if (CurrentBar > 1)
                {
                    Trend[0] = 0;
                    if (SuperSmoother[0] > SuperSmoother[1])
                        Trend[0] = 1;
                    else if (SuperSmoother[0] < SuperSmoother[1])
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

        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SuperSmoother
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
            set { poles = Math.Min(Math.Max(2, value), 3); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period", Order = 1, GroupName = "Gen. Parameters")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
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
		private AuSuperSmootherFilter[] cacheAuSuperSmootherFilter;
		public AuSuperSmootherFilter AuSuperSmootherFilter(int poles, int period)
		{
			return AuSuperSmootherFilter(Input, poles, period);
		}

		public AuSuperSmootherFilter AuSuperSmootherFilter(ISeries<double> input, int poles, int period)
		{
			if (cacheAuSuperSmootherFilter != null)
				for (int idx = 0; idx < cacheAuSuperSmootherFilter.Length; idx++)
					if (cacheAuSuperSmootherFilter[idx] != null && cacheAuSuperSmootherFilter[idx].Poles == poles && cacheAuSuperSmootherFilter[idx].Period == period && cacheAuSuperSmootherFilter[idx].EqualsInput(input))
						return cacheAuSuperSmootherFilter[idx];
			return CacheIndicator<AuSuperSmootherFilter>(new AuSuperSmootherFilter(){ Poles = poles, Period = period }, input, ref cacheAuSuperSmootherFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuSuperSmootherFilter AuSuperSmootherFilter(int poles, int period)
		{
			return indicator.AuSuperSmootherFilter(Input, poles, period);
		}

		public Indicators.AuSuperSmootherFilter AuSuperSmootherFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.AuSuperSmootherFilter(input, poles, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuSuperSmootherFilter AuSuperSmootherFilter(int poles, int period)
		{
			return indicator.AuSuperSmootherFilter(Input, poles, period);
		}

		public Indicators.AuSuperSmootherFilter AuSuperSmootherFilter(ISeries<double> input , int poles, int period)
		{
			return indicator.AuSuperSmootherFilter(input, poles, period);
		}
	}
}

#endregion
