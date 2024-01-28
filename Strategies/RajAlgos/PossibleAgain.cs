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
using NinjaTrader.Custom.Strategies.RajAlgos;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class PossibleAgain : Strategy
    {
        private UniqueStack<Ray> swingHighRays;        /*	Last Entry represents the most recent swing, i.e. 				*/
        private UniqueStack<Ray> swingLowRays;         /*	swingHighRays are sorted descedingly by price and vice versa	*/

        private Series<double> secondarySeries;
        private int htf_mult;
        private SwingRays2c swingRaysLtf;
        private SwingRays2c swingRaysHtf;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"PossibleAgain";
                Name = "PossibleAgain";
                Calculate = Calculate.OnEachTick;
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

                Strength = 4;
                TakeProfit = 100;
                StopLoss = 50;
                KeepBrokenLines = true; // defaulted to false to reduce overhead
                SwingHighColor = Brushes.DodgerBlue;
                SwingLowColor = Brushes.Fuchsia;
                LineWidth = 1;
                htf_mult = 1 / 5; // htf multiplier, default 5min/1min tf
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Minute, 5);

                swingHighRays = new UniqueStack<Ray>();
                swingLowRays = new UniqueStack<Ray>();

                SetStopLoss(CalculationMode.Ticks, TakeProfit);
                SetProfitTarget(CalculationMode.Ticks, StopLoss);

                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                swingRaysLtf = SwingRays2c(Closes[0], 4, true, 1);
                swingRaysHtf = SwingRays2c(Closes[1], 4, true, 1);
                AddChartIndicator(swingRaysLtf);
                AddChartIndicator(swingRaysHtf);
                //secondarySeries = new Series<double>(this);   
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

                //Print("Time inside strat: " + Time[0]);
                //Print("total high swings: " + swingRays2c.SwingHighRays.Count);
                //Print("total low swings: " + swingRays2c.SwingLowRays.Count);

                 Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                // Print("Time[0]: " + Time[0].ToString());
                 Print("CurrentBar: " + CurrentBar);

                // check for 5 min high sweep
                // check for 5 min low broken(not sweep)
                // check for 1 min high sweep
                // confirm trend (50 < 200)
                // enter short
                // wait till 5 min low broken

                // after 5 min sweep, store
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties
        [Range(2, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Swing Strength", Description = "Number of bars before/after each pivot bar", Order = 1, GroupName = "Parameters")]
        public int Strength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Take Profit (ticks)", Description = "", Order = 1, GroupName = "ATM")]
        public int TakeProfit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Stop Loss (ticks)", Description = "", Order = 1, GroupName = "ATM")]
        public int StopLoss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Keep broken lines", Description = "Show broken swing lines, beginning to end", Order = 3, GroupName = "Parameters")]
        public bool KeepBrokenLines
        { get; set; }

        [XmlIgnore]
        [Display(Name = "Swing High color", Description = "Color for swing high rays/lines", Order = 4, GroupName = "Options")]
        public Brush SwingHighColor
        { get; set; }

        [Browsable(false)]
        public string SwingHighColorSerializable
        {
            get { return Serialize.BrushToString(SwingHighColor); }
            set { SwingHighColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Swing Low color", Description = "Color for swing low rays/lines", Order = 5, GroupName = "Options")]
        public Brush SwingLowColor
        { get; set; }

        [Browsable(false)]
        public string SwingLowColorSerializable
        {
            get { return Serialize.BrushToString(SwingLowColor); }
            set { SwingLowColor = Serialize.StringToBrush(value); }
        }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Line width", Description = "Thickness of swing lines", Order = 6, GroupName = "Options")]
        public int LineWidth
        { get; set; }



        #endregion
    }
}
