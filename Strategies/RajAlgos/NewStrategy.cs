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
using NinjaTrader.NinjaScript.Indicators.RajIndicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class NewStrategy : Strategy
    {
        // private SessionLevels sessionLevels;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"NewStrategy";
                Name = "NewStrategy";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 1;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 10;

                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = false;

                EnableAtm = false;
                AtmStrategyTemplateId = "your atm";
//                Profit_Target = 2000;
//                Stop_Loss = 1000;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                //sessionLevels = SessionLevels(true, true)
                AddPlot(Brushes.Green, "HighestHigh");
                AddPlot(Brushes.Red, "LowestLow");
            }
            else if (State == State.DataLoaded)
            {
//                sessionLevels = SessionLevels(Close, true, true, true, true, DateTime.Parse("6:00 PM"), DateTime.Parse("11:59 PM"), true, DateTime.Parse("12:00 AM"), DateTime.Parse("6:00 AM"), true, DateTime.Parse("6:00 AM"), DateTime.Parse("12:00 PM"), true, DateTime.Parse("12:00 PM"), DateTime.Parse("6:00 PM"));

//                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
//                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
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

                // Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                // Print("Time[0]: " + Time[0].ToString());
                // Print("CurrentBar: " + CurrentBar);

                // write your logic here
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties

        // ATM properties
        [NinjaScriptProperty]
        [Display(Name = "Enable Atm", Order = 2, GroupName = "ATM")]
        public bool EnableAtm
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATM Strategy (Only real time)", Order = 3, GroupName = "ATM")]
        public string AtmStrategyTemplateId
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr Period", Order = 3, GroupName = "ATM")]
        public int atrPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for SL", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForStopLoss { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Atr mult for TP", Order = 3, GroupName = "ATM")]
        public double atrMultiplierForTakeProfit { get; set; }

        // [NinjaScriptProperty]
        // [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        // [Display(Name = "Asian Start", Order = 1, GroupName = "Time")]
        // public DateTime Start_Time_1
        // { get; set; }

        #endregion
    }
}
