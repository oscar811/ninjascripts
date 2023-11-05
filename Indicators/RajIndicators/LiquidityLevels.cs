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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class LiquidityLevels : Indicator
    {
        private Swing swingIndicator;
        private List<SwingPoint> swingHighs = new List<SwingPoint>();
        private List<SwingPoint> swingLows = new List<SwingPoint>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"LiquidityLevels.";
                Name = "LiquidityLevels";
                DisplayInDataBox = false;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;
                IsOverlay = true;

                PivotLength = 5;
            }
            else if (State == State.Configure)
            {
                swingIndicator = Swing(PivotLength);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < PivotLength) return;

            // Update our lists of swing highs and lows
            //if (swingIndicator.SwingHigh[0] > 0)
            if (High[0] >= swingIndicator.SwingHigh[0])
            {
                swingHighs.Add(new SwingPoint { Price = swingIndicator.SwingHigh[0], BarIndex = CurrentBar, IsSwept = false });
            }

            //if (swingIndicator.SwingLow[0] > 0)
            if (Low[0] <= swingIndicator.SwingLow[0])
            {
                swingLows.Add(new SwingPoint { Price = swingIndicator.SwingLow[0], BarIndex = CurrentBar, IsSwept = false });
            }

            // Check for sweep
            CheckForSweep();

            RemoveSweptSwingPoints();

            // Plot remaining unswept swings
            PlotSwings();

            Print("swingHighs.Count:" + swingHighs.Count);
            Print("swingLows.Count:" + swingLows.Count);
        }

        private void CheckForSweep()
        {
            // Logic to mark swept swing points
            for (int i = 0; i < swingHighs.Count - 1; i++)
            {
                if (!swingHighs[i].IsSwept && High[0] >= swingHighs[i].Price)
                {
                    swingHighs[i].IsSwept = true;
                }
            }

            //Print("IsSwept: " + swingHighs.Count(s => s.IsSwept));
            //Print("UnSwept: " + swingHighs.Count(s => !s.IsSwept));

            for (int i = 0; i < swingLows.Count - 1; i++)
            {
                if (!swingLows[i].IsSwept && Low[0] <= swingLows[i].Price)
                    swingLows[i].IsSwept = true;
            }

            // Remove swept swing points
            swingHighs.RemoveAll(swingHigh => swingHigh.IsSwept);
            swingLows.RemoveAll(swingLow => swingLow.IsSwept);
        }

        private void PlotSwings()
        {
            // Logic to plot unswept swing points on the chart
            foreach (var swingHigh in swingHighs)
            {
                if (!swingHigh.IsSwept)
                    Draw.Dot(this, "SwingHigh" + CurrentBar, false, 0, swingHigh.Price, Brushes.Green);
            }

            foreach (var swingLow in swingLows)
            {
                if (!swingLow.IsSwept)
                    Draw.Dot(this, "SwingLow" + CurrentBar, false, 0, swingLow.Price, Brushes.Red);
            }
        }

        private void RemoveSweptSwingPoints()
        {
            for (int i = swingHighs.Count - 1; i >= 0; i--)
            {
                if (swingHighs[i].IsSwept)
                {
                    // Swing high has been swept, remove the plot and the point from the list
                    RemoveDrawObject("SwingHigh" + swingHighs[i].BarIndex);
                    swingHighs.RemoveAt(i);

                    Print("SwingHigh" + swingHighs[i].BarIndex + " removed.");
                }
            }

            for (int i = swingLows.Count - 1; i >= 0; i--)
            {
                if (swingLows[i].IsSwept)
                {
                    // Swing low has been swept, remove the plot and the point from the list
                    RemoveDrawObject("SwingLow" + swingLows[i].BarIndex);
                    swingLows.RemoveAt(i);
                }
            }
        }

        public class SwingPoint
        {
            public double Price { get; set; }
            public int BarIndex { get; set; }
            public bool IsSwept { get; set; }
        }    

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Pivot Length", Order = 1, GroupName = "Parameters")]
        public int PivotLength { get; set; }        

        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.LiquidityLevels[] cacheLiquidityLevels;
		public RajIndicators.LiquidityLevels LiquidityLevels(int pivotLength)
		{
			return LiquidityLevels(Input, pivotLength);
		}

		public RajIndicators.LiquidityLevels LiquidityLevels(ISeries<double> input, int pivotLength)
		{
			if (cacheLiquidityLevels != null)
				for (int idx = 0; idx < cacheLiquidityLevels.Length; idx++)
					if (cacheLiquidityLevels[idx] != null && cacheLiquidityLevels[idx].PivotLength == pivotLength && cacheLiquidityLevels[idx].EqualsInput(input))
						return cacheLiquidityLevels[idx];
			return CacheIndicator<RajIndicators.LiquidityLevels>(new RajIndicators.LiquidityLevels(){ PivotLength = pivotLength }, input, ref cacheLiquidityLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.LiquidityLevels LiquidityLevels(int pivotLength)
		{
			return indicator.LiquidityLevels(Input, pivotLength);
		}

		public Indicators.RajIndicators.LiquidityLevels LiquidityLevels(ISeries<double> input , int pivotLength)
		{
			return indicator.LiquidityLevels(input, pivotLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.LiquidityLevels LiquidityLevels(int pivotLength)
		{
			return indicator.LiquidityLevels(Input, pivotLength);
		}

		public Indicators.RajIndicators.LiquidityLevels LiquidityLevels(ISeries<double> input , int pivotLength)
		{
			return indicator.LiquidityLevels(input, pivotLength);
		}
	}
}

#endregion
