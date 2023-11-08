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
	/// The AuWMA (Weighted Moving Average) is a Moving Average indicator that shows the average 
	/// value of a security's price over a period of time with special emphasis on the more recent 
	/// portions of the time period under analysis as opposed to the earlier.
	/// </summary>
	public class AuWMA : Indicator
	{
        #region Variables
        private int		myPeriod;
		private double	priorSum;
		private double	priorWsum;
		private double	sum;
		private double	wsum;

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
				
				Description                 = @"AuWMA (Weighted Moving Average) with Paintbar option - Modified by RAYKO";
                Name						= "AuWMA";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

                ShowTransparentPlotsInDataBox = true;

                AddPlot(Brushes.Orange, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameWMA);
                AddPlot(new Stroke() { Brush = Brushes.Transparent }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
				priorSum	= 0;
				priorWsum	= 0;
				sum			= 0;
				wsum		= 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				if (CurrentBar == 0)
					Value[0] = Input[0];
				else
				{
					int back = Math.Min(Period - 1, CurrentBar);
					double val = 0;
					int weight = 0;
					for (int idx = back; idx >= 0; idx--)
					{
						val += (idx + 1) * Input[back - idx];
						weight += (idx + 1);
					}
					Value[0] = val / weight;
				}
			}
			else
			{
				if (IsFirstTickOfBar)
				{
					priorWsum = wsum;
					priorSum = sum;
					myPeriod = Math.Min(CurrentBar + 1, Period);
				}

				wsum = priorWsum - (CurrentBar >= Period ? priorSum : 0) + myPeriod * Input[0];
				sum = priorSum + Input[0] - (CurrentBar >= Period ? Input[Period] : 0);
				Value[0] = wsum / (0.5 * myPeriod * (myPeriod + 1));
			}

            if (CurrentBar < 1)
            {
                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;
            }
            else
            {
                alphaBarClr = 25 * opacity;

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
		private AuWMA[] cacheAuWMA;
		public AuWMA AuWMA(int period)
		{
			return AuWMA(Input, period);
		}

		public AuWMA AuWMA(ISeries<double> input, int period)
		{
			if (cacheAuWMA != null)
				for (int idx = 0; idx < cacheAuWMA.Length; idx++)
					if (cacheAuWMA[idx] != null && cacheAuWMA[idx].Period == period && cacheAuWMA[idx].EqualsInput(input))
						return cacheAuWMA[idx];
			return CacheIndicator<AuWMA>(new AuWMA(){ Period = period }, input, ref cacheAuWMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuWMA AuWMA(int period)
		{
			return indicator.AuWMA(Input, period);
		}

		public Indicators.AuWMA AuWMA(ISeries<double> input , int period)
		{
			return indicator.AuWMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuWMA AuWMA(int period)
		{
			return indicator.AuWMA(Input, period);
		}

		public Indicators.AuWMA AuWMA(ISeries<double> input , int period)
		{
			return indicator.AuWMA(input, period);
		}
	}
}

#endregion
