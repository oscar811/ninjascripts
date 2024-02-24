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
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class SuperTrendAIStrategy : Strategy
    {
        private SuperTrendAIClustering2 supertrend;
        private ATR atrIndicator;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Lux SuperTrendAIStrategy";
                Name = "SuperTrendAIStrategy";
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

                length = 10;
                minMult = 1;
                maxMult = 5;
                step = 0.5;
                minThreshold = 1;
                maxThreshold = 5;
                perfAlpha = 10.0;
                maxIter = 10000;
                maxData = 10000;
                takeProfit = 100;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                SetProfitTarget(CalculationMode.Ticks, takeProfit / TickSize);
            }
            else if (State == State.DataLoaded)
            {
                supertrend = SuperTrendAIClustering2(length, minMult, maxMult, step, perfAlpha, LuxSTAIFromCluster.Best, maxIter, maxData, 
                    Brushes.Crimson, Brushes.Teal, showSignals: true, showDash: false, dashLoc: LuxTablePosition.TopRight, textSize: 12);

                AddChartIndicator(supertrend);
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

                Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                //Print("Time[0]: " + Time[0].ToString());
                Print("CurrentBar: " + CurrentBar);
                if (supertrend.BullSignalValue[0].HasValue)
                    Print("supertrend.BullSignalValue[0].Value: " + supertrend.BullSignalValue[0].Value);
                
                if (supertrend.BearSignalValue[0].HasValue)
                    Print("supertrend.BearSignalValue[0].Value: " + supertrend.BearSignalValue[0].Value);

                //if (Position.MarketPosition == MarketPosition.Long)
                //{
                //    if (Close[0] < supertrend[0])
                //    {
                //        ExitLong();
                //    }
                //}

                //if (Position.MarketPosition == MarketPosition.Short)
                //{
                //    if (Close[0] > supertrend[0])
                //    {
                //        ExitShort();
                //    }
                //}

                //if (Close[0] > supertrend[0] && supertrend.BullSignalValue[0].HasValue
                //    && supertrend.BullSignalValue[0].Value >= minThreshold && supertrend.BullSignalValue[0].Value <= maxThreshold)
                //{
                //    Print("CurrentBar: " + CurrentBar);
                //    //Print("Time[0]: " + Time[0].ToString());
                //    Print("supertrend.BullSignalValue[0].Value: " + supertrend.BullSignalValue[0].Value);
                //    EnterLong(DefaultQuantity, Convert.ToString(CurrentBar) + " Long");
                //}

                //if (Close[0] < supertrend[0] && supertrend.BearSignalValue[0].HasValue
                //    && supertrend.BearSignalValue[0].Value >= minThreshold && supertrend.BearSignalValue[0].Value >= maxThreshold)
                //{
                //    EnterShort(DefaultQuantity, Convert.ToString(CurrentBar) + " Short");
                //}
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Length", Order = 1, GroupName = "1. Parameters")]
        public int length { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Factor Range Min", Order = 2, GroupName = "1. Parameters")]
        public int minMult { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Factor Range Max", Order = 3, GroupName = "1. Parameters")]
        public int maxMult { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Step", Order = 4, GroupName = "1. Parameters")]
        public double step { get; set; }

        [NinjaScriptProperty]
        [Range(0, 10)]
        [Display(Name = "Min. Threshold for entry", Order = 5, GroupName = "1. Parameters")]
        public int minThreshold { get; set; }

        [NinjaScriptProperty]
        [Range(0, 10)]
        [Display(Name = "Max. Threshold for entry", Order = 5, GroupName = "1. Parameters")]
        public int maxThreshold { get; set; }

        [NinjaScriptProperty]
        [Range(2.0, double.MaxValue)]
        [Display(Name = "Performance Memory", Order = 5, GroupName = "1. Parameters")]
        public double perfAlpha { get; set; }

        //[NinjaScriptProperty]
        //[Display(Name = "From Cluster", Order = 6, GroupName = "1. Parameters")]
        //public LuxSTAIFromCluster fromCluster { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Maximum Iteration Steps", Order = 7, GroupName = "2. Optimization")]
        public int maxIter { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Historical Bars Calculation", Order = 8, GroupName = "2. Optimization")]
        public int maxData { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "Take Profit (points)", Order = 3, GroupName = "3. Risk")]
        public double takeProfit { get; set; }
        #endregion
    }
}
