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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The AuVWMA (Volume-Weighted Moving Average) returns the volume-weighted moving average
	/// for the specified price series and period. AuVWMA is similar to a Simple Moving Average
	/// (SMA), but each bar of data is weighted by the bar's Volume. AuVWMA places more significance 
	/// on the days with the largest volume and the least for the days with lowest volume for the period specified.
	/// </summary>
	public class AuVWMA : Indicator
	{
        #region Variables
        private double			priorVolPriceSum;
		private double			volPriceSum;
		private Series<double>	volSum;

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
				Description                 = @"AuVWMA (Volume-Weighted Moving Average) with Paintbar option - Modified by RAYKO";
                Name						= "AuVWMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				Period						= 14;

                ShowTransparentPlotsInDataBox = true;

                AddPlot(Brushes.Blue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameVWMA);
                AddPlot(new Stroke() { Brush = Brushes.Transparent }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
				volSum	= new Series<double>(this);
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				int numBars = Math.Min(CurrentBar, Period);

				double volPriceSum = 0;
				double volSum = 0;

				for (int i = 0; i < numBars; i++)
				{
					volPriceSum += Input[i] * Volume[i];
					volSum += Volume[i];
				}

				// Protect agains div by zero evilness
				if (volSum <= double.Epsilon)
					Value[0] = volPriceSum;
				else
					Value[0] = volPriceSum / volSum;
			}
			else
			{
				if (IsFirstTickOfBar)
					priorVolPriceSum = volPriceSum;

				double volume0 = Volume[0];
				double volumePeriod = Volume[Math.Min(Period, CurrentBar)];
				volPriceSum = priorVolPriceSum + Input[0] * volume0 - (CurrentBar >= Period ? Input[Period] * volumePeriod : 0);
				volSum[0] = volume0 + (CurrentBar > 0 ? volSum[1] : 0) - (CurrentBar >= Period ? volumePeriod : 0);
				Value[0] = volSum[0].ApproxCompare(0) == 0 ? volPriceSum : volPriceSum / volSum[0];
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
		private AuVWMA[] cacheAuVWMA;
		public AuVWMA AuVWMA(int period)
		{
			return AuVWMA(Input, period);
		}

		public AuVWMA AuVWMA(ISeries<double> input, int period)
		{
			if (cacheAuVWMA != null)
				for (int idx = 0; idx < cacheAuVWMA.Length; idx++)
					if (cacheAuVWMA[idx] != null && cacheAuVWMA[idx].Period == period && cacheAuVWMA[idx].EqualsInput(input))
						return cacheAuVWMA[idx];
			return CacheIndicator<AuVWMA>(new AuVWMA(){ Period = period }, input, ref cacheAuVWMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuVWMA AuVWMA(int period)
		{
			return indicator.AuVWMA(Input, period);
		}

		public Indicators.AuVWMA AuVWMA(ISeries<double> input , int period)
		{
			return indicator.AuVWMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuVWMA AuVWMA(int period)
		{
			return indicator.AuVWMA(Input, period);
		}

		public Indicators.AuVWMA AuVWMA(ISeries<double> input , int period)
		{
			return indicator.AuVWMA(input, period);
		}
	}
}

#endregion
