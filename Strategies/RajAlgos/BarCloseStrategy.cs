#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class BarCloseStrategy : Strategy
    {
        //private EMA ema1;
        private RSI rsi;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Count candles";
                Name = "BarCloseStrategy";
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

                UseRsiFilter = true;
                RsiPeriod = 10;
                RsiUpper = 70;
                RsiLower = 30;
                ConsecutiveClosesNeeded = 4;
                LongTP = 160;
                LongSL = 70;
                ShortTP = 120;
                ShortSL = 70;
            }
            else if (State == State.Configure)
            {                
                //SetProfitTarget("", CalculationMode.Ticks, LongTP);
                //SetStopLoss("", CalculationMode.Ticks, LongSL, false);
            }
            else if (State == State.DataLoaded)
            {
                //ema1 = EMA(Close, Convert.ToInt32(EmaPeriod));
                //AddChartIndicator(ema1);
                rsi = RSI(RsiPeriod, 2);
                AddChartIndicator(rsi);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                bool shouldEnterShort = true;
                bool shouldEnterLong = true;

                for (int i = 0; i < ConsecutiveClosesNeeded; i++)
                {
                    if (Close[i] <= Close[i + 1])
                    {
                        shouldEnterShort = false;
                    }
                    if (Close[i] >= Close[i + 1])
                    {
                        shouldEnterLong = false;
                    }

                    if (!shouldEnterShort && !shouldEnterLong)
                    {
                        break;
                    }
                }

                ExitIfTpOrSl();
                ExitIfOppositeDisplacement();

                if (shouldEnterShort && (!UseRsiFilter || rsi[0] > RsiUpper))
                {
                    EnterShort();
                }
                else if (shouldEnterLong && (!UseRsiFilter || rsi[0] < RsiLower))
                {
                    EnterLong();
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        private void ExitIfTpOrSl()
        {            
            if (Position.MarketPosition == MarketPosition.Long)
            {
                double profitLoss = (Close[0] - Position.AveragePrice) * Position.Quantity;
                if (profitLoss > LongTP)
                    ExitLong();
                else if (profitLoss < LongSL)
                    ExitShort();
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                double profitLoss = -1 * (Close[0] - Position.AveragePrice) * Position.Quantity;
                if (profitLoss > ShortTP)
                    ExitLong();
                else if (profitLoss < ShortSL)
                    ExitShort();
            }
        }

        private void ExitIfOppositeDisplacement()
        {
            if (Position.Quantity > 0)
            {
                double averageBodySize = CalculateAverageBodySize(20);

                if (Position.MarketPosition == MarketPosition.Long && (Open[0] - Close[0]) > 2 * averageBodySize)
                {
                    Print("averageBodySize: " + averageBodySize); 
                    ExitLong();
                }

                if (Position.MarketPosition == MarketPosition.Short && (Close[0] - Open[0]) > 2 * averageBodySize)
                {
                    Print("averageBodySize: " + averageBodySize);
                    ExitShort();
                }
            }
        }

        private double GetUnRealizedProfitOrLoss()
        {
            return (Position.MarketPosition == MarketPosition.Long ? 1 : -1 ) * (Close[0] - Position.AveragePrice) * Position.Quantity;
        }

        //private double realizedPnL = 0;

        ////protected override void OnExecutionUpdate(Execution execution, string executionId, Order order, Position position)
        //protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)        
        //{
        //    if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
        //    {
        //        double pnlForExecution = 0;
        //        if (execution.Order.OrderAction == OrderAction.Buy)
        //        {
        //            pnlForExecution = (execution.Price - Position.AveragePrice) * execution.Quantity;
        //        }
        //        else if (execution.Order.OrderAction == OrderAction.Sell)
        //        {
        //            pnlForExecution = (Position.AveragePrice - execution.Price) * execution.Quantity;
        //        }

        //        // Adjust for Buy/Sell and Long/Short
        //        if (Position.MarketPosition == MarketPosition.Short)
        //        {
        //            pnlForExecution = -pnlForExecution;
        //        }

        //        realizedPnL += pnlForExecution;

        //        // Print or handle the realized PnL
        //        Print("Realized PnL: " + realizedPnL);
        //    }

        //    //base.OnExecutionUpdate(execution, executionId, price, quantity, marketPosition, orderId, time);
        //}

        private double CalculateAverageBodySize(int period)
        {
            double totalBodySize = 0;

            for (int i = 0; i < period; i++)
            {
                double bodySize = Math.Abs(Close[i] - Open[i]);
                totalBodySize += bodySize;
            }

            return totalBodySize / period;
        }

        #region Properties

        [Range(3, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Consecutive Closes", Description = "Number of consecutive up/down closed candles", Order = 1, GroupName = "Strategy")]
        public int ConsecutiveClosesNeeded
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Rsi Filter", Order = 2, GroupName = "Strategy")]
        public bool UseRsiFilter
        { get; set; }

        [Range(6, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Rsi Period", Order = 3, GroupName = "Strategy")]
        public int RsiPeriod
        { get; set; }

        [Range(60, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Rsi Upper", Order = 4, GroupName = "Strategy")]
        public int RsiUpper
        { get; set; }

        [Range(15, 30)]
        [NinjaScriptProperty]
        [Display(Name = "Rsi Lower", Order = 4, GroupName = "Strategy")]
        public int RsiLower
        { get; set; }

        [Range(60, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Long TP (ticks)", Order = 4, GroupName = "ATM")]
        public int LongTP
        { get; set; }

        [Range(30, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Long SL (ticks)", Order = 5, GroupName = "ATM")]
        public int LongSL
        { get; set; }

        [Range(60, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Short TP (ticks)", Order = 4, GroupName = "ATM")]
        public int ShortTP
        { get; set; }

        [Range(30, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Short SL (ticks)", Order = 5, GroupName = "ATM")]
        public int ShortSL
        { get; set; }

        #endregion
    }
}
