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
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20) return; // Wait for enough bars to be present

            // Identifying a bearish order block - a bullish candle before a strong bearish move
            if (Close[1] > Open[1] && // Previous candle was bullish
                Close[0] < Open[0] && // Current candle is bearish
                Close[0] < Close[1]) // Current candle closes below the previous candle's close
            {
                bearishOrderBlock = High[1]; // High of the bullish candle is potential bearish order block
                Draw.Line(this, "BearishBlock" + CurrentBar, false, 1, bearishOrderBlock, 0, bearishOrderBlock, Brushes.DodgerBlue, DashStyleHelper.Solid, 2);
            }

            // Identifying a bullish order block - a bearish candle before a strong bullish move
            if (Close[1] < Open[1] && // Previous candle was bearish
                Close[0] > Open[0] && // Current candle is bullish
                Close[0] > Close[1]) // Current candle closes above the previous candle's close
            {
                bullishOrderBlock = Low[1]; // Low of the bearish candle is potential bullish order block
                Draw.Line(this, "BullishBlock" + CurrentBar, false, 1, bullishOrderBlock, 0, bullishOrderBlock, Brushes.Crimson, DashStyleHelper.Solid, 2);
            }

            // The above conditions identify potential order blocks.
            // To use these for entries, stops, or targets, you'll need to add additional logic.
        }
    }
}
