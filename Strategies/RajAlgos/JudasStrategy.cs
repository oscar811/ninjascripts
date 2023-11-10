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
    public class JudasStrategy : Strategy
    {
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
                Profit_Target = 200;
                Stop_Loss = 100;

                Start_Time_1 = DateTime.Parse("20:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_1 = DateTime.Parse("22:00", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                AddPlot(Brushes.Green, "HighestHigh");
                AddPlot(Brushes.Red, "LowestLow");
            }
            else if (State == State.DataLoaded)
            {
                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        private DateTime startDateTime;
        private DateTime endDateTime;

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> HighestHigh
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LowestLow
        {
            get { return Values[1]; }
        }
        #endregion

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

                DateTime StartTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, Start_Time_1.Hour, Start_Time_1.Minute, 0);
                DateTime EndTime1 = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, Stop_Time_1.Hour, Stop_Time_1.Minute, 0);
                if (StartTime1.Hour == 0) StartTime1 = StartTime1.AddDays(1);
                if (EndTime1.Hour == 0) EndTime1 = EndTime1.AddDays(1);

                if (Time[0].TimeOfDay < new TimeSpan(StartTime1.Hour, EndTime1.Minute, 0))
                {
                    StartTime1 = StartTime1.AddDays(-1);
                    EndTime1 = EndTime1.AddDays(-1);
                }
                          

                if (Time[0] > EndTime1)
                {                    
                    int startBarsAgo = Bars.GetBar(StartTime1);
                    int endBarsAgo = Bars.GetBar(EndTime1);                    

                    /* Now that we have the start and end bars ago values for the specified time range we can calculate the highest high for this range

                    Note: We add 1 to the period range for MAX and MIN to compensate for the difference between "period" logic and "bars ago" logic.
                    "Period" logic means exactly how many bars you want to check including the current bar.
                    "Bars ago" logic means how many bars we are going to go backwards. The current bar is not counted because on that bar we aren't going back any bars so it would be "bars ago = 0" */
                    double highestHigh = MAX(High, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

                    // Now that we have the start and end bars ago values for the specified time range we can calculate the lowest low for this range
                    double lowestLow = MIN(Low, endBarsAgo - startBarsAgo + 1)[CurrentBar - endBarsAgo];

                    // Set the plot values
                    HighestHigh[0] = highestHigh;
                    LowestLow[0] = lowestLow;
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
