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
                ClearOutputWindow();

                swingIndicator = Swing(PivotLength);
                SwingHighs = new List<SwingPoint>();
                SwingLows = new List<SwingPoint>();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < PivotLength) return;
            
            if (High[0] >= swingIndicator.SwingHigh[0])
            {
                SwingHighs.Add(new SwingPoint { Tag = "SwingHigh-" + CurrentBar, Price = swingIndicator.SwingHigh[0], BarIndex = CurrentBar, IsSwept = false });
            }

            if (Low[0] <= swingIndicator.SwingLow[0])
            {
                SwingLows.Add(new SwingPoint { Tag = "SwingLow-" + CurrentBar, Price = swingIndicator.SwingLow[0], BarIndex = CurrentBar, IsSwept = false });
            }

            CheckForSweep();

            RemoveSweptSwingPoints();

            PlotSwings();
        }

        private void CheckForSweep()
        {
            for (int i = 0; i < SwingHighs.Count - 1; i++)
            {
                if (!SwingHighs[i].IsSwept && High[0] >= SwingHighs[i].Price)                
                    SwingHighs[i].IsSwept = true;                
            }
            
            for (int i = 0; i < SwingLows.Count - 1; i++)
            {
                if (!SwingLows[i].IsSwept && Low[0] <= SwingLows[i].Price)
                    SwingLows[i].IsSwept = true;
            }            
        }

        private void PlotSwings()
        {
            foreach (var swingHigh in SwingHighs)
            {
                if (!swingHigh.IsSwept && !swingHigh.IsPainted)
                {
                    Draw.Line(this, swingHigh.Tag, false, 0, swingHigh.Price, -20, swingHigh.Price, Brushes.Green, DashStyleHelper.Solid, 1);
                    swingHigh.IsPainted = true;
                }
            }

            foreach (var swingLow in SwingLows)
            {
                if (!swingLow.IsSwept && !swingLow.IsPainted)
                {
                    Draw.Line(this, swingLow.Tag, false, 0, swingLow.Price, -20, swingLow.Price, Brushes.Red, DashStyleHelper.Solid, 1);
                    swingLow.IsPainted = true;
                }
            }
        }

        private void RemoveSweptSwingPoints()
        {
            for (int i = SwingHighs.Count - 1; i >= 0; i--)
            {
                if (SwingHighs[i].IsSwept)
                {
                    RemoveDrawObject(SwingHighs[i].Tag);
                    SwingHighs.RemoveAt(i);

                    Print(SwingHighs[i].Tag + " removed.");
                }
            }

            for (int i = SwingLows.Count - 1; i >= 0; i--)
            {
                if (SwingLows[i].IsSwept)
                {
                    RemoveDrawObject(SwingLows[i].Tag);
                    SwingLows.RemoveAt(i);
                }
            }
        }

        public class SwingPoint
        {
            public string Tag { get; set; }
            public double Price { get; set; }
            public int BarIndex { get; set; }
            public bool IsSwept { get; set; }
            public bool IsPainted { get; set; }
        }

        #region output

        [Browsable(false)]
        [XmlIgnore()]
        public List<SwingPoint> SwingHighs { get; set; }

        [Browsable(false)]
        [XmlIgnore()]
        public List<SwingPoint> SwingLows { get; set; }

        #endregion

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
