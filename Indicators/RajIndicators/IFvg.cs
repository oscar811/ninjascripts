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
#endregion

//This namespace holds Indicators in this folder && is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class IFvg : Indicator
    {
        private ATR atrIndicator;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"IFvg";
                Name = "IFvg";
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
                atrIndicator = ATR(200);
            }
        }

        private int buffer = 100; //How many FVGs to keep in memory.
        private int disp_num = 5; //How many IFvgs to display
        private double atr_multi = 0.25; //ATR Multiplier
        private bool wt = false; //Signal Preference

        private void label_maker(DateTime _x, double _y, int _dir)
        {
            switch (_dir)
            {
                case 1:
                    //label.new(_x, _y, "\n?", style = label.style_text_outline, color = invis, textcolor = color.new(green, 0), size = size.small, xloc = xloc.bar_time);
                    Draw.Text(this, "label" + CurrentBar, true, "\nup", _x, _y, 0, Brushes.Green, null, TextAlignment.Left, null, null, 0);
                    break;
                case -1:
                    Draw.Text(this, "label" + CurrentBar, true, "down\n", _x, _y, 0, Brushes.Red, null, TextAlignment.Left, null, null, 0);
                    //label.new(_x, _y, "?\n", style = label.style_text_outline, color = invis, textcolor = color.new(red, 0), size = size.small, xloc = xloc.bar_time);
                    break;
            }
        }

        private void fvg_manage(List<Fvg> _ary, List<Fvg> _inv_ary, DateTime dateTime, double c_top, double c_bot) //First step filtering of FVG data, Not all FVGs will be displayed, only inversions.
        {
            if (_ary.Count >= buffer)
                _ary.RemoveAt(0);

            if (_ary.Count > 0)
            {
                for (int i = _ary.Count - 1; i >= 0; i--)
                {
                    Fvg value = _ary[i];

                    int? _dir = value.dir;
                    Fvg convertingFvg = _ary[i];
                    if (_dir == 1 && (c_bot < value.bot))
                    {
                        value.x_val = dateTime;
                        _ary.RemoveAt(i);
                        _inv_ary.Add(convertingFvg);
                    }
                    if (_dir == -1 && (c_top > value.top))
                    {
                        value.x_val = dateTime;
                        _ary.RemoveAt(i);
                        _inv_ary.Add(convertingFvg);
                    }
                }
            }
        }


        private bool inv_manage(List<Fvg> _ary, DateTime time, ISeries<double> high, ISeries<double> low, ISeries<double> close, double c_top, double c_bot) //All inversions will be displayed.
        {
            bool fire = false;
            if (_ary.Count >= buffer)
                _ary.RemoveAt(0);
            if (_ary.Count > 0)
                for (int i = _ary.Count - 1; i > 0; i--)
                {
                    Fvg value = _ary[i];
                    double bx_top = value.top.Value;
                    double bx_bot = value.bot.Value;
                    double _dir = value.dir.Value;
                    double st = value.state.Value;

                    if (st == 0 && _dir == 1)
                    {
                        value.state = 1;
                        value.dir = -1;
                    }
                    if (_dir == -1 && st == 0)
                    {
                        value.state = 1;
                        value.dir = 1;
                    }
                    if (st >= 1)
                        value.right = Time[0];

                    if (_dir == -1 && st == 1 && Close[0] < bx_bot && (wt ? high[0] : close[1]) >= bx_bot && (wt ? high[0] : close[1]) < bx_top)
                    {
                        //value.labs.push(lab.new(time, bx_top, -1))
                        value.labs.Add(new Lab(time, bx_top, -1));
                        fire = true;
                    }
                    if (_dir == 1 && st == 1 && close[0] > bx_top && (wt ? low[0] : close[1]) <= bx_top && (wt ? low[0] : close[1]) > bx_bot)
                    {
                        //value.labs.push(lab.new(time, bx_bot, 1));
                        value.labs.Add(new Lab(time, bx_top, 1));
                        fire = true;
                    }
                    if (st >= 1 && ((_dir == -1 && c_top > bx_top) || (_dir == 1 && c_bot < bx_bot)))
                        _ary.RemoveAt(i);
                }

            return fire;
        }

        private void send_it(List<Fvg> _ary, DateTime currentBarDateTime)  // Draws Everything on the Chart
        {
            int last_index = _ary.Count - 1;
            for (int i = 0; i < _ary.Count; i++)
            {
                double? bx_top = _ary[i].top;
                double? bx_bot = _ary[i].bot;
                DateTime bx_left = _ary[i].left.Value;
                DateTime? xval = _ary[i].x_val;
                double mid = _ary[i].mid.Value;
                SolidColorBrush col = _ary[i].dir == -1 ? Brushes.Green : Brushes.Red;
                SolidColorBrush o_col = _ary[i].dir == -1 ? Brushes.Red : Brushes.Green;

                if (i > last_index - disp_num)
                {
                    //box.new(bx_left, bx_top, xval, bx_bot, bgcolor = col, border_color = invis, xloc = xloc.bar_time);
                    //box.new(xval, bx_top, time, bx_bot, bgcolor = o_col, border_color = invis, xloc = xloc.bar_time);
                    
                    Draw.Line(this, "myDashedLine" + CurrentBar, true, ToTime(bx_left), mid, ToTime(currentBarDateTime), mid, Brushes.Gray, DashStyleHelper.Dash, 2);
                    ////line.new(bx_left, mid, time, mid, color = gray, style = line.style_dashed, xloc = xloc.bar_time);

                    //box.new(bar_index, bx_top, bar_index + 50, bx_bot, bgcolor = o_col, border_color = invis);

                    //Draw.Line(this, "myDashedLine" + CurrentBar, true, ToTime(bx_top), mid, ToTime(currentBarDateTime), mid, Brushes.Gray, DashStyleHelper.Dash, 2);
                    ////line.new(bar_index, mid, bar_index + 50, mid, color = gray, style = line.style_dashed);
                }

                foreach (var stuff in _ary[i].labs)
                    label_maker(stuff.x.Value, stuff.y, stuff.dir);
            }
        }

        //public double Nz(double? value)
        //{
        //    return value ?? 0.0;
        //}

        public T Nz<T>(T? value, T defaultValue) where T : struct
        {
            return value ?? defaultValue;
        }

        private double cumulativeSum = 0;

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 3)
                    return;

                double c_top = Math.Max(Open[0], Close[0]);
                double c_bot = Math.Min(Open[0], Close[0]);

                //---------------------------------------------------------------------------------------------------------------------}
                //Delete drawings
                //---------------------------------------------------------------------------------------------------------------------{
                //for boxes in box.all
                //    box.delete(boxes)

                //for lines in line.all
                //    line.delete(lines)

                //for labels in label.all
                //    label.delete(labels)

                //---------------------------------------------------------------------------------------------------------------------}
                //Data Arrays
                //---------------------------------------------------------------------------------------------------------------------{
                var bull_fvg_ary = new List<Fvg>(); // array.new< fvg > (na) // FVG Data, Not all will be Drawn
                var bear_fvg_ary = new List<Fvg>();  // array.new< fvg > (na)

                var bull_inv_ary = new List<Fvg>(); // array.new< fvg > (na) // Inversion Data, All will be Drawn
                var bear_inv_ary = new List<Fvg>(); // array.new< fvg > (na)

                cumulativeSum += High[0] - Low[0];
                double averageBar = cumulativeSum / (CurrentBar + 1);

                //---------------------------------------------------------------------------------------------------------------------}
                //FVG Detection
                //---------------------------------------------------------------------------------------------------------------------{
                double atr = Nz(atrIndicator[0] * atr_multi, averageBar);

                bool fvg_up = (Low[0] > High[2]) && (Close[1] > High[2]);
                bool fvg_down = (High[0] < Low[2]) && (Close[1] < Low[2]);

                if (fvg_up && Math.Abs(Low[0] - High[2]) > atr)
                    //array.push(bull_fvg_ary, fvg.new(time[1], low, time, high[2], math.avg(low, high[2]), 1, 0, array.new< lab > (na), na))
                    bull_fvg_ary.Add(new Fvg(Time[1], Low[0], Time[0], High[2], (Low[0] + High[2]) / 2, 1, 0, new List<Lab>(), null));

                if (fvg_down && Math.Abs(Low[2] - High[0]) > atr)
                    //array.push(bear_fvg_ary, fvg.new(time[1], low[2], time, high, math.avg(high, low[2]), -1, 0, array.new< lab > (na), na));
                    bull_fvg_ary.Add(new Fvg(Time[1], Low[2], Time[0], High[0], (High[0] + Low[2]) / 2, -1, 0, new List<Lab>(), null));

                Print("bull_fvg_ary:" + bull_fvg_ary.Count);

                //---------------------------------------------------------------------------------------------------------------------}
                //Running Functions
                //---------------------------------------------------------------------------------------------------------------------{
                // FVG_Data -> Inversion_Data -> Chart
                fvg_manage(bull_fvg_ary, bull_inv_ary, Time[0], c_top, c_bot);
                fvg_manage(bear_fvg_ary, bear_inv_ary, Time[0], c_top, c_bot);

                bool bear_signal = inv_manage(bull_inv_ary, Time[0], High, Low, Close, c_top, c_bot);
                bool bull_signal = inv_manage(bear_inv_ary, Time[0], High, Low, Close, c_top, c_bot);

                if (CurrentBar == Bars.Count - 1
                    || (State == State.Realtime && Bars.IsLastBarOfSession))
                {
                    send_it(bull_inv_ary, Time[0]);
                    send_it(bear_inv_ary, Time[0]);
                }

                //Alert Options
                //alertcondition(bull_signal, "Bullish Signal")
                //alertcondition(bear_signal, "Bearish Signal")
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
		private RajIndicators.IFvg[] cacheIFvg;
		public RajIndicators.IFvg IFvg()
		{
			return IFvg(Input);
		}

		public RajIndicators.IFvg IFvg(ISeries<double> input)
		{
			if (cacheIFvg != null)
				for (int idx = 0; idx < cacheIFvg.Length; idx++)
					if (cacheIFvg[idx] != null &&  cacheIFvg[idx].EqualsInput(input))
						return cacheIFvg[idx];
			return CacheIndicator<RajIndicators.IFvg>(new RajIndicators.IFvg(), input, ref cacheIFvg);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.IFvg IFvg()
		{
			return indicator.IFvg(Input);
		}

		public Indicators.RajIndicators.IFvg IFvg(ISeries<double> input )
		{
			return indicator.IFvg(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.IFvg IFvg()
		{
			return indicator.IFvg(Input);
		}

		public Indicators.RajIndicators.IFvg IFvg(ISeries<double> input )
		{
			return indicator.IFvg(input);
		}
	}
}

#endregion
