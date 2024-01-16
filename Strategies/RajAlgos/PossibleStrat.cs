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
using System.Collections;
using NinjaTrader.Custom.Strategies.RajAlgos;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class PossibleStrat : Strategy
    {
        private UniqueStack<Ray> swingHighRays;        /*	Last Entry represents the most recent swing, i.e. 				*/
        private UniqueStack<Ray> swingLowRays;         /*	swingHighRays are sorted descedingly by price and vice versa	*/
        private int soundBar = 0;		// to prevent multiple sounds when Calculate = OnEachTick or OnPriceChange, so one sound per bar only.	

        private Series<double> secondarySeries;
        private int htf_mult;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"PossibleStrat";
                Name = "PossibleStrat";
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
                htf_mult = 5; // htf multiplier, default 5min/1min tf
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 5);

                swingHighRays = new UniqueStack<Ray>();
                swingLowRays = new UniqueStack<Ray>();

                SetStopLoss(CalculationMode.Ticks, TakeProfit);
                SetProfitTarget(CalculationMode.Ticks, StopLoss);

                ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                secondarySeries = new Series<double>(this);   
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            if (BarsInProgress != 0)
                return;

            if (CurrentBars[0] < 1 || CurrentBars[1] < 1)
                return;

            //secondarySeries[0] = Closes[1][0];
            secondarySeries[0] = Closes[1][0];


            /*			Swing break determination is on TickByTick basis, Swing creation not			*/
            /*			IRay's Anchor member gives the price at which the ray is draw...				*/

            //if (CurrentBar == 0)
            //{
            //	if (ChartPanel.PanelIndex != 0)  // support for placing indicator into indicator panel V1.02
            //	{
            //		DrawOnPricePanel = false;  // move draw objects to indicator panel
            //	}
            //}			
            
            
            if (CurrentBars[1] < 2 * Strength + 2)
                return;

            if (IsFirstTickOfBar)
            {   /*	Highs - Check whether we have a new swing peaking at CurrentBar - Strength - 1	 	*/
                int uiHistory = 1;

                bool bIsSwingHigh = true;

                while (uiHistory <= 2 * Strength + 1 && bIsSwingHigh)
                {
                    if (uiHistory != Strength + 1 && Highs[1][uiHistory] > Highs[1][Strength + 1] - double.Epsilon)
                    {
                        bIsSwingHigh = false;
                    }
                    else
                    {
                        uiHistory++;
                    }
                }

                if (bIsSwingHigh)
                {
                    double sh = Highs[1][Strength + 1];
                    Ray newRay = Draw.Ray(this, "highRay" + sh, false, (Strength + 2) * htf_mult, sh, 0, sh, SwingHighColor, DashStyleHelper.Dash, LineWidth);
                    swingHighRays.Push(sh, newRay);                    
                }

                /*	Low - Check whether we have a new swing with a bottom at CurrentBar - Strength - 1	*/
                bool bIsSwingLow = true;

                uiHistory = 1;

                while (uiHistory <= 2 * Strength + 1 && bIsSwingLow)
                {
                    if (uiHistory != Strength + 1 && Lows[1][uiHistory] < Lows[1][Strength + 1] + double.Epsilon)
                    {
                        bIsSwingLow = false;
                    }
                    else
                    {
                        uiHistory++;
                    }
                }

                if (bIsSwingLow)
                {
                    double sl = Lows[1][Strength + 1];
                    Ray newRay = Draw.Ray(this, "lowRay" + sl, false, (Strength + 2) * htf_mult, sl, 0, sl, SwingLowColor, DashStyleHelper.Dash, LineWidth);
                    swingLowRays.Push(sl, newRay);
                }
            }


            /*	Check the break of some swing	*/
            /*	High swings first...	*/
            Ray tmpRay = null;            
            if (swingHighRays.Count() != 0)
            {
                tmpRay = (Ray)swingHighRays.Peek();
            }

            while (swingHighRays.Count() != 0 && Highs[0][1] > tmpRay.StartAnchor.Price)
            {
                RemoveDrawObject(tmpRay.Tag);
                
                if (Closes[0][1] < tmpRay.StartAnchor.Price)
                {
                    EnterShort(Convert.ToInt32(DefaultQuantity), @"Short");
                }

                if (KeepBrokenLines)
                {   /*	Draw a line for the broken swing */
                    int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + (Strength + 2) * htf_mult;          /*	When did the ray being removed start?  Had to account for strength */
                    Draw.Line(this, "highLine" + (CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingHighColor, DashStyleHelper.Dot, LineWidth);
                }

                swingHighRays.Pop();
                
                if (swingHighRays.Count() != 0)
                {
                    tmpRay = (Ray)swingHighRays.Peek();
                }
            }

            /*		Low swings follow...	*/
            if (swingLowRays.Count() != 0)
            {
                tmpRay = (Ray)swingLowRays.Peek();
            }

            while (swingLowRays.Count() != 0 && Lows[0][1] < tmpRay.StartAnchor.Price)
            {
                RemoveDrawObject(tmpRay.Tag);
                
                if (Closes[0][1] > tmpRay.StartAnchor.Price)
                {
                    Print("tmpRay.StartAnchor.Price: " + tmpRay.StartAnchor.Price);
                    EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
                }

                if (KeepBrokenLines)
                {   /*	Draw a line for the broken swing */
                    int uiBarsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + (Strength + 2) * htf_mult;          /*	When did the ray being removed start?  Had to account for strength	*/
                    Draw.Line(this, "lowLine" + (CurrentBar - uiBarsAgo), false, uiBarsAgo, tmpRay.StartAnchor.Price, 0, tmpRay.StartAnchor.Price, SwingLowColor, DashStyleHelper.Dot, LineWidth);
                }

                swingLowRays.Pop();

                if (swingLowRays.Count() != 0)
                {
                    tmpRay = (Ray)swingLowRays.Peek();
                }
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
