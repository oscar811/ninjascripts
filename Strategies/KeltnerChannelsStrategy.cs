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
namespace NinjaTrader.NinjaScript.Strategies
{
    public class KeltnerChannelsStrategy : Strategy
    {
        private double upper, lower, ma, rangema;
        private int length = 20;
        private double mult = 2.0;
        private bool exp = true;
        private int atrlength = 10;

        private EMA ema;
        private SMA sma;
        private ATR atr;

        private Series<double> bprice;
        private Series<double> sprice;

        private Series<bool> crossBcond;
        private Series<bool> crossScond;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"KeltnerChannelsStrategy";
                Name = "KeltnerChannelsStrategy";
                Calculate = Calculate.OnBarClose;
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
            }
            else if (State == State.Configure)
            {
                bprice = new Series<double>(this);
                sprice = new Series<double>(this);

                crossBcond = new Series<bool>(this);
                crossScond = new Series<bool>(this);
            }
            else if (State == State.DataLoaded)
            {
                ema = EMA(Close, length);
                sma = SMA(Close, length);
                atr = ATR(atrlength);

                ClearOutputWindow();
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 20)
                    return;

                if (Bars.BarsSinceNewTradingDay < 1)
                    return;
                
                // BandsStyle = input.string("Average True Range", options = ["Average True Range", "True Range", "Range"], title="Bands Style")                

                // ma = esma(src, length)
                ma = exp ? ema[0] : sma[0];

                // rangema = BandsStyle == "True Range" ? ta.tr(true) : BandsStyle == "Average True Range" ? ta.atr(atrlength) : ta.rma(high - low, length)
                rangema = atr[0];

                upper = ma + rangema * mult;
                lower = ma - rangema * mult;
                // crossUpper = ta.crossover(src, upper)
                bool crossUpper = CrossAbove(Close, upper, 1);

                // crossLower = ta.crossunder(src, lower)
                bool crossLower = CrossBelow(Close, lower, 1);

                // bprice = 0.0
                // bprice := crossUpper ? high+syminfo.mintick : nz(bprice[1])
                bprice[0] = crossUpper ? High[0] + TickSize : bprice[1];


                // sprice = 0.0
                // sprice := crossLower ? low -syminfo.mintick : nz(sprice[1])
                sprice[0] = crossLower ? Low[0] - TickSize : sprice[1];

                // crossBcond = false
                // crossBcond[0] = crossUpper ? true : na(crossBcond[1]) ? false : crossBcond[1];
                // crossBcond[0] = crossUpper ? true : na(crossBcond[1]) ? false : crossBcond[1];

                // crossScond = false
                // crossScond := crossLower ? true : na(crossScond[1]) ? false : crossScond[1]

                bool cancelBcond = crossUpper && (Close[0] < ma || High[0] >= bprice[0]);
                bool cancelScond = crossLower && (Close[0] > ma || Low[0] <= sprice[0]);
                Order entryOrderLong = null;
                Order entryOrderShort = null;

                if (cancelBcond && entryOrderLong != null && entryOrderLong.OrderState == OrderState.Working)
                    CancelOrder(entryOrderLong);

                if (crossUpper)
                    entryOrderLong = EnterLongStopMarket(bprice[0], "KltChLE");

                if (cancelScond && entryOrderShort != null && entryOrderShort.OrderState == OrderState.Working)
                    CancelOrder(entryOrderShort);
                
                if (crossLower)
                    entryOrderShort = EnterShortStopMarket(sprice[0], "KltChSE");
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private bool OrderExists(string orderName)
        {
            foreach (Order order in Orders)
                if (order.Name == orderName && order.OrderState == OrderState.Working)
                    return true;
            return false;
        }

        protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice,
            int quantity, int filled, double averageFillPrice,
            Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
        {

        }
    }
}
