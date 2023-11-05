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
    public class SimpleOrderBlockStrategy : Strategy
    {
        private double bearishOrderBlock;
        private double bullishOrderBlock;
        private double bullishOrderBlockLevel;
        private double atrValue;
        private ATR atr;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Plots ICT Order Blocks for potential institutional interest levels.";
                Name = "SimpleOrderBlockStrategy";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true; // To plot on the main chart
            }
            else if (State == State.Configure)
            {
                // Add any necessary plot configurations here
                AddPlot(Brushes.DodgerBlue, "BearishOrderBlock");
                AddPlot(Brushes.Crimson, "BullishOrderBlock");
            }
            else if (State == State.DataLoaded)
            {
                atr = ATR(14);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20) return; // Wait for enough bars to be present

            atrValue = ATR(14)[0];
            int lookBackPeriod = 5; // Define the look back period to identify a swing low
            double lowestLow = Low[LowestBar(Low, lookBackPeriod)];

            //// Identifying a bearish order block - a bullish candle before a strong bearish move
            //if (Close[1] > Open[1] && // Previous candle was bullish
            //    Close[0] < Open[0] && // Current candle is bearish
            //    Close[0] < Close[1]) // Current candle closes below the previous candle's close
            //{
            //    bearishOrderBlock = High[1]; // High of the bullish candle is potential bearish order block
            //    Draw.Line(this, "BearishBlock" + CurrentBar, false, 1, bearishOrderBlock, 0, bearishOrderBlock, Brushes.DodgerBlue, DashStyleHelper.Solid, 2);
            //}

            // Define conditions for a bullish order block
            bool lastTwoBearish = Close[2] < Open[2] && Close[3] < Open[3];
            bool followedByBullishCloseAbove = Close[0] > Math.Max(High[2], High[3]);
            bool aggressiveUpwardMove = Close[0] > Close[1] && (High[0] - Low[0]) > 2 * atrValue;
            bool sslSwept = Math.Min(Low[2], Low[3]) < lowestLow;

            if (lastTwoBearish && followedByBullishCloseAbove && aggressiveUpwardMove && sslSwept)
            {
                bullishOrderBlockLevel = Math.Max(High[2], High[3]);

                string tag = "BullishBlock" + CurrentBar;
                Draw.Line(this, tag, false, 3, bullishOrderBlockLevel, -20, bullishOrderBlockLevel, Brushes.Green, DashStyleHelper.Solid, 2);
                Draw.Text(this, tag + ":Text", "+OB", -18, bullishOrderBlockLevel + 4, Brushes.Green);

                Values[0][0] = bullishOrderBlockLevel;
            }

            // Define conditions for a bearish order block
            bool lastTwoBullish = Close[2] > Open[2] && Close[3] > Open[3];
            bool followedByBearishCloseBelow = Close[0] < Math.Min(Low[2], Low[3]);
            bool aggressiveDownwardMove = Close[0] < Close[1] && (High[0] - Low[0]) > 2 * atrValue;

            if (lastTwoBullish && followedByBearishCloseBelow && aggressiveDownwardMove)
            {
                double bearishOrderBlockLevel = Math.Min(Low[2], Low[3]);

                string tag = "BearishBlock" + CurrentBar;
                Draw.Line(this, tag, false, 3, bearishOrderBlockLevel, -20, bearishOrderBlockLevel, Brushes.Red, DashStyleHelper.Solid, 2);
                Draw.Text(this, tag + "Text", "-OB", -18, bearishOrderBlockLevel - 4, Brushes.Red); // Offset the text below the line

                Values[0][0] = bearishOrderBlockLevel; // Storing the level, assuming the Values array has been defined
            }
        }
    }
}
