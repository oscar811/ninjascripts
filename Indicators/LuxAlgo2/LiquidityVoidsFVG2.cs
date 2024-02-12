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
using NinjaTrader.NinjaScript.Indicators.LuxAlgo;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.LuxAlgo2
{
    public class LiquidityVoidsFVG2 : Indicator
    {
        private Series<bool> bull;

        private Series<bool> bear;

        private Rectangle[] lqV;

        private Rectangle[] removedlqV;

        private static PineLib Pine;

        public override string DisplayName => "Lux-FVG-2";

        [NinjaScriptProperty]
        [Display(Name = "Mode", Description = "Mode", Order = 1, GroupName = "Parameters")]
        public LUXLVFVGMode mode { get; set; }

        [NinjaScriptProperty]
        [Range(100, 5000)]
        [Display(Name = "# Bars", Description = "- Historical, takes into account all data available to the user\n- Present, takes into account only the last X bars specified in the '# Bars' option", Order = 2, GroupName = "Parameters")]
        public int back { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Liquidity Voids Threshold", Description = "Act as a filter while detecting the Liquidity Voids. When set to 0 means no filtering is applied, increasing the value causes the script to check the width of the void compared to a fixed-length ATR value", Order = 3, GroupName = "Parameters")]
        public double lqTH { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish", Description = "Bullish", Order = 4, GroupName = "Parameters")]
        public Brush lqBC { get; set; }

        [Browsable(false)]
        public string lqBCSerializable
        {
            get
            {
                return Serialize.BrushToString(lqBC);
            }
            set
            {
                lqBC = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish", Description = "Bearish", Order = 5, GroupName = "Parameters")]
        public Brush lqSC { get; set; }

        [Browsable(false)]
        public string lqSCSerializable
        {
            get
            {
                return Serialize.BrushToString(lqSC);
            }
            set
            {
                lqSC = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Filled Liquidity Voids", Description = "Toggles the visibility of the Filled Liquidity Voids", Order = 7, GroupName = "Parameters")]
        public bool lqVF { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Filled Color", Description = "", Order = 8, GroupName = "Parameters")]
        public Brush lqFC { get; set; }

        [Browsable(false)]
        public string lqFCSerializable
        {
            get
            {
                return Serialize.BrushToString(lqFC);
            }
            set
            {
                lqFC = Serialize.StringToBrush(value);
            }
        }

        protected override void OnStateChange()
        {
            if (base.State == State.SetDefaults)
            {
                base.Description = "The Liquidity Voids (FVG) indicator is designed to detect liquidity voids/imbalances derived from the fair value gaps and highlight the distribution of the liquidity voids at specific price levels. Fair value gaps and liquidity voids are both indicators of sell-side and buy-side imbalance in trading. The only difference is how they are represented in the trading chart. Liquidity voids occur when the price moves sharply in one direction forming long-range candles that have little trading activity, whilst a fair value is a gap in price.";
                base.Name = "Liquidity Voids FVG";
                base.Calculate = Calculate.OnBarClose;
                base.IsOverlay = true;
                base.DisplayInDataBox = true;
                base.DrawOnPricePanel = true;
                base.DrawHorizontalGridLines = true;
                base.DrawVerticalGridLines = true;
                base.PaintPriceMarkers = true;
                base.ScaleJustification = ScaleJustification.Right;
                base.IsSuspendedWhileInactive = true;
                mode = LUXLVFVGMode.Historical;
                back = 360;
                lqTH = 0.5;
                lqBC = Brushes.Teal;
                lqSC = Brushes.Crimson;
                lqVF = true;
                lqFC = Brushes.Gray;
            }
            else if (base.State != State.Configure && base.State == State.DataLoaded)
            {
                Pine = new PineLib(this, this, base.DrawObjects);
                lqV = new Rectangle[0];
                removedlqV = new Rectangle[0];
                bull = new Series<bool>(this);
                bear = new Series<bool>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (base.CurrentBar < 2)
            {
                bull[0] = false;
                bear[0] = false;
                return;
            }

            bool num = mode != 0 || base.Count - base.CurrentBar <= back;
            double num2 = ATR(144)[0] * lqTH;
            bull[0] = bull[1];
            bear[0] = bear[1];
            if (num)
            {
                bull[0] = base.Low[0] - base.High[2] > num2 && base.Low[0] > base.High[2] && base.Close[1] > base.High[2];
                if (bull[0])
                {
                    int num3 = 13;
                    if (bull[1])
                    {
                        double num4 = Math.Abs(base.Low[0] - base.Low[1]) / (double)num3;
                        for (int i = 0; i < num3; i++)
                        {
                            Pine.Array.PushElement<Rectangle>(ref lqV, Pine.Box.New(base.CurrentBar - 2, base.Low[1] + (double)(i + 1) * num4, base.CurrentBar, base.Low[1] + (double)i * num4, null, 1, DashStyleHelper.Solid, lqBC));
                        }
                    }
                    else
                    {
                        double num5 = Math.Abs(base.Low[0] - base.High[2]) / (double)num3;
                        for (int j = 0; j < num3; j++)
                        {
                            Pine.Array.PushElement(ref lqV, Pine.Box.New(base.CurrentBar - 2, base.High[2] + (double)(j + 1) * num5, base.CurrentBar, base.High[2] + (double)j * num5, null, 1, DashStyleHelper.Solid, lqBC));
                        }
                    }
                }

                bear[0] = base.Low[2] - base.High[0] > num2 && base.High[0] < base.Low[2] && base.Close[1] < base.Low[2];
                if (bear[0])
                {
                    int num6 = 13;
                    if (bear[1])
                    {
                        double num7 = Math.Abs(base.High[1] - base.High[0]) / (double)num6;
                        for (int k = 0; k < num6; k++)
                        {
                            Pine.Array.PushElement(ref lqV, Pine.Box.New(base.CurrentBar - 2, base.High[0] + (double)(k + 1) * num7, base.CurrentBar, base.High[0] + (double)k * num7, null, 1, DashStyleHelper.Solid, lqSC));
                        }
                    }
                    else
                    {
                        double num8 = Math.Abs(base.Low[2] - base.High[0]) / (double)num6;
                        for (int l = 0; l < num6; l++)
                        {
                            Pine.Array.PushElement(ref lqV, Pine.Box.New(base.CurrentBar - 2, base.High[0] + (double)(l + 1) * num8, base.CurrentBar, base.High[0] + (double)l * num8, null, 1, DashStyleHelper.Solid, lqSC));
                        }
                    }
                }
            }

            if (lqV.Length == 0)
            {
                return;
            }

            for (int num9 = lqV.Length - 1; num9 >= 0; num9--)
            {
                if (num9 < lqV.Length)
                {
                    double top = Pine.Box.GetTop(ref lqV[num9]);
                    double bottom = Pine.Box.GetBottom(ref lqV[num9]);
                    if (base.High[0] > bottom && base.Low[0] < top)
                    {
                        if (lqVF)
                        {
                            Pine.Box.SetBgColor(ref lqV[num9], lqFC);
                        }
                        else
                        {
                            Pine.Box.Delete(ref lqV[num9]);
                        }

                        Pine.Array.PushElement(ref removedlqV, lqV[num9]);
                        Pine.Array.RemoveElement(ref lqV, num9);
                    }
                    else
                    {
                        Pine.Box.SetRight(ref lqV[num9], base.CurrentBar + 1);
                    }
                }
            }

            while (lqV.Length > 500)
            {
                Pine.Box.Delete(Pine.Array.ShiftElement(ref lqV));
            }

            while (removedlqV.Length > 500)
            {
                Pine.Box.Delete(Pine.Array.ShiftElement(ref removedlqV));
            }
        }
    }

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LuxAlgo2.LiquidityVoidsFVG2[] cacheLiquidityVoidsFVG2;
		public LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			return LiquidityVoidsFVG2(Input, mode, back, lqTH, lqBC, lqSC, lqVF, lqFC);
		}

		public LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(ISeries<double> input, LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			if (cacheLiquidityVoidsFVG2 != null)
				for (int idx = 0; idx < cacheLiquidityVoidsFVG2.Length; idx++)
					if (cacheLiquidityVoidsFVG2[idx] != null && cacheLiquidityVoidsFVG2[idx].mode == mode && cacheLiquidityVoidsFVG2[idx].back == back && cacheLiquidityVoidsFVG2[idx].lqTH == lqTH && cacheLiquidityVoidsFVG2[idx].lqBC == lqBC && cacheLiquidityVoidsFVG2[idx].lqSC == lqSC && cacheLiquidityVoidsFVG2[idx].lqVF == lqVF && cacheLiquidityVoidsFVG2[idx].lqFC == lqFC && cacheLiquidityVoidsFVG2[idx].EqualsInput(input))
						return cacheLiquidityVoidsFVG2[idx];
			return CacheIndicator<LuxAlgo2.LiquidityVoidsFVG2>(new LuxAlgo2.LiquidityVoidsFVG2(){ mode = mode, back = back, lqTH = lqTH, lqBC = lqBC, lqSC = lqSC, lqVF = lqVF, lqFC = lqFC }, input, ref cacheLiquidityVoidsFVG2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			return indicator.LiquidityVoidsFVG2(Input, mode, back, lqTH, lqBC, lqSC, lqVF, lqFC);
		}

		public Indicators.LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(ISeries<double> input , LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			return indicator.LiquidityVoidsFVG2(input, mode, back, lqTH, lqBC, lqSC, lqVF, lqFC);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			return indicator.LiquidityVoidsFVG2(Input, mode, back, lqTH, lqBC, lqSC, lqVF, lqFC);
		}

		public Indicators.LuxAlgo2.LiquidityVoidsFVG2 LiquidityVoidsFVG2(ISeries<double> input , LUXLVFVGMode mode, int back, double lqTH, Brush lqBC, Brush lqSC, bool lqVF, Brush lqFC)
		{
			return indicator.LiquidityVoidsFVG2(input, mode, back, lqTH, lqBC, lqSC, lqVF, lqFC);
		}
	}
}

#endregion
