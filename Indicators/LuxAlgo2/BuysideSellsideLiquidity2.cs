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
namespace NinjaTrader.NinjaScript.Indicators.LuxAlgo2
{
    public class BuysideSellsideLiquidity2 : Indicator
    {
        private ZZ aZZ;

        private liq[] b_liq_B;

        private liq[] b_liq_S;

        private Rectangle[] b_liq_V;

        private Series<int> dir;

        private Series<int> x1;

        private Series<int> x2;

        private Series<double> y1;

        private Series<double> y2;

        private Series<bool> bull;

        private Series<bool> bear;

        private SimpleFont font;

        private int maxSize;

        private double liqMar;

        protected static PineLib Pine;

        public override string DisplayName => "Lux - BSL-SSL - 2";

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Detection Length", Description = "Detection Length", Order = 1, GroupName = "Liquidity Detection")]
        [Range(3, 30)]
        public int liqLen { get; set; }

        [Range(1, 40)]
        [Display(Name = "Margin", Description = "Margin", Order = 2, GroupName = "Liquidity Detection")]
        [NinjaScriptProperty]
        public double LiqMar { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Buyside Liquidity Zones", Description = "Buyside Liquidity Zones, Margin", Order = 3, GroupName = "Liquidity Detection")]
        public bool liqBuy { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Margin Buy", Description = "Margin", Order = 4, GroupName = "Liquidity Detection")]
        [Range(1.5, 10.0)]
        public double marBuy { get; set; }

        [Display(Name = "Color Buy", Description = "Color Buy", Order = 5, GroupName = "Liquidity Detection")]
        [NinjaScriptProperty]
        [XmlIgnore]
        public Brush cLIQ_B { get; set; }

        [Browsable(false)]
        public string cLIQ_BSerializable
        {
            get
            {
                return Serialize.BrushToString(cLIQ_B);
            }
            set
            {
                cLIQ_B = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Sellside Liquidity Zones", Description = "Sellside Liquidity Zones, Margin", Order = 6, GroupName = "Liquidity Detection")]
        public bool liqSel { get; set; }

        [NinjaScriptProperty]
        [Range(1.5, 10.0)]
        [Display(Name = "Margin Sell", Description = "Margin", Order = 7, GroupName = "Liquidity Detection")]
        public double marSel { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Color Sell", Description = "Color Sell", Order = 8, GroupName = "Liquidity Detection")]
        public Brush cLIQ_S { get; set; }

        [Browsable(false)]
        public string cLIQ_SSerializable
        {
            get
            {
                return Serialize.BrushToString(cLIQ_S);
            }
            set
            {
                cLIQ_S = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Liquidity Voids, Bullish", Description = "Liquidity Voids, Bullish", Order = 9, GroupName = "Liquidity Detection")]
        public bool lqVoid { get; set; }

        [XmlIgnore]
        [Display(Name = "Color Void Buy", Description = "Color Void Buy", Order = 10, GroupName = "Liquidity Detection")]
        [NinjaScriptProperty]
        public Brush cLQV_B { get; set; }

        [Browsable(false)]
        public string cLQV_BSerializable
        {
            get
            {
                return Serialize.BrushToString(cLQV_B);
            }
            set
            {
                cLQV_B = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Color Void Sell", Description = "Color Void Sell", Order = 11, GroupName = "Liquidity Detection")]
        [XmlIgnore]
        public Brush cLQV_S { get; set; }

        [Browsable(false)]
        public string cLQV_SSerializable
        {
            get
            {
                return Serialize.BrushToString(cLQV_S);
            }
            set
            {
                cLQV_S = Serialize.StringToBrush(value);
            }
        }

        [Display(Name = "Mode", Description = "Mode", Order = 13, GroupName = "Liquidity Detection")]
        [NinjaScriptProperty]
        public LuxBSLMode mode { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Visible Levels", Description = "Visible Levels", Order = 14, GroupName = "Liquidity Detection")]
        [Range(1, 50)]
        public int visLiq { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public double atr => ATR(10)[0];

        [Browsable(false)]
        [XmlIgnore]
        public double atr200 => ATR(200)[0];

        [XmlIgnore]
        [Browsable(false)]
        public bool per
        {
            get
            {
                if (mode != 0)
                {
                    return true;
                }

                return CurrentBar > Count - 500;
            }
        }

        #endregion

        public Series<int> Lq_Breach;
        public Series<int> Fvg;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "BSL SSL";
                Name = "Buyside & Sellside Liquidity";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                liqLen = 7;
                LiqMar = 6.9;
                liqBuy = true;
                marBuy = 2.3;
                cLIQ_B = Brushes.Green;
                liqSel = true;
                marSel = 2.3;
                cLIQ_S = Brushes.Red;
                lqVoid = false;
                cLQV_B = Brushes.Green;
                cLQV_S = Brushes.Red;
                mode = LuxBSLMode.Present;
                visLiq = 3;
            }
            else if (State != State.Configure && State == State.DataLoaded)
            {
                maxSize = 50;
                Pine = new PineLib(this, this, DrawObjects);
                aZZ = new ZZ(new int[maxSize], new int[maxSize], new double[maxSize]);
                b_liq_B = new liq[1]
                {
                new liq()
                };
                b_liq_S = new liq[1]
                {
                new liq()
                };
                b_liq_V = new Rectangle[0];
                dir = new Series<int>(this);
                x1 = new Series<int>(this);
                x2 = new Series<int>(this);
                y1 = new Series<double>(this);
                y2 = new Series<double>(this);
                bull = new Series<bool>(this);
                bear = new Series<bool>(this);
                font = new SimpleFont("Arial", 10);
                liqMar = 10.0 / LiqMar;

                Lq_Breach = new Series<int>(this);
                Fvg = new Series<int>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < liqLen)
            {
                return;
            }

            x2[0] = CurrentBar - 1;
            x1[0] = x1[1];
            dir[0] = dir[1];
            y1[0] = y1[1];
            y2[0] = y2[1];
            double num = Pine.TA.PivotHigh(High, liqLen, 1);
            double num2 = Pine.TA.PivotLow(Low, liqLen, 1);
            if (!double.IsNaN(num))
            {
                dir[0] = aZZ.d[0];
                x1[0] = aZZ.x[0];
                y1[0] = aZZ.y[0];
                y2[0] = High[1];
                if (dir[0] < 1)
                {
                    aZZ.in_out(1, x2[0], y2[0]);
                }
                else if (dir[0] == 1 && num > y1[0])
                {
                    aZZ.x[0] = x2[0];
                    aZZ.y[0] = y2[0];
                }

                if (per)
                {
                    int num3 = 0;
                    double num4 = 0.0;
                    int num5 = 0;
                    double num6 = 0.0;
                    double num7 = 10000000.0;
                    for (int i = 0; i < maxSize; i++)
                    {
                        if (aZZ.d[i] != 1)
                        {
                            continue;
                        }

                        if (aZZ.y[i] > num + atr / liqMar)
                        {
                            break;
                        }

                        if (aZZ.y[i] > num - atr / liqMar && aZZ.y[i] < num + atr / liqMar)
                        {
                            num3++;
                            num5 = aZZ.x[i];
                            num4 = aZZ.y[i];
                            if (aZZ.y[i] > num6)
                            {
                                num6 = aZZ.y[i];
                            }

                            if (aZZ.y[i] < num7)
                            {
                                num7 = aZZ.y[i];
                            }
                        }
                    }

                    if (num3 > 2)
                    {
                        liq liq = b_liq_B[0];
                        if (num5 == Pine.Box.GetLeft(ref liq.bx))
                        {
                            Pine.Box.SetTop(ref liq.bx, Pine.Math.Avg<double>(num6, num7) + atr / liqMar);
                            Pine.Box.SetRightBottom(ref liq.bx, CurrentBar + 10, Pine.Math.Avg<double>(num6, num7) - atr / liqMar);
                        }
                        else
                        {
                            Pine.Array.UnshiftElement(ref b_liq_B, new liq(Pine.Box.New(num5, Pine.Math.Avg<double>(num6, num7) + atr / liqMar, CurrentBar + 10, Pine.Math.Avg<double>(num6, num7) - atr / liqMar), Pine.Box.New(), Pine.Label.New(num5, num4, "Buyside liquidity", null, null, 0, 1, cLIQ_B, font, TextAlignment.Left, 10), brZ: false, brL: false, Pine.Line.New(num5, num4, CurrentBar - 1, num4, cLIQ_B), Pine.Line.New(CurrentBar - 1, num4, 0, num4, cLIQ_B, DashStyleHelper.Dot)));
                            Pine.Alerts.DoAlert("buyside liquidity level detected/updated for " + Instrument.FullName);
                        }

                        if (b_liq_B.Length > visLiq)
                        {
                            liq liq2 = Pine.Array.PopElement(ref b_liq_B);
                            Pine.Box.Delete(liq2.bx);
                            Pine.Box.Delete(liq2.bxz);
                            Pine.Label.Delete(liq2.bxt);
                            Pine.Line.Delete(liq2.ln);
                            Pine.Line.Delete(liq2.lne);
                        }
                    }
                }
            }

            if (!double.IsNaN(num2))
            {
                dir[0] = aZZ.d[0];
                x1[0] = aZZ.x[0];
                y1[0] = aZZ.y[0];
                y2[0] = Low[1];
                if (dir[0] > -1)
                {
                    aZZ.in_out(-1, x2[0], y2[0]);
                }
                else if (dir[0] == -1 && num2 < y1[0])
                {
                    aZZ.x[0] = x2[0];
                    aZZ.y[0] = y2[0];
                }

                if (per)
                {
                    int num8 = 0;
                    double num9 = 0.0;
                    int num10 = 0;
                    double num11 = 0.0;
                    double num12 = 10000000.0;
                    for (int j = 0; j < maxSize - 1; j++)
                    {
                        if (aZZ.d[j] != -1)
                        {
                            continue;
                        }

                        if (aZZ.y[j] < num2 - atr / liqMar)
                        {
                            break;
                        }

                        if (aZZ.y[j] > num2 - atr / liqMar && aZZ.y[j] < num2 + atr / liqMar)
                        {
                            num8++;
                            num10 = aZZ.x[j];
                            num9 = aZZ.y[j];
                            if (aZZ.y[j] > num11)
                            {
                                num11 = aZZ.y[j];
                            }

                            if (aZZ.y[j] < num12)
                            {
                                num12 = aZZ.y[j];
                            }
                        }
                    }

                    if (num8 > 2)
                    {
                        liq liq3 = b_liq_S[0];
                        if (num10 == Pine.Box.GetLeft(ref liq3.bx))
                        {
                            Pine.Box.SetTop(ref liq3.bx, Pine.Math.Avg<double>(num11, num12) + atr / liqMar);
                            Pine.Box.SetRightBottom(ref liq3.bx, CurrentBar + 10, Pine.Math.Avg<double>(num11, num12) - atr / liqMar);
                        }
                        else
                        {
                            PineLib.PineArray array = Pine.Array;
                            ref liq[] array2 = ref b_liq_S;
                            Rectangle bx = Pine.Box.New(num10, Pine.Math.Avg<double>(num11, num12) + atr / liqMar, CurrentBar + 10, Pine.Math.Avg<double>(num11, num12) - atr / liqMar);
                            Rectangle bxz = Pine.Box.New();
                            PineLib.PineLabel label = Pine.Label;
                            int num13 = num10;
                            double num14 = num9;
                            Brush textcolor = cLIQ_S;
                            array.UnshiftElement(ref array2, new liq(bx, bxz, label.New(num13, num14, "Sellside liquidity", null, null, 0, -1, textcolor, font, TextAlignment.Left, 10), brZ: false, brL: false, Pine.Line.New(num10, num9, CurrentBar - 1, num9, cLIQ_S), Pine.Line.New(CurrentBar - 1, num9, 0, num9, cLIQ_S, DashStyleHelper.Dot)));
                            Pine.Alerts.DoAlert("sellside liquidity level detected/updated for " + Instrument.FullName, 1);
                        }

                        if (b_liq_S.Length > visLiq)
                        {
                            liq liq4 = Pine.Array.PopElement(ref b_liq_S);
                            Pine.Box.Delete(liq4.bx);
                            Pine.Box.Delete(liq4.bxz);
                            Pine.Label.Delete(liq4.bxt);
                            Pine.Line.Delete(liq4.ln);
                            Pine.Line.Delete(liq4.lne);
                        }
                    }
                }
            }

            for (int k = 0; k < b_liq_B.Length; k++)
            {
                liq liq5 = b_liq_B[k];
                if (!liq5.brL)
                {
                    Pine.Line.SetX2(ref liq5.lne, CurrentBar);
                    if (High[0] > Pine.Box.GetTop(ref liq5.bx))
                    {
                        liq5.brL = true;
                        liq5.brZ = true;
                        Pine.Alerts.DoAlert("buyside liquidity level breached for " + Instrument.FullName, 2);
                        Pine.Box.SetLeftTop(ref liq5.bxz, CurrentBar - 1, Math.Min(Pine.Line.GetY1(ref liq5.ln) + marBuy * atr, High[0]));
                        Pine.Box.SetRightBottom(ref liq5.bxz, CurrentBar + 1, Pine.Line.GetY1(ref liq5.ln));
                        Pine.Box.SetBgColor(ref liq5.bxz, cLIQ_B);
                        Pine.Box.SetOpacity(ref liq5.bxz, liqBuy ? 25 : 0);

                        Lq_Breach[0] = 1;
                    }
                }
                else
                {
                    if (!liq5.brZ)
                    {
                        continue;
                    }

                    if (Low[0] > Pine.Line.GetY1(ref liq5.ln) - marBuy * atr && High[0] < Pine.Line.GetY1(ref liq5.ln) + marBuy * atr)
                    {
                        Pine.Box.SetRight(ref liq5.bxz, CurrentBar + 1);
                        Pine.Box.SetTop(ref liq5.bxz, Math.Max(High[0], Pine.Box.GetTop(ref liq5.bxz)));
                        if (liqBuy)
                        {
                            Pine.Line.SetX2(ref liq5.lne, CurrentBar + 1);
                        }
                    }
                    else
                    {
                        liq5.brZ = false;
                    }
                }
            }

            for (int l = 0; l < b_liq_S.Length; l++)
            {
                liq liq6 = b_liq_S[l];
                if (!liq6.brL)
                {
                    Pine.Line.SetX2(ref liq6.lne, CurrentBar);
                    if (Low[0] < Pine.Box.GetBottom(ref liq6.bx))
                    {
                        liq6.brL = true;
                        liq6.brZ = true;
                        Pine.Alerts.DoAlert("sellside liquidity level breached for " + Instrument.FullName, 3);
                        Pine.Box.SetLeftTop(ref liq6.bxz, CurrentBar - 1, Pine.Line.GetY1(ref liq6.ln));
                        Pine.Box.SetRightBottom(ref liq6.bxz, CurrentBar + 1, Math.Max(Pine.Line.GetY1(ref liq6.ln) - marSel * atr, Low[0]));
                        Pine.Box.SetBgColor(ref liq6.bxz, cLIQ_S);
                        Pine.Box.SetOpacity(ref liq6.bxz, liqSel ? 25 : 0);

                        Lq_Breach[0] = -1;
                    }
                }
                else
                {
                    if (!liq6.brZ)
                    {
                        continue;
                    }

                    if (Low[0] > Pine.Line.GetY1(ref liq6.ln) - marSel * atr && High[0] < Pine.Line.GetY1(ref liq6.ln) + marSel * atr)
                    {
                        Pine.Box.SetRightBottom(ref liq6.bxz, CurrentBar + 1, Math.Min(Low[0], Pine.Box.GetBottom(ref liq6.bxz)));
                        if (liqSel)
                        {
                            Pine.Line.SetX2(ref liq6.lne, CurrentBar + 1);
                        }
                    }
                    else
                    {
                        liq6.brZ = false;
                    }
                }
            }

            if (lqVoid && per)
            {
                bull[0] = Low[0] - High[2] > atr200 && Low[0] > High[2] && Close[1] > High[2];
                bear[0] = Low[2] - High[0] > atr200 && High[0] < Low[2] && Close[1] < Low[2];
                if (bull[0])
                {
                    int num15 = 13;
                    if (bull[1])
                    {
                        double num16 = Math.Abs(Low[0] - Low[1]) / (double)num15;
                        for (int m = 0; m < num15; m++)
                        {
                            Pine.Array.PushElement(ref b_liq_V, Pine.Box.New(CurrentBar - 2, Low[1] + (double)m * num16, CurrentBar, Low[1] + (double)(m + 1) * num16, null, 1, DashStyleHelper.Solid, cLQV_B, 10));

                            Fvg[0] = 1;
                        }
                    }
                    else
                    {
                        double num17 = Math.Abs(Low[0] - High[2]) / (double)num15;
                        for (int n = 0; n < num15; n++)
                        {
                            Pine.Array.PushElement(ref b_liq_V, Pine.Box.New(CurrentBar - 2, High[2] + (double)n * num17, CurrentBar, High[2] + (double)(n + 1) * num17, null, 1, DashStyleHelper.Solid, cLQV_B, 10));

                            Fvg[0] = 1;
                        }
                    }
                }

                if (bear[0])
                {
                    int num18 = 13;
                    if (bear[1])
                    {
                        double num19 = Math.Abs(High[1] - High[0]) / (double)num18;
                        for (int num20 = 0; num20 < num18; num20++)
                        {
                            Pine.Array.PushElement(ref b_liq_V, Pine.Box.New(CurrentBar - 2, High[0] + (double)num20 * num19, CurrentBar, High[0] + (double)(num20 + 1) * num19, null, 1, DashStyleHelper.Solid, cLQV_S, 10));

                            Fvg[0] = -1;
                        }
                    }
                    else
                    {
                        double num21 = Math.Abs(Low[2] - High[0]) / (double)num18;
                        for (int num22 = 0; num22 < num18; num22++)
                        {
                            Pine.Array.PushElement(ref b_liq_V, Pine.Box.New(CurrentBar - 2, High[0] + (double)num22 * num21, CurrentBar, High[0] + (double)(num22 + 1) * num21, null, 1, DashStyleHelper.Solid, cLQV_S, 10));

                            Fvg[0] = -1;
                        }
                    }
                }
            }

            if (b_liq_V.Length <= 0)
            {
                return;
            }

            int num23 = b_liq_V.Length;
            for (int num24 = num23 - 1; num24 >= 0; num24--)
            {
                if (num24 < b_liq_V.Length)
                {
                    Rectangle box = b_liq_V[num24];
                    double num25 = (Pine.Box.GetBottom(ref box) + Pine.Box.GetTop(ref box)) / 2.0;
                    if (Math.Sign(Close[1] - num25) != Math.Sign(Close[0] - num25) || Math.Sign(Close[1] - num25) != Math.Sign(Low[0] - num25) || Math.Sign(Close[1] - num25) != Math.Sign(High[0] - num25))
                    {
                        Pine.Array.RemoveElement(ref b_liq_V, num24);
                    }
                    else
                    {
                        Pine.Box.SetRight(ref box, CurrentBar + 1);
                        _ = CurrentBar - Pine.Box.GetLeft(ref box);
                        _ = 21;
                    }
                }
            }
        }

        public class ZZ
        {
            public int[] d;

            public int[] x;

            public double[] y;

            public ZZ(int[] d = null, int[] x = null, double[] y = null)
            {
                this.d = d ?? new int[0];
                this.x = x ?? new int[0];
                this.y = y ?? new double[0];
            }

            public void in_out(int _d, int _x, double _y)
            {
                Pine.Array.UnshiftElement(ref d, _d);
                Pine.Array.UnshiftElement(ref x, _x);
                Pine.Array.UnshiftElement(ref y, _y);
                Pine.Array.PopElement(ref d);
                Pine.Array.PopElement(ref x);
                Pine.Array.PopElement(ref y);
            }
        }

        public class liq
        {
            public Rectangle bx;

            public Rectangle bxz;

            public Text bxt;

            public bool brZ;

            public bool brL;

            public NinjaTrader.NinjaScript.DrawingTools.Line ln;

            public NinjaTrader.NinjaScript.DrawingTools.Line lne;

            public liq(Rectangle bx = null, Rectangle bxz = null, Text bxt = null, bool brZ = false, bool brL = false, NinjaTrader.NinjaScript.DrawingTools.Line ln = null, NinjaTrader.NinjaScript.DrawingTools.Line lne = null)
            {
                this.bx = bx ?? Pine.Box.New();
                this.bxz = bxz ?? Pine.Box.New();
                this.bxt = bxt ?? Pine.Label.New();
                this.brZ = brZ;
                this.brL = brL;
                this.ln = ln ?? Pine.Line.New();
                this.lne = lne ?? Pine.Line.New();
            }
        }

    }

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LuxAlgo2.BuysideSellsideLiquidity2[] cacheBuysideSellsideLiquidity2;
		public LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			return BuysideSellsideLiquidity2(Input, liqLen, liqMar, liqBuy, marBuy, cLIQ_B, liqSel, marSel, cLIQ_S, lqVoid, cLQV_B, cLQV_S, mode, visLiq);
		}

		public LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(ISeries<double> input, int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			if (cacheBuysideSellsideLiquidity2 != null)
				for (int idx = 0; idx < cacheBuysideSellsideLiquidity2.Length; idx++)
					if (cacheBuysideSellsideLiquidity2[idx] != null && cacheBuysideSellsideLiquidity2[idx].liqLen == liqLen && cacheBuysideSellsideLiquidity2[idx].LiqMar == liqMar && cacheBuysideSellsideLiquidity2[idx].liqBuy == liqBuy && cacheBuysideSellsideLiquidity2[idx].marBuy == marBuy && cacheBuysideSellsideLiquidity2[idx].cLIQ_B == cLIQ_B && cacheBuysideSellsideLiquidity2[idx].liqSel == liqSel && cacheBuysideSellsideLiquidity2[idx].marSel == marSel && cacheBuysideSellsideLiquidity2[idx].cLIQ_S == cLIQ_S && cacheBuysideSellsideLiquidity2[idx].lqVoid == lqVoid && cacheBuysideSellsideLiquidity2[idx].cLQV_B == cLQV_B && cacheBuysideSellsideLiquidity2[idx].cLQV_S == cLQV_S && cacheBuysideSellsideLiquidity2[idx].mode == mode && cacheBuysideSellsideLiquidity2[idx].visLiq == visLiq && cacheBuysideSellsideLiquidity2[idx].EqualsInput(input))
						return cacheBuysideSellsideLiquidity2[idx];
			return CacheIndicator<LuxAlgo2.BuysideSellsideLiquidity2>(new LuxAlgo2.BuysideSellsideLiquidity2(){ liqLen = liqLen, LiqMar = liqMar, liqBuy = liqBuy, marBuy = marBuy, cLIQ_B = cLIQ_B, liqSel = liqSel, marSel = marSel, cLIQ_S = cLIQ_S, lqVoid = lqVoid, cLQV_B = cLQV_B, cLQV_S = cLQV_S, mode = mode, visLiq = visLiq }, input, ref cacheBuysideSellsideLiquidity2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			return indicator.BuysideSellsideLiquidity2(Input, liqLen, liqMar, liqBuy, marBuy, cLIQ_B, liqSel, marSel, cLIQ_S, lqVoid, cLQV_B, cLQV_S, mode, visLiq);
		}

		public Indicators.LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(ISeries<double> input , int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			return indicator.BuysideSellsideLiquidity2(input, liqLen, liqMar, liqBuy, marBuy, cLIQ_B, liqSel, marSel, cLIQ_S, lqVoid, cLQV_B, cLQV_S, mode, visLiq);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			return indicator.BuysideSellsideLiquidity2(Input, liqLen, liqMar, liqBuy, marBuy, cLIQ_B, liqSel, marSel, cLIQ_S, lqVoid, cLQV_B, cLQV_S, mode, visLiq);
		}

		public Indicators.LuxAlgo2.BuysideSellsideLiquidity2 BuysideSellsideLiquidity2(ISeries<double> input , int liqLen, double liqMar, bool liqBuy, double marBuy, Brush cLIQ_B, bool liqSel, double marSel, Brush cLIQ_S, bool lqVoid, Brush cLQV_B, Brush cLQV_S, LuxBSLMode mode, int visLiq)
		{
			return indicator.BuysideSellsideLiquidity2(input, liqLen, liqMar, liqBuy, marBuy, cLIQ_B, liqSel, marSel, cLIQ_S, lqVoid, cLQV_B, cLQV_S, mode, visLiq);
		}
	}
}

#endregion
