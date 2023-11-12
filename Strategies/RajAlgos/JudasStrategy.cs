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

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class JudasStrategy : Strategy
    {
        private SessionLevels sessionLevels;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"ICT Judas Swing Strategy";
                Name = "JudasStrategy";
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
                Profit_Target = 2000;
                Stop_Loss = 1000;                
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
                sessionLevels = SessionLevels(Close, true, true, true, true, DateTime.Parse("6:00 PM"), DateTime.Parse("11:59 PM"), true, DateTime.Parse("12:00 AM"), DateTime.Parse("6:00 AM"), true, DateTime.Parse("6:00 AM"), DateTime.Parse("12:00 PM"), true, DateTime.Parse("12:00 PM"), DateTime.Parse("6:00 PM"));

                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        private bool isHighTaken;
        private bool isLowTaken;
        private bool isHighTakenFirst;
        private bool isLowTakenFirst;

        protected override void OnBarUpdate()
        {
            // consider 5 min timeframe
            // mark asian high and low
            // check isAsianHighTaken  and isAsianLowTaken, also record which is taken first
            // if high is taken first, and then low is taken, bullish bias
            //// wait for reversal, wait for displacement/fvg, enter in fvg and target some lq level
            // vice versa if low is taken first and then high
            // if not both is taken, no trade
            // need a good indicator to mark swing high/low (possibly in htf) for lq targets
            // we are start with profit target or atm for now

            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                if (sessionLevels.Asian_High[0] == 0)
                {
                    isHighTaken = false;
                    isLowTaken = false;
                }
                
                if (High[0] > sessionLevels.Asian_High[0] && sessionLevels.London_High[0] == 0)
                {
                    isHighTaken = true;
                    if (isLowTaken)
                    {
                        isLowTakenFirst = true;  
                    }
                }

                if (Low[0] < sessionLevels.Asian_Low[0] && sessionLevels.London_High[0] == 0)
                {
                    isLowTaken = true;
                    if (isHighTaken)
                    {
                        isHighTakenFirst = true;
                    }
                }

            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties        

        [NinjaScriptProperty]
        [Display(Name = "Enable Atm", Order = 2, GroupName = "ATM")]
        public bool EnableAtm
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATM Strategy (Only real time)", Order = 3, GroupName = "ATM")]
        public string AtmStrategyTemplateId
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Profit_Target", Order = 4, GroupName = "ATM")]
        public int Profit_Target
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop_Loss", Order = 5, GroupName = "ATM")]
        public int Stop_Loss
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asian Start", Order = 1, GroupName = "Time")]
        public DateTime Start_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asian End", Order = 2, GroupName = "Time")]
        public DateTime Stop_Time_1
        { get; set; }
        #endregion
    }
}
