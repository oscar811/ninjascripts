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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class CaptainBacktest2 : Strategy
    {
        private Series<double> range_high;
        private Series<double> range_low;

        private Series<int> bias;

        private Series<bool> opp_close;
        private Series<bool> took_hl;
        private Series<bool> is_long;
        private Series<bool> is_short;

        private Series<bool> t_prev;
        private Series<bool> t_take;
        private Series<bool> t_trade;

        private int last_trades = 0;
        private int prior_num_trades = 0;
        private int prior_session_trades = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"CaptainBacktest2";
                Name = "CaptainBacktest2";
                Calculate = Calculate.OnPriceChange;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                prev_start = 060000;
                prev_end = 100000;
                take_start = 100000;
                take_end = 111500;
                trade_start = 100000;
                trade_end = 16000000;

                retrace_1 = true; 
                retrace_2 = true;
                stop_orders = false;
                fixed_rr = true;
                risk = 25;
                reward = 50;
            }
            else if (State == State.Configure)
            {
                range_high = new Series<double>(this);
                range_low = new Series<double>(this);

                bias = new Series<int>(this);

                opp_close = new Series<bool>(this);
                took_hl = new Series<bool>(this);
                is_long = new Series<bool>(this);
                is_short = new Series<bool>(this);

                t_prev = new Series<bool>(this);
                t_take = new Series<bool>(this);
                t_trade = new Series<bool>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 3)
                return;

            t_prev[0] = check_time(prev_start, prev_end);
            t_take[0] = check_time(take_start, take_end);
            t_trade[0] = check_time(trade_start, trade_end);

            bias[0] = bias[1];
            opp_close[0] = opp_close[1];
            took_hl[0] = took_hl[1];
            is_short[0] = is_short[1];
            is_long[0] = is_long[1];

            bool can_trade = took_trade() == false;

            if (fixed_rr)
            {
                SetProfitTarget("", CalculationMode.Ticks, reward / TickSize);
                SetStopLoss("", CalculationMode.Ticks, risk / TickSize, false);
            }            

            prev_range();
            reset();
            take_range();

            if (can_trade)
            {
                trade_range();
            }
            else if (!t_trade[0] && t_trade[1])
            {
                ExitLong();
                ExitShort();
            }
        }

        private void trade_range()
        {
            opp_close[0] = opp_close[1];
            took_hl[0] = took_hl[1];

            if (t_trade[0])
            {
                if(!retrace_1)
                {
                    opp_close[0] = true;
                }
                else
                {
                    if (bias[0] == 1 && Close[0] < Open[0])
                    {
                        opp_close[0] = true;
                    }
                    if (bias[0] == -1 && Close[0] > Open[0])
                    {
                        opp_close[0] = true;
                    }
                }

                if (!retrace_2)
                {
                    took_hl[0] = true;
                }
                else
                {
                    if (bias[0] == 1 && Low[0] < Low[1])
                    {
                        took_hl[0] = true;
                    }
                    if (bias[0] == -1 && High[0] > High[1])
                    {
                        took_hl[0] = true;
                    }
                }
            }

            if (CurrentBar > 3)
            {
                if (bias[1] == 1 && Close[0] > High[1] && opp_close[0] && took_hl[0] && !is_long[1])
                {
                    is_long[0] = true;
                    if (stop_orders)
                    {
                        EnterLongStopMarket(DefaultQuantity, High[0], Convert.ToString(CurrentBar) + " Long");
                    }
                    else
                    {
                        EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Long");
                    }
                }
                if (bias[1] == 11 && Close[0] < Low[1] && opp_close[0] && took_hl[0] && !is_short[1])
                {
                    is_short[0] = true;
                    if (stop_orders)
                    {
                        EnterShortStopMarket(DefaultQuantity, Low[0], Convert.ToString(CurrentBar) + " Short");
                    }
                    else
                    {
                        EnterShort(DefaultQuantity, Convert.ToString(CurrentBar) + " Short");
                    }
                }
            }
        }

        private void take_range()
        {
            bool draw = false;
            if (t_take[0] && CurrentBar > 3)
            {
                if (High[0] > range_high[0] && bias[0] == 0)
                {
                    bias[0] = 1;
                    draw = true;
                    Draw.ArrowUp(this, Convert.ToString(CurrentBar) + " ArrowUp", true, 0, High[0], Brushes.Black);
                }
                if (Low[0] < range_low[0] && bias[0] == 0)
                {
                    bias[0] = 11;
                    draw = true;
                    Draw.ArrowDown(this, Convert.ToString(CurrentBar) + " ArrowDown", true, 0, Low[0], Brushes.Black);
                }
            }
            else if (!t_take[0] && t_take[1] && bias[0] == 0)
            {
                Draw.Text(this, Convert.ToString(CurrentBar) + " NoTrade", "No Trades", 0, High[0]);
                draw = true;
            }

            if (draw)
            {
                Draw.Line(this, Convert.ToString(CurrentBar) + " Range High", 20, range_high[0], 0, range_high[0], Brushes.Red);
                Draw.Line(this, Convert.ToString(CurrentBar) + " Range Low", 20, range_low[0], 0, range_low[0], Brushes.Red);
            }
        }

        private void reset()
        {
            if (CurrentBar > 3)
            {
                if (!t_trade[0] && t_trade[1])
                {
                    bias[0] = 0;
                    is_long[0] = false;
                    is_short[0] = false;
                    opp_close[0] = false;
                    took_hl[0] = false;
                }
            }
        }

        private void prev_range()
        {
            range_high[0] = range_high[1];
            range_low[0] = range_low[1];

            if (t_prev[0] && CurrentBar > 3)
            {
                if (!t_prev[1])
                {
                    range_high[0] = High[0];
                    range_low[0] = Low[0];
                }
                else
                {
                    range_high[0] = Math.Max(range_high[1], High[0]);
                    range_low[0] = Math.Min(range_low[1], Low[0]);
                }
            }
        }

        private bool took_trade()
        {
            bool trade = false;
            if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
            {
                prior_session_trades = SystemPerformance.AllTrades.Count;
            }

            if (SystemPerformance.AllTrades.Count - prior_session_trades > 0)
            {
                trade = true;
            }

            return trade;
        }

        private bool check_time(int T1, int T2)
        {
            bool result = false;
            int T = ToTime(Time[0]);
            if (T1 > T2)
            {
                result = T >= T1 || T <= T2;
            }
            else
            {
                result = T >= T1 && T <= T2;
            }
            return result;
        }


        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Price Range Start", Order = 1, GroupName = "Time")]
        public int prev_start { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Price Range End", Order = 2, GroupName = "Time")]
        public int prev_end { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bias Window Start", Order = 3, GroupName = "Time")]
        public int take_start { set; get; }

        [NinjaScriptProperty]
        [Display(Name = "Bias Window End", Order = 4, GroupName = "Time")]
        public int take_end { set; get; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Window Start", Order = 5, GroupName = "Time")]
        public int trade_start { set; get; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Window End", Order = 6, GroupName = "Time")]
        public int trade_end { set; get; }


        //
        [NinjaScriptProperty]
        [Display(Name = "Retrace - Opposite close candle", Order = 3, GroupName = "Strategy")]
        public bool retrace_1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Retrace - Took previous high/low", Order = 3, GroupName = "Strategy")]
        public bool retrace_2 { get; set; }

        public bool stop_orders { get; set; }

        private bool fixed_rr = true;

        //
        [NinjaScriptProperty]
        [Display(Name = "Risk", Order = 3, GroupName = "Risk")]
        public double risk {  get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Reward", Order = 3, GroupName = "Risk")]
        public double reward { get; set; }
        #endregion
    }
}
