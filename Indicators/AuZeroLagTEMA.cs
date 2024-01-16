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
    public class AuZeroLagTEMA : Indicator
	{
        #region Variables
        private int period = 14;

        private TEMA tema1;
        private TEMA tema2;

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
				Description									= @"Zero-Lagging TEMA";
				Name										= "AuZeroLagTEMA";
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

                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "Zero-Lag TEMA");
                AddPlot(new Stroke() { Brush = Brushes.Transparent, Width = 1, DashStyleHelper = DashStyleHelper.Solid }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
                tema1 = TEMA(Inputs[0], period);
                tema2 = TEMA(tema1, period);
            }
		}

		protected override void OnBarUpdate()
		{
            ZeroTEMA[0] = 2 * tema1[0] - tema2[0];

            //OnStartup
            if (CurrentBar < 1)
            {
                alphaBarClr = 25 * opacity;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;
            }

            if (CurrentBar > 1)
            {
                Trend[0] = 0;
                if (ZeroTEMA[0] > ZeroTEMA[1])
                    Trend[0] = 1;
                else if (ZeroTEMA[0] < ZeroTEMA[1])
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
        public Series<double> ZeroTEMA
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
		private AuZeroLagTEMA[] cacheAuZeroLagTEMA;
		public AuZeroLagTEMA AuZeroLagTEMA(int period)
		{
			return AuZeroLagTEMA(Input, period);
		}

		public AuZeroLagTEMA AuZeroLagTEMA(ISeries<double> input, int period)
		{
			if (cacheAuZeroLagTEMA != null)
				for (int idx = 0; idx < cacheAuZeroLagTEMA.Length; idx++)
					if (cacheAuZeroLagTEMA[idx] != null && cacheAuZeroLagTEMA[idx].Period == period && cacheAuZeroLagTEMA[idx].EqualsInput(input))
						return cacheAuZeroLagTEMA[idx];
			return CacheIndicator<AuZeroLagTEMA>(new AuZeroLagTEMA(){ Period = period }, input, ref cacheAuZeroLagTEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuZeroLagTEMA AuZeroLagTEMA(int period)
		{
			return indicator.AuZeroLagTEMA(Input, period);
		}

		public Indicators.AuZeroLagTEMA AuZeroLagTEMA(ISeries<double> input , int period)
		{
			return indicator.AuZeroLagTEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuZeroLagTEMA AuZeroLagTEMA(int period)
		{
			return indicator.AuZeroLagTEMA(Input, period);
		}

		public Indicators.AuZeroLagTEMA AuZeroLagTEMA(ISeries<double> input , int period)
		{
			return indicator.AuZeroLagTEMA(input, period);
		}
	}
}

#endregion
