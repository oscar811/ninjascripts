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
using NinjaTrader.NinjaScript.Indicators.RajIndicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class UnicornStrategy : Strategy
    {
        private Swing swingIndicator;
        private double bearishCandleLow;
        private int bearishCandleBar;
        private bool sslSwept;
        private int sslSweptBar;
        private List<SwingPoint> swingHighs = new List<SwingPoint>();
        private List<SwingPoint> swingLows = new List<SwingPoint>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Bearish strategy that enters on liquidity sweeps of Swing High with a close below the low of the prior bearish candle.";
                Name = "UnicornStrategy";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 60;
                IsFillLimitOnTouch = true;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 10;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                swingSize = 10;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                swingIndicator = Swing(swingSize);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < swingSize || CurrentBar < BarsRequiredToTrade) return;

            //Draw.Text(this, "CurrentBarText" + CurrentBar, CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Black);
            //Print("CurrentBar: " + CurrentBar);

            AddSwingPoints();

            if (CheckIfSwingHighIsTaken(High[0]))
            {
                sslSweptBar = CurrentBar;

                int bearishCandleBarAgo = FindBearishCandleBefore(CurrentBar);
                bearishCandleBar = CurrentBar - bearishCandleBarAgo;
                bearishCandleLow = Low[bearishCandleBarAgo];
            }

            //            if (Close[0] < bearishCandleLow && (CurrentBar - bearishCandleBar) < 10 && sslSwept && (CurrentBar - sslSweptBar) < 5)
            if (Close[0] < bearishCandleLow && (CurrentBar - bearishCandleBar) < 20)
            {
                //Print("bearishCandleBar: " + bearishCandleBar);
                Print("bearishCandleLow: " + bearishCandleLow);
                //Print("sslSweptBar: " + sslSweptBar);

                FvgType? fvgType = isFVGPresent(CurrentBar - sslSweptBar);

                if (fvgType.HasValue && fvgType.Value == FvgType.Bearish)
                {
                    // This is where you could place a short entry
                    EnterShort("Bearish Unicorn Entry");

                    string tag = "BB" + bearishCandleLow;

                    if (DrawObjects.FirstOrDefault(o => o.Tag == tag) == null)
                    {
                        Draw.Line(this, tag, false, CurrentBar - bearishCandleBar, bearishCandleLow, -20, bearishCandleLow, Brushes.Red, DashStyleHelper.Solid, 2);
                        Draw.Text(this, tag + "Text", "-Uni", -22, bearishCandleLow, Brushes.Red);
                    }
                }
            }

            RemoveSweptSwingPoints();
        }

        private FvgType? isFVGPresent(int barsAgo)
        {
            double gapSizeThreshold = 5;
            for (int i = 0; i <= barsAgo; i++)
            {
                if (High[i] < Low[i + 2] && (Low[i + 2] - High[i]) >= gapSizeThreshold * TickSize)
                {
                    return FvgType.Bearish;
                }
                else if (Low[i] > High[i + 2] && (Low[i] - High[i + 2]) >= gapSizeThreshold * TickSize)
                {
                    return FvgType.Bullish;
                }
            }
            return null;
        }

        enum FvgType
        {
            Bearish,
            Bullish
        }

        private void AddSwingPoints()
        {
            if (High[0] >= swingIndicator.SwingHigh[0])
            {
                swingHighs.Add(new SwingPoint { Price = High[0], BarIndex = CurrentBar, IsSwept = false });
            }

            if (Low[0] <= swingIndicator.SwingLow[0])
            {
                swingLows.Add(new SwingPoint { Price = Low[0], BarIndex = CurrentBar, IsSwept = false });
            }
        }

        private void RemoveSweptSwingPoints()
        {
            for (int i = 0; i < swingHighs.Count; i++)
            {
                if (!swingHighs[i].IsSwept && High[0] > swingHighs[i].Price)
                {
                    swingHighs[i].IsSwept = true;
                }
            }

            for (int i = 0; i < swingLows.Count; i++)
            {
                if (!swingLows[i].IsSwept && Low[0] < swingLows[i].Price)
                    swingLows[i].IsSwept = true;
            }

            swingHighs.RemoveAll(swingHigh => swingHigh.IsSwept);
            swingLows.RemoveAll(swingLow => swingLow.IsSwept);
        }

        private int FindBearishCandleBefore(int barIndex)
        {
            for (int i = 1; i <= 10; i++)
            {
                if (Close[i] < Open[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private bool CheckIfSwingHighIsTaken(double highValue)
        {
            for (int i = 0; i <= Math.Min(1, swingHighs.Count - 1); i++)
            {
                if (highValue >= swingHighs[i].Price && swingHighs[i].IsSwept == false)
                {
                    Print("swing high taken: " + swingHighs[i].Price);

                    return true;
                }
            }

            return false;
        }

        public class SwingPoint
        {
            public double Price { get; set; }
            public int BarIndex { get; set; }
            public bool IsSwept { get; set; }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Swing Size", Order = 1, GroupName = "Parameters")]
        public int swingSize
        { get; set; }

        #endregion
    }
}
