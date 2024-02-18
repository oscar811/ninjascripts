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
using NinjaTrader.NinjaScript.Indicators.LuxAlgo2;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static NinjaTrader.CQG.ProtoBuf.Quote.Types;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class CameronStrategy : Strategy
    {
        private LiquiditySwings2 lqSwings;
        //private BuysideSellsideLiquidity2 lq;
        //private BuysideSellsideLiquidity2 lq2;

        //private LiquidityVoidsFVG2 fvg; 

        private int StopLoss = 50;
        private int TakeProfit = 100;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"CameronStrategy";
                Name = "CameronStrategy";
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
                ClearOutputWindow();

                AddDataSeries(BarsPeriodType.Minute, 5);
                //AddDataSeries("ES", BarsPeriodType.Minute, BarsPeriod.Value);
                //AddDataSeries("ES", BarsPeriodType.Minute, HtfTimeFrame);

                SetStopLoss(CalculationMode.Ticks, StopLoss);
                SetProfitTarget(CalculationMode.Ticks, TakeProfit);

                Lq_BslBreach = new Series<bool?>(this);
                Lq_SslBreach = new Series<bool?>(this);
                Lq_BullFvg = new Series<bool?>(this);
                Lq_BearFvg = new Series<bool?>(this);
            }
            else if (State == State.DataLoaded)
            {
                //lqSwings = LiquiditySwings(Closes[0], )
                //lq = BuysideSellsideLiquidity2(Closes[0], liqLen: 30, liqMar: 6.9, liqBuy: false, marBuy: 2.3, cLIQ_B: Brushes.Green,
                //    liqSel: false, marSel: 2.3, cLIQ_S: Brushes.Red, lqVoid: true, cLQV_B: Brushes.Green, cLQV_S: Brushes.Red, mode: LuxBSLMode.Historical, visLiq: 10);
                //AddChartIndicator(lq);
                //lq.OnBslBreach += Lq_OnBslBreached;
                //lq.OnSslBreach += Lq_OnSslBreached;
                //lq.OnBullFvgCreate += Lq_OnBullFvgCreate;
                //lq.OnBearFvgCreate += Lq_OnBearFvgCreate;
                //fvg = LiquidityVoidsFVG2(Closes[2], mode: LUXLVFVGMode.Historical, back: 360, lqTH: 0.5, lqBC: Brushes.Teal, lqSC: Brushes.Crimson, lqVF: true, lqFC: Brushes.Gray);
                //AddChartIndicator(fvg);
            }
        }

        private Series<bool?> Lq_BslBreach;
        private Series<bool?> Lq_SslBreach;
        private Series<bool?> Lq_BullFvg;
        private Series<bool?> Lq_BearFvg;

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                //Print("Time[0]: " + Time[0].ToString());
                //Print("CurrentBar: " + CurrentBar);

                Lq_BslBreach[0] = null;
                Lq_SslBreach[0] = null;
                Lq_BullFvg[0] = null;
                Lq_BearFvg[0] = null;

                // so if lq is swept? how to know if it is swept?
                // breach + opposite candle?, need another series to track that
                // fvg in ltf, need series to track that
                // if both conditions are met, take position
                // when to exit? opposite lq breached? 1:1? start with 1:1

            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private void Lq_OnBslBreached(double newValue)
        {
            Lq_BslBreach[0] = true;
            // Handle the value change event, e.g., by adjusting strategy behavior
            Print("BSL breached: " + newValue);
        }

        private void Lq_OnSslBreached(double newValue)
        {
            Lq_SslBreach[0] = true;
            // Handle the value change event, e.g., by adjusting strategy behavior
            Print("SSL breached: " + newValue);
        }

        private void Lq_OnBullFvgCreate(double barNo)
        {
            Lq_BullFvg[0] = true;
            //Print("Bull FVG created: " + barNo);
        }

        private void Lq_OnBearFvgCreate(double barNo)
        {
            Lq_BearFvg[0] = true;
            //Print("Bear FVG created: " + barNo);
        }
    }
}
