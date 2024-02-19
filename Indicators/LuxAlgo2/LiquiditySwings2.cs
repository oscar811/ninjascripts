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

namespace NinjaTrader.NinjaScript.Indicators.LuxAlgo2
{
    public class LiquiditySwings2 : Indicator
    {
        private SimpleFont font;

        private LSClass phClass;

        private LSClass plClass;

        private Rectangle ph_bx;

        private Rectangle pl_bx;

        protected static PineLib Pine;

        public override string DisplayName => "LiquiditySwings-2 (" + length + ")";

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Pivot Lookback", Order = 1, GroupName = "Parameters")]
        public int length { get; set; }

        [Display(Name = "Swing Area", Order = 2, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public LuxLSAreaType Area { get; set; }

        [Display(Name = "Intrabar Precision", Order = 3, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public bool IntraPrecision { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Intrabar Minutes", Order = 4, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public int IntrabarTf { get; set; }

        [Display(Name = "Filter Areas By", Order = 5, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public LuxLSFilterType FilterOptions { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Filter Value", Order = 6, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public int FilterValue { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Swing High", Order = 7, GroupName = "Style")]
        public bool ShowTop { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "High Color", Order = 8, GroupName = "Style")]
        public Brush TopCss { get; set; }

        [Browsable(false)]
        public string TopCssSerializable
        {
            get
            {
                return Serialize.BrushToString(TopCss);
            }
            set
            {
                TopCss = Serialize.StringToBrush(value);
            }
        }

        [Display(Name = "Swing Low", Order = 9, GroupName = "Style")]
        [NinjaScriptProperty]
        public bool ShowBtm { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Low Color", Order = 10, GroupName = "Style")]
        public Brush BtmCss { get; set; }

        [Browsable(false)]
        public string BtmCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BtmCss);
            }
            set
            {
                BtmCss = Serialize.StringToBrush(value);
            }
        }

        [Display(Name = "Font Size", Order = 11, GroupName = "Style")]
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        public int FontSize { get; set; }

        [Browsable(false)]
        public new NinjaScriptBase Owner;

        public Series<bool?> Lq_BslBreach;
        public Series<bool?> Lq_SslBreach;

        //public event Action<double> OnBslBreach;
        //public event Action<double> OnSslBreach;
        //public event Action<double> OnBullFvgCreate;
        //public event Action<double> OnBearFvgCreate;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "The Liquidity Swings indicator highlights swing areas with large trading activity for traders to find accumulation/distribution zones as well as levels to trade as support and resistance. The number of times price revisited a swing area is highlighted by a zone delimiting the areas. Additionally, the accumulated volume within swing areas is highlighted by labels on the chart. An option to filter out swing areas with volume/counts not reaching a user set threshold is also included.";
                Name = "LiquiditySwings-2";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                length = 3;
                Area = LuxLSAreaType.Wick_Extremity;
                IntraPrecision = false;
                IntrabarTf = 1;
                FilterOptions = LuxLSFilterType.Count;
                FilterValue = 0;
                ShowTop = true;
                TopCss = Brushes.Crimson;
                ShowBtm = true;
                BtmCss = Brushes.LightSeaGreen;
                FontSize = 12;
            }
            else if (State == State.Configure)
            {
                if (IntraPrecision)
                {
                    AddDataSeries(BarsPeriodType.Minute, IntrabarTf);
                }

                Lq_BslBreach = new Series<bool?>(this);
                Lq_SslBreach = new Series<bool?>(this);
            }
            else if (State == State.DataLoaded)
            {
                if (Owner == null) Owner = this;
                Pine = new PineLib(Owner, this, DrawObjects);
                font = new SimpleFont("Arial", FontSize);
                ph_bx = Pine.Box.New();
                pl_bx = Pine.Box.New();
                phClass = new LSClass(this, length, FilterValue);
                plClass = new LSClass(this, length, FilterValue);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
            {
                return;
            }

            Lq_BslBreach[0] = Lq_BslBreach[1];
            Lq_SslBreach[0] = Lq_SslBreach[1];

            if (CurrentBar < length + 2)
            {
                phClass.BeforeBar();
                plClass.BeforeBar();
                return;
            }

            phClass.OnBar();
            plClass.OnBar();
            double d = Pine.TA.PivotHigh(High, length, length);
            phClass.get_counts(!double.IsNaN(d), IntraPrecision);
            if (!double.IsNaN(d) && ShowTop)
            {
                phClass.top[0] = High[length];
                switch (Area)
                {
                    case LuxLSAreaType.Wick_Extremity:
                        phClass.btm[0] = Pine.Math.MaxValue<double>(Close[length], Open[length]);
                        break;
                    case LuxLSAreaType.Full_Range:
                        phClass.btm[0] = Low[length];
                        break;
                }

                phClass.x1[0] = CurrentBar - length;
                phClass.crossed[0] = false;
                Pine.Box.SetLeftTop(ref ph_bx, (int)phClass.x1[0], phClass.top[0]);
                Pine.Box.SetRightBottom(ref ph_bx, (int)phClass.x1[0], phClass.btm[0]);
            }
            else
            {
                phClass.crossed[0] = Close[0] > phClass.top[0] || phClass.crossed[0];
                if (phClass.crossed[0])
                {
                    Pine.Box.SetRight(ref ph_bx, (int)phClass.x1[0]);
                    //OnBslBreach?.Invoke(CurrentBar);
                    Lq_BslBreach[0] = true;
                    Lq_SslBreach[0] = false;
                }
                else
                {
                    Pine.Box.SetRight(ref ph_bx, CurrentBar + 3);
                }
            }

            if (ShowTop)
            {
                phClass.set_zone(!double.IsNaN(d), (FilterOptions == LuxLSFilterType.Count) ? phClass.count : phClass.vol, TopCss);
                phClass.set_level(!double.IsNaN(d), phClass.top[0], (FilterOptions == LuxLSFilterType.Count) ? phClass.count : phClass.vol, TopCss);
                phClass.set_label((FilterOptions == LuxLSFilterType.Count) ? phClass.count : phClass.vol, phClass.top[0], TopCss, Pine.Label.style_label_down, font);
            }

            double d2 = Pine.TA.PivotLow(Low, length, length);
            plClass.get_counts(!double.IsNaN(d2), IntraPrecision);
            if (!double.IsNaN(d2) && ShowBtm)
            {
                plClass.btm[0] = Low[length];
                switch (Area)
                {
                    case LuxLSAreaType.Wick_Extremity:
                        plClass.top[0] = Pine.Math.MinValue<double>(Close[length], Open[length]);
                        break;
                    case LuxLSAreaType.Full_Range:
                        plClass.top[0] = High[length];
                        break;
                }

                plClass.x1[0] = CurrentBar - length;
                plClass.crossed[0] = false;
                Pine.Box.SetLeftTop(ref pl_bx, (int)plClass.x1[0], plClass.top[0]);
                Pine.Box.SetRightBottom(ref pl_bx, (int)plClass.x1[0], plClass.btm[0]);
            }
            else
            {
                plClass.crossed[0] = Close[0] < plClass.btm[0] || plClass.crossed[0];
                if (plClass.crossed[0])
                {
                    Pine.Box.SetRight(ref pl_bx, (int)plClass.x1[0]);
                    //OnSslBreach?.Invoke(CurrentBar);
                    Lq_BslBreach[0] = false;
                    Lq_SslBreach[0] = true;
                }
                else
                {
                    Pine.Box.SetRight(ref pl_bx, CurrentBar + 3);
                }
            }

            if (ShowBtm)
            {
                plClass.set_zone(!double.IsNaN(d2), (FilterOptions == LuxLSFilterType.Count) ? plClass.count : plClass.vol, BtmCss);
                plClass.set_level(!double.IsNaN(d2), plClass.btm[0], (FilterOptions == LuxLSFilterType.Count) ? plClass.count : plClass.vol, BtmCss);
                plClass.set_label((FilterOptions == LuxLSFilterType.Count) ? plClass.count : plClass.vol, plClass.btm[0], BtmCss, Pine.Label.style_label_up, font);
            }
        }

        private class LSClass
        {
            private NinjaScriptBase owner;

            private int length;

            private double FilterValue;

            private Text lbl;

            private DrawingTools.Line lvl;

            private Rectangle bx;

            public Series<double> count;

            public Series<double> vol;

            public Series<double> top;

            public Series<double> btm;

            public Series<double> x1;

            public Series<bool> crossed;

            public LSClass(NinjaScriptBase owner, int length, double FilterValue)
            {
                this.owner = owner;
                lbl = Pine.Label.New();
                lvl = Pine.Line.New();
                bx = Pine.Box.New();
                this.length = length;
                this.FilterValue = FilterValue;
                count = new Series<double>(owner);
                vol = new Series<double>(owner);
                top = new Series<double>(owner);
                btm = new Series<double>(owner);
                x1 = new Series<double>(owner);
                crossed = new Series<bool>(owner);
            }

            public void OnBar()
            {
                count[0] = count[1];
                vol[0] = vol[1];
                top[0] = top[1];
                btm[0] = btm[1];
                x1[0] = x1[1];
                crossed[0] = crossed[1];
            }

            public void BeforeBar()
            {
                count[0] = 0.0;
                vol[0] = 0.0;
                top[0] = 0.0;
                btm[0] = 0.0;
                x1[0] = 0.0;
                crossed[0] = false;
            }

            public void get_counts(bool condition, bool IntraPrecision)
            {
                if (condition)
                {
                    count[0] = 0.0;
                    vol[0] = 0.0;
                    return;
                }

                if (IntraPrecision)
                {
                    int num = owner.BarsArray[1].GetBar(owner.Time[length + 2]) + 1;
                    int num2 = owner.BarsArray[1].GetBar(owner.Time[length + 1]) + 1;
                    for (int i = num; i < num2; i++)
                    {
                        vol[0] += ((owner.BarsArray[1].GetLow(i) < top[0] && owner.BarsArray[1].GetHigh(i) > btm[0]) ? owner.BarsArray[1].GetVolume(i) : 0);
                    }
                }
                else
                {
                    vol[0] += ((owner.Low[length] < top[0] && owner.High[length] > btm[0]) ? owner.Volume[length] : 0.0);
                }

                count[0] += ((owner.Low[length] < top[0] && owner.High[length] > btm[0]) ? 1 : 0);
            }

            public void set_label(Series<double> target, double y, Brush css, int lbl_style, SimpleFont font)
            {
                if (Pine.TA.CrossOver(target, FilterValue))
                {
                    lbl = Pine.Label.New((int)x1[0], y, Pine.FormatNumber(vol[0]), null, null, 0, lbl_style, css, font, TextAlignment.Center, 15);
                }

                if (target[0] > FilterValue)
                {
                    lbl.DisplayText = Pine.FormatNumber(vol[0]);
                }
            }

            public void set_level(bool condition, double value, Series<double> target, Brush css)
            {
                if (condition)
                {
                    if (target[1] < FilterValue)
                    {
                        Pine.Line.Delete(lvl);
                    }
                    else if (!crossed[1])
                    {
                        Pine.Line.SetX2(ref lvl, owner.CurrentBar - length);
                    }

                    lvl = Pine.Line.New(owner.CurrentBar - length, value, owner.CurrentBar, value);
                }

                if (!crossed[1])
                {
                    Pine.Line.SetX2(ref lvl, owner.CurrentBar + 3);
                }

                if (crossed[0] && !crossed[1])
                {
                    Pine.Line.SetX2(ref lvl, owner.CurrentBar);
                    Pine.Line.SetStyle(ref lvl, Pine.Line.style_dashed);
                }

                if (target[0] > FilterValue)
                {
                    Pine.Line.SetColor(ref lvl, css);
                }
            }

            public void set_zone(bool condition, Series<double> target, Brush css)
            {
                if (Pine.TA.CrossOver(target, FilterValue))
                {
                    bx = Pine.Box.New((int)x1[0], top[0], (int)x1[0] + (int)count[0], btm[0], null, 1, DashStyleHelper.Solid, css);
                }

                if (target[0] > FilterValue)
                {
                    Pine.Box.SetRight(ref bx, (int)x1[0] + (int)count[0]);
                }
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LuxAlgo2.LiquiditySwings2[] cacheLiquiditySwings2;
		public LuxAlgo2.LiquiditySwings2 LiquiditySwings2(int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			return LiquiditySwings2(Input, length, area, intraPrecision, intrabarTf, filterOptions, filterValue, showTop, topCss, showBtm, btmCss, fontSize);
		}

		public LuxAlgo2.LiquiditySwings2 LiquiditySwings2(ISeries<double> input, int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			if (cacheLiquiditySwings2 != null)
				for (int idx = 0; idx < cacheLiquiditySwings2.Length; idx++)
					if (cacheLiquiditySwings2[idx] != null && cacheLiquiditySwings2[idx].length == length && cacheLiquiditySwings2[idx].Area == area && cacheLiquiditySwings2[idx].IntraPrecision == intraPrecision && cacheLiquiditySwings2[idx].IntrabarTf == intrabarTf && cacheLiquiditySwings2[idx].FilterOptions == filterOptions && cacheLiquiditySwings2[idx].FilterValue == filterValue && cacheLiquiditySwings2[idx].ShowTop == showTop && cacheLiquiditySwings2[idx].TopCss == topCss && cacheLiquiditySwings2[idx].ShowBtm == showBtm && cacheLiquiditySwings2[idx].BtmCss == btmCss && cacheLiquiditySwings2[idx].FontSize == fontSize && cacheLiquiditySwings2[idx].EqualsInput(input))
						return cacheLiquiditySwings2[idx];
			return CacheIndicator<LuxAlgo2.LiquiditySwings2>(new LuxAlgo2.LiquiditySwings2(){ length = length, Area = area, IntraPrecision = intraPrecision, IntrabarTf = intrabarTf, FilterOptions = filterOptions, FilterValue = filterValue, ShowTop = showTop, TopCss = topCss, ShowBtm = showBtm, BtmCss = btmCss, FontSize = fontSize }, input, ref cacheLiquiditySwings2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LuxAlgo2.LiquiditySwings2 LiquiditySwings2(int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			return indicator.LiquiditySwings2(Input, length, area, intraPrecision, intrabarTf, filterOptions, filterValue, showTop, topCss, showBtm, btmCss, fontSize);
		}

		public Indicators.LuxAlgo2.LiquiditySwings2 LiquiditySwings2(ISeries<double> input , int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			return indicator.LiquiditySwings2(input, length, area, intraPrecision, intrabarTf, filterOptions, filterValue, showTop, topCss, showBtm, btmCss, fontSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LuxAlgo2.LiquiditySwings2 LiquiditySwings2(int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			return indicator.LiquiditySwings2(Input, length, area, intraPrecision, intrabarTf, filterOptions, filterValue, showTop, topCss, showBtm, btmCss, fontSize);
		}

		public Indicators.LuxAlgo2.LiquiditySwings2 LiquiditySwings2(ISeries<double> input , int length, LuxLSAreaType area, bool intraPrecision, int intrabarTf, LuxLSFilterType filterOptions, int filterValue, bool showTop, Brush topCss, bool showBtm, Brush btmCss, int fontSize)
		{
			return indicator.LiquiditySwings2(input, length, area, intraPrecision, intrabarTf, filterOptions, filterValue, showTop, topCss, showBtm, btmCss, fontSize);
		}
	}
}

#endregion
