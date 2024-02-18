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
        private BuysideSellsideLiquidity2 lq;
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
                AddDataSeries(BarsPeriodType.Minute, 5);
                AddDataSeries(BarsPeriodType.Second, 30);
                //AddDataSeries("ES", BarsPeriodType.Minute, BarsPeriod.Value);
                //AddDataSeries("ES", BarsPeriodType.Minute, HtfTimeFrame);

                SetStopLoss(CalculationMode.Ticks, StopLoss);
                SetProfitTarget(CalculationMode.Ticks, TakeProfit);

                Lq_Breach = new Series<bool?>(this);

                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                lq = BuysideSellsideLiquidity2(Closes[0], liqLen: 7, liqMar: 6.9, liqBuy: true, marBuy: 2.3, cLIQ_B: Brushes.Green,
                    liqSel: true, marSel: 2.3, cLIQ_S: Brushes.Red, lqVoid: false, cLQV_B: Brushes.Green, cLQV_S: Brushes.Red, mode: LuxBSLMode.Present, visLiq: 3);
                AddChartIndicator(lq);
                lq.OnLqBreach += Lq_OnBreached;

                //fvg = LiquidityVoidsFVG2(Closes[2], mode: LUXLVFVGMode.Historical, back: 360, lqTH: 0.5, lqBC: Brushes.Teal, lqSC: Brushes.Crimson, lqVF: true, lqFC: Brushes.Gray);
                //AddChartIndicator(fvg);
            }
        }

        private Series<bool?> Lq_Breach; // = new Series<bool?>(this);

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                Print("Time[0]: " + Time[0].ToString());
                Print("CurrentBar: " + CurrentBar);
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private void Lq_OnBreached(double newValue)
        {
            // Handle the value change event, e.g., by adjusting strategy behavior
            Print("Indicator value changed to: " + newValue);
        }

    }
}
