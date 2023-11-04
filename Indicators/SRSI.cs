#region Using declarations
using NinjaTrader.Gui;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class SRSI : Indicator
    {
        #region Variables
        private int period = 14;
        private int basePeriod = 6;
        private int smooth = 10;
        private double k = 0.0;
        private double up = 0.0;
        private double down = 0.0;
        private double sumUp = 0.0;
        private double sumDown = 0.0;
        private double pSumUp = 0.0;
        private double pSumDown = 0.0;
        private double avgUp = 0.0;
        private double avgDown = 0.0;
        private double pAvgUp = 0.0;
        private double pAvgDown = 0.0;
        private double rsi = 0.0;
        private double rsiAvg = 0.0;
        private EMA baseline;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"The SRSI (Slow Relative Strength Index) is a price-following oscillator which was presented by Vitali Apirine in the April 2015 issue of the TASC magazine.";
                Name = "SRSI";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                AddPlot(new Stroke(Brushes.LimeGreen, 2), PlotStyle.Line, "SRSI");
                AddPlot(new Stroke(Brushes.DarkOrange, 2), PlotStyle.Line, "Avg");

                AddLine(Brushes.MediumVioletRed, 30, "Lower");
                AddLine(Brushes.MediumPurple, 70, "Upper");

                //BarsRequiredToTrade = period + 3 * basePeriod;

            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                k = 2.0 / (1 + smooth);
                baseline = EMA(Input, basePeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < period)
            {
                if (CurrentBar == 0)
                {
                    up = 0.0;
                    down = 0.0;
                    sumUp = 0.0;
                    sumDown = 0.0;
                    SlowRSI[0] = (50);
                    Avg[0] = (50);
                }
                else if (CurrentBar < period - 1)
                {
                    if (IsFirstTickOfBar)
                    {
                        pSumUp = sumUp;
                        pSumDown = sumDown;
                    }
                    up = Math.Max(Input[0] - baseline[0], 0);
                    down = Math.Max(baseline[0] - Input[0], 0);
                    sumUp = pSumUp + up;
                    sumDown = pSumDown + down;
                    SlowRSI[0] = (50);
                    Avg[0] = (50);
                }
                else
                {
                    up = Math.Max(Input[0] - baseline[0], 0);
                    down = Math.Max(baseline[0] - Input[0], 0);
                    avgUp = (sumUp + up) / period;
                    avgDown = (sumDown + down) / period;
                    rsi = (avgUp == 0 && avgDown == 0) ? 50 : 100 * avgUp / (avgUp + avgDown);
                    rsiAvg = k * rsi + (1 - k) * Avg[1];
                    SlowRSI[0] = (rsi);
                    Avg[0] = (rsiAvg);
                }
                return;
            }

            if (IsFirstTickOfBar)
            {
                pAvgUp = avgUp;
                pAvgDown = avgDown;
            }

            up = Math.Max(Input[0] - baseline[0], 0);
            down = Math.Max(baseline[0] - Input[0], 0);
            avgDown = (pAvgDown * (period - 1) + down) / period;
            avgUp = (pAvgUp * (period - 1) + up) / period;
            rsi = (avgUp == 0 && avgDown == 0) ? 50 : 100 * avgUp / (avgUp + avgDown);
            rsiAvg = k * rsi + (1 - k) * Avg[1];
            SlowRSI[0] = (rsi);
            Avg[0] = (rsiAvg);
        }

        #region Properties
        /// <summary>
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> SlowRSI
        {
            get { return Values[0]; }
        }

        /// <summary>
        /// </summary>
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> Avg
        {
            get { return Values[1]; }
        }

        /// <summary>
        /// </summary>
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SRSI lookback period", Order = 0, Description = "Lookback period for the SRSI in number of bars", GroupName = "Parameters")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
        }

        /// <summary>
        /// </summary>
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "EMA period", Order = 1, Description = "Period of the underlyling EMA", GroupName = "Parameters")]
        public int BasePeriod
        {
            get { return basePeriod; }
            set { basePeriod = Math.Max(1, value); }
        }

        /// <summary>
        /// </summary>
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Signal line smoothing", Order = 2, Description = "Smoothing period used to calculate the signal line", GroupName = "Parameters")]
        public int Smooth
        {
            get { return smooth; }
            set { smooth = Math.Max(1, value); }
        }
        #endregion


    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SRSI[] cacheSRSI;
		public SRSI SRSI(int period, int basePeriod, int smooth)
		{
			return SRSI(Input, period, basePeriod, smooth);
		}

		public SRSI SRSI(ISeries<double> input, int period, int basePeriod, int smooth)
		{
			if (cacheSRSI != null)
				for (int idx = 0; idx < cacheSRSI.Length; idx++)
					if (cacheSRSI[idx] != null && cacheSRSI[idx].Period == period && cacheSRSI[idx].BasePeriod == basePeriod && cacheSRSI[idx].Smooth == smooth && cacheSRSI[idx].EqualsInput(input))
						return cacheSRSI[idx];
			return CacheIndicator<SRSI>(new SRSI(){ Period = period, BasePeriod = basePeriod, Smooth = smooth }, input, ref cacheSRSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SRSI SRSI(int period, int basePeriod, int smooth)
		{
			return indicator.SRSI(Input, period, basePeriod, smooth);
		}

		public Indicators.SRSI SRSI(ISeries<double> input , int period, int basePeriod, int smooth)
		{
			return indicator.SRSI(input, period, basePeriod, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SRSI SRSI(int period, int basePeriod, int smooth)
		{
			return indicator.SRSI(Input, period, basePeriod, smooth);
		}

		public Indicators.SRSI SRSI(ISeries<double> input , int period, int basePeriod, int smooth)
		{
			return indicator.SRSI(input, period, basePeriod, smooth);
		}
	}
}

#endregion
