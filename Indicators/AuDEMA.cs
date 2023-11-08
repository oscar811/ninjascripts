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
	/// Double Exponential Moving Average with Paintbar option - Modified by RAYKO
	/// </summary>
	public class AuDEMA : Indicator
	{
        #region Variables
        private EMA ema;
		private EMA emaEma;

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
                Description                 = @"Double Exponential Moving Average with Paintbar option - Modified by RAYKO";
                Name						= "AuDEMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 14;

                ShowTransparentPlotsInDataBox = true;

                AddPlot(Brushes.Orange, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameDEMA);
                AddPlot(new Stroke() { Brush = Brushes.Transparent}, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
				ema		= EMA(Inputs[0], Period);
				emaEma	= EMA(ema, Period);
			}
		}
		
		protected override void OnBarUpdate()
		{
			Value[0] = 2 * ema[0] -  emaEma[0];

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
        public Series<double> Trend
        {
            get { return Values[1]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Gen. Parameters", Order = 0)]
        public int Period
        { get; set; }

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
		private AuDEMA[] cacheAuDEMA;
		public AuDEMA AuDEMA(int period)
		{
			return AuDEMA(Input, period);
		}

		public AuDEMA AuDEMA(ISeries<double> input, int period)
		{
			if (cacheAuDEMA != null)
				for (int idx = 0; idx < cacheAuDEMA.Length; idx++)
					if (cacheAuDEMA[idx] != null && cacheAuDEMA[idx].Period == period && cacheAuDEMA[idx].EqualsInput(input))
						return cacheAuDEMA[idx];
			return CacheIndicator<AuDEMA>(new AuDEMA(){ Period = period }, input, ref cacheAuDEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuDEMA AuDEMA(int period)
		{
			return indicator.AuDEMA(Input, period);
		}

		public Indicators.AuDEMA AuDEMA(ISeries<double> input , int period)
		{
			return indicator.AuDEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuDEMA AuDEMA(int period)
		{
			return indicator.AuDEMA(Input, period);
		}

		public Indicators.AuDEMA AuDEMA(ISeries<double> input , int period)
		{
			return indicator.AuDEMA(input, period);
		}
	}
}

#endregion
