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
using NinjaTrader.NinjaScript.SuperDomColumns;
using System.Windows.Shapes;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using NinjaTrader.NinjaScript.Strategies;
using SharpDX;
#endregion

//This namespace holds Indicators in this folder && is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class IFvg2 : Indicator
    {
        private bool showFvg = false;
        private bool showIFvg = true;
        private bool extendRight = false;
        private int displacement = 3;
        private int displayLimit = 10;
        private string session = "0000-0000";
        private int rejectionStrength = 1;
        private bool showFvgRejection = false;
        private bool showIFvgRejection = false;
        private bool showBothRejections = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"IFvg2";
                Name = "IFvg2";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
            }
        }

        private int buffer = 100; //How many FVGs to keep in memory.
        private int disp_num = 5; //How many IFvgs to display
        private double atr_multi = 0.25; //ATR Multiplier
        private bool wt = false; //Signal Preference

        private List<double> tf_o = new List<double>();
        private List<double> tf_h = new List<double>();
        private List<double> tf_l = new List<double>();
        private List<double> tf_c = new List<double>();
        private List<int> tf_time = new List<int>();
        private List<bool> tf_t = new List<bool>();

        // For boxes and lines, we'll manage their reference names as strings, since NinjaTrader handles drawings differently
        private List<string> reg_fvg = new List<string>(); // References to Regular FVG drawings (you'll draw these as needed)
        private List<string> reg_fvg_ce = new List<string>(); // References to center line drawings for Regular FVG
        private List<string> reg_fvg_side = new List<string>(); // Store sides as "bull" or "bear"

        private List<string> inv_fvg = new List<string>(); // References to Inverse FVG drawings
        private List<string> inv_fvg_ce = new List<string>(); // References to center line drawings for Inverse FVG
        private List<string> inv_fvg_side = new List<string>(); // Store sides as "inv bear" or "inv bull"

        private bool tf_new = false;

        public T Nz<T>(T? value, T defaultValue) where T : struct
        {
            return value ?? defaultValue;
        }
        //private SolidColorBrush SolidColor(Color color)
        //{
        //    return new SolidColorBrush(color);
        //}

        private double AvgOver(ISeries<double> highs, ISeries<double> lows, int length)
        {
            double sum = 0.0;
            int count = 0;

            for (int i = 0; i < length && i < CurrentBar; i++)
            {
                sum += (highs[i] - lows[i]);
                count++;
            }

            return count > 0 ? sum / count : 0.0; // Prevent division by zero
        }


        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 3)
                    return;

                bool reg_rj_bear = false;
                bool reg_rj_bull = false;

                bool inv_rj_bear = false;
                bool inv_rj_bull = false;


//                if timeframe.change(tf) and timeframe.in_seconds(tf) >= timeframe.in_seconds(timeframe.period)
//    tf_o.unshift(o)
//    tf_h.unshift(h)
//    tf_l.unshift(l)
//    tf_c.unshift(c)
//    tf_t.unshift(t[1])
//    tf_time.unshift(ti)

//    tf_new:= true

//    if tf_o.size() > 300
//        tf_o.pop()
//        tf_h.pop()
//        tf_l.pop()
//        tf_c.pop()
//        tf_t.pop()
//        tf_time.pop()

//if not extend_right
//    if reg_fvg.size() > 0
//        for i = 0 to reg_fvg.size() - 1
//            reg_fvg.get(i).set_right(time)
//            reg_fvg_ce.get(i).set_x2(time)


//    if inv_fvg.size() > 0
//        for i = 0 to inv_fvg.size() - 1
//            inv_fvg.get(i).set_right(time)
//            inv_fvg_ce.get(i).set_x2(time)

//if tf_o.size() > 20 and tf_new
//    bull = tf_c.get(1) > tf_o.get(1)
//    fvg = bull ? (tf_h.get(2) < tf_l.get(0) and tf_l.get(1) <= tf_h.get(2) and tf_h.get(1) >= tf_l.get(0)) : (tf_l.get(2) > tf_h.get(0) and tf_l.get(1) <= tf_h.get(0) and tf_l.get(2) <= tf_h.get(1))
//    fvg_len = bull ? tf_l.get(0) - tf_h.get(2) : tf_l.get(2) - tf_h.get(0)
//    atr_check = fvg_len > avg_over(tf_h, tf_l, 20) * disp_x / 10

//    if tf_t.get(2) and fvg and atr_check
//        top = bull ? tf_l.get(0) : tf_l.get(2)
//        bottom = bull ? tf_h.get(2) : tf_h.get(0)
//        reg_fvg.unshift(box.new(tf_time.get(1), top, time, bottom, xloc = xloc.bar_time, extend = extend_right ? extend.right : extend.none, bgcolor = show_rfvg ? rfvg_color : na, border_color = na))
//        reg_fvg_ce.unshift(line.new(tf_time.get(1), math.avg(top, bottom), time, math.avg(top, bottom), xloc = xloc.bar_time, extend = extend_right ? extend.right : extend.none, color = show_rfvg ? solid_color(rfvg_color) : na, style = line.style_dashed))
//        reg_fvg_side.unshift(bull ? 'bull' : 'bear')


//        tf_new:= false

