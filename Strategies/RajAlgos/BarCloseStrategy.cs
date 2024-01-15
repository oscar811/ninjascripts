#region Using declarations
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using System;
using System.ComponentModel.DataAnnotations;
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

                UseRsiFilter = false;
                EmaPeriod = 20;
                ConsecutiveClosesNeeded = 3;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                //SetProfitTarget("", CalculationMode.Ticks, 100);
                //SetStopLoss("", CalculationMode.Ticks, 80, false);
            }
            else if (State == State.DataLoaded)
            {
                //ema1 = EMA(Close, Convert.ToInt32(EmaPeriod));
                //AddChartIndicator(ema1);
                rsi = RSI(14, 3);
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

                ExitIfTpOrSL();

                if (shouldEnterShort && (!UseRsiFilter || rsi[0] > 70))
                {
                    EnterShort();
                }
                else if (shouldEnterLong && (!UseRsiFilter || rsi[0] < 30))
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

        private void ExitIfTpOrSL()
        {
            if (Position.Quantity != 0)
            {
                double profitOrLoss = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks);
                Print("Position.MarketPosition: " + Position.MarketPosition);
                Print("profitOrLoss: " + profitOrLoss);

                if (Position.MarketPosition == MarketPosition.Long)
                {
                    if (profitOrLoss > 100 || profitOrLoss < -80)
                        ExitLong();
                }
                else if (Position.MarketPosition == MarketPosition.Short)
                {
                    if (profitOrLoss > 100 || profitOrLoss < -80)
                        ExitShort();
                }
            }
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

        [Range(20, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Ema Period", Order = 3, GroupName = "Strategy")]
        public int EmaPeriod
        { get; set; }


        #endregion
    }
}
