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
using System.IO;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class CameronStrategy : Strategy
    {
        //private LiquiditySwings2 lqSwings;
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
                ClearOutputWindow();

                AddDataSeries(BarsPeriodType.Minute, 5);

                SetStopLoss(CalculationMode.Ticks, StopLoss);
                SetProfitTarget(CalculationMode.Ticks, TakeProfit);

                //Lq_BslBreach = new Series<bool?>(this);
                //Lq_SslBreach = new Series<bool?>(this);
                //Lq_BullFvg = new Series<bool?>(this);
                //Lq_BearFvg = new Series<bool?>(this);

                longBias = new Series<bool?>(this);
                opp_close = new Series<bool?>(this);
            }
            else if (State == State.DataLoaded)
            {
                //lqSwings = LiquiditySwings2(Closes[0], length: 3, LuxLSAreaType.Wick_Extremity, intraPrecision: false, intrabarTf: 1,
                //    filterOptions: LuxLSFilterType.Count, filterValue: 0, showTop: true, topCss: Brushes.Crimson, showBtm: true, btmCss: Brushes.LightSeaGreen, 12);
                //AddChartIndicator(lqSwings);

                lq = BuysideSellsideLiquidity2(Closes[1], liqLen: 3, liqMar: 27, liqBuy: false, marBuy: 2.3, cLIQ_B: Brushes.Green,
                    liqSel: false, marSel: 2.3, cLIQ_S: Brushes.Red, lqVoid: true, cLQV_B: Brushes.Green, cLQV_S: Brushes.Red, mode: LuxBSLMode.Historical, visLiq: 20);
                AddChartIndicator(lq);

                //lqSwings.OnBslBreach += Lq_OnBslBreached;
                //lqSwings.OnSslBreach += Lq_OnSslBreached;
                //lqSwings.OnBullFvgCreate += Lq_OnBullFvgCreate;
                //lqSwings.OnBearFvgCreate += Lq_OnBearFvgCreate;

                //fvg = LiquidityVoidsFVG2(Closes[2], mode: LUXLVFVGMode.Historical, back: 360, lqTH: 0.5, lqBC: Brushes.Teal, lqSC: Brushes.Crimson, lqVF: true, lqFC: Brushes.Gray);
                //AddChartIndicator(fvg);
            }
        }

        //private Series<bool?> Lq_BslBreach;
        //private Series<bool?> Lq_SslBreach;
        //private Series<bool?> Lq_BullFvg;
        //private Series<bool?> Lq_BearFvg;

        private Series<bool?> longBias;
        private Series<bool?> opp_close;

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

                //longBias[0] = lq.Lq_BslBreach[0] == true && lq.Lq_SslBreach[0] == false;
                longBias[0] = lq.Lq_Breach[0] == 1;
                opp_close[0] = opp_close[1];

                //if (lq.Lq_BslBreach[0].HasValue && lq.Lq_BslBreach[0].Value)
                //{
                //    Print("CurrentBar: " + CurrentBar);
                //    longBias[0] = false;
                //}

                if (longBias[0] == false && lq.Fvg[0] == -1)
                {
                    Print("CurrentBar: " + CurrentBar);
                    Print("Go Short");
                }

                //Lq_BslBreach[0] = Lq_BslBreach[1];
                //Lq_SslBreach[0] = Lq_SslBreach[1];
                //Lq_BullFvg[0] = Lq_BullFvg[1];
                //Lq_BearFvg[0] = Lq_BearFvg[1];

                //Print("Lq_BslBreach[0]: " + Lq_BslBreach[0].ToString());
                //Print("Lq_SslBreach[0]: " + Lq_SslBreach[0].ToString());

                //Print("Lq_BslBreach[1]: " + Lq_BslBreach[1].ToString());
                //Print("Lq_SslBreach[1]: " + Lq_SslBreach[1].ToString());

                // so if lq is swept? how to know if it is swept?
                // breach + opposite candle?, need another series to track that
                // fvg in ltf, need series to track that
                // if both conditions are met, take position
                // when to exit? opposite lq breached? 1:1? start with 1:1

                // if bsl is breached, set breach, then reset of ssl is breached
                // if bsl is breached and it is going to be true, all the time, then no point in series
                // if bsl is breached, set it to true and the set it to false when ssl is breached
                // set bsl to prev value, reset if ssl is breached

                //reset first if opp is breached


                //if (Lq_BslBreach[0].HasValue && Lq_BslBreach[0].Value)
                //{
                //    longBias[0] = false;
                //}

                //if (longBias[0] == true && Close[0] < Open[0])
                //{
                //    opp_close[0] = true;
                //}
                //else if (longBias[0] == false && Close[0] > Open[0])
                //{
                //    opp_close[0] = true;
                //}

            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        //private void Lq_OnBslBreached(double barNo)
        //{
        //    Lq_BslBreach[0] = true;
        //    Lq_SslBreach[0] = false;
        //    //Print("BSL breached: " + newValue);

        //    //Print("Lq_BslBreach[0]: " + Lq_BslBreach[0].ToString());
        //    //Print("Lq_SslBreach[0]: " + Lq_SslBreach[0].ToString());
        //}

        //private void Lq_OnSslBreached(double barNo)
        //{
        //    Lq_SslBreach[0] = true;
        //    Lq_BslBreach[0] = false;

        //    //Print("SSL breached: " + newValue);
        //}

        //private void Lq_OnBullFvgCreate(double barNo)
        //{
        //    Lq_BullFvg[0] = true;
        //    //Print("Bull FVG created: " + barNo);
        //}

        //private void Lq_OnBearFvgCreate(double barNo)
        //{
        //    Lq_BearFvg[0] = true;
        //    //Print("Bear FVG created: " + barNo);
        //}
    }
}