//    if reg_fvg.size() > 0
//        for i = reg_fvg.size() - 1 to 0
//            remove_bull = reg_fvg_side.get(i) == 'bull' and tf_c.get(0) < reg_fvg.get(i).get_bottom()
//            remove_bear = reg_fvg_side.get(i) == 'bear' and tf_c.get(0) > reg_fvg.get(i).get_top()

//            if remove_bull or remove_bear
//                inv_fvg.unshift(box.copy(reg_fvg.get(i)))
//                inv_fvg_ce.unshift(line.copy(reg_fvg_ce.get(i)))
//                inv_fvg.get(0).set_bgcolor(show_ifvg ? ifvg_color : na)
//                inv_fvg_ce.get(0).set_color(show_ifvg ? solid_color(ifvg_color) : na)

//                box.delete(reg_fvg.get(i))
//                line.delete(reg_fvg_ce.get(i))
//                reg_fvg.remove(i)
//                reg_fvg_ce.remove(i)
//                reg_fvg_side.remove(i)


//                if remove_bear
//                    inv_fvg_side.unshift('inv bear')
//                else if remove_bull
//                    inv_fvg_side.unshift('inv bull')

//        if reg_fvg.size() > disp_limit
//            box.delete(reg_fvg.pop())
//            line.delete(reg_fvg_ce.pop())
//            reg_fvg_side.pop()

//    if inv_fvg.size() > 0
//        for i = inv_fvg.size() - 1 to 0
//            remove_inv_bear = inv_fvg_side.get(i) == 'inv bear' and tf_c.get(0) < inv_fvg.get(i).get_bottom()
//            remove_inv_bull = inv_fvg_side.get(i) == 'inv bull' and tf_c.get(0) > inv_fvg.get(i).get_top()

//            if remove_inv_bear or remove_inv_bull
//                box.delete(inv_fvg.get(i))
//                line.delete(inv_fvg_ce.get(i))
//                inv_fvg.remove(i)
//                inv_fvg_ce.remove(i)
//                inv_fvg_side.remove(i)

//        if inv_fvg.size() > disp_limit
//            box.delete(inv_fvg.pop())
//            line.delete(inv_fvg_ce.pop())
//            inv_fvg_side.pop()

//    if math.min(reg_fvg.size(), inv_fvg.size()) > 0
//        for i = 0 to reg_fvg.size() - 1
//            _rrj_bear = ta.pivothigh(high, ps, ps) and high[ps] >= reg_fvg.get(i).get_bottom() and high[ps] <= reg_fvg.get(i).get_top()
//            _rrj_bull = ta.pivotlow(low, ps, ps) and low[ps] >= reg_fvg.get(i).get_bottom() and low[ps] <= reg_fvg.get(i).get_top()

//            if _rrj_bear
//                reg_rj_bear:= true
//            if _rrj_bull
//                reg_rj_bull:= true

//        for i = 0 to inv_fvg.size() - 1
//            _irj_bear = ta.pivothigh(high, ps, ps) and high[ps] >= inv_fvg.get(i).get_bottom() and high[ps] <= inv_fvg.get(i).get_top()
//            _irj_bull = ta.pivotlow(low, ps, ps) and low[ps] >= inv_fvg.get(i).get_bottom() and low[ps] <= inv_fvg.get(i).get_top()

//            if _irj_bear
//                inv_rj_bear:= true
//            if _irj_bull
//                inv_rj_bull:= true;
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private class Fvg
        {
            public DateTime? left = null;
            public double? top = null;
            public DateTime? right = null;
            public double? bot = null;
            public double? mid = null;
            public int? dir = null;
            public int? state = null;
            public List<Lab> labs = null;
            public DateTime? x_val = null;

            //public Fvg(int left, double low, int right, high[2], math.avg(low, high[2]) {
            //}

            public Fvg(DateTime left, double top, DateTime right, double bot, double mid, int dir, int state, List<Lab> labs, DateTime? x_val)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bot = bot;
                this.mid = mid;
                this.dir = dir;
                this.state = state;
                this.labs = labs;
                this.x_val = x_val;
            }
        }

        private class Lab
        {
            public DateTime? x;
            public double y;
            public int dir;

            public Lab(DateTime x, double y, int dir)
            {
                this.x = x;
                this.y = y;
                this.dir = dir;
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.IFvg2[] cacheIFvg2;
		public RajIndicators.IFvg2 IFvg2()
		{
			return IFvg2(Input);
		}

		public RajIndicators.IFvg2 IFvg2(ISeries<double> input)
		{
			if (cacheIFvg2 != null)
				for (int idx = 0; idx < cacheIFvg2.Length; idx++)
					if (cacheIFvg2[idx] != null &&  cacheIFvg2[idx].EqualsInput(input))
						return cacheIFvg2[idx];
			return CacheIndicator<RajIndicators.IFvg2>(new RajIndicators.IFvg2(), input, ref cacheIFvg2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.IFvg2 IFvg2()
		{
			return indicator.IFvg2(Input);
		}

		public Indicators.RajIndicators.IFvg2 IFvg2(ISeries<double> input )
		{
			return indicator.IFvg2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.IFvg2 IFvg2()
		{
			return indicator.IFvg2(Input);
		}

		public Indicators.RajIndicators.IFvg2 IFvg2(ISeries<double> input )
		{
			return indicator.IFvg2(input);
		}
	}
}

#endregion
