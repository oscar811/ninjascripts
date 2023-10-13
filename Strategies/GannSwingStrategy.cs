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
namespace NinjaTrader.NinjaScript.Strategies
{
    public class GannSwingStrategy : Strategy
    {
        private int Length;
        private bool reverse;
        private double pos, possig;
        private double longStopPrice, shortStopPrice;
        private double longTrailPerc, shortTrailPerc;

        private Series<double> xHH;
        private Series<double> xLL;
        private Series<double> xGSO;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"GannSwingStrategy";
                Name = "GannSwingStrategy";
                Calculate = Calculate.OnBarClose;
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

                Length = 20;
                reverse = true;
				longTrailPerc = 0.3;
				shortTrailPerc = 0.3;
            }
            else if (State == State.Configure)
            {
                xHH = new Series<double>(this);
                xLL = new Series<double>(this);
                xGSO = new Series<double>(this);

                ClearOutputWindow();

                SetTrailStop("Long", CalculationMode.Percent, longTrailPerc, false);
                SetTrailStop("Short", CalculationMode.Percent, shortTrailPerc, false);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < 20)
                    return;

                xHH[0] = Highest(Length);

                // xLL = lowest(Length)
                xLL[0] = Lowest(Length);

                // xGSO = iff(xHH[2] > xHH[1] and xHH[0] > xHH[1], 1,
                //         iff(xLL[2] < xLL[1] and xLL[0] < xLL[1], -1, nz(xGSO[1],0)))

                //double xGSO;
                if (xHH[2] > xHH[1] && xHH[0] > xHH[1])
                {
                    xGSO[0] = 1;
                }
                else if (xLL[2] < xLL[1] && xLL[0] < xLL[1])
                {
                    xGSO[0] = -1;
                }
                else
                {
                    xGSO[0] = xGSO[1];
                }

                // pos = iff(xGSO > 0, 1,
                //         iff(xGSO < 0, -1, nz(pos[1], 0))) 

                int pos = xGSO[0] > 0 ? 1 : xGSO[0] < 0 ? -1 : 0;

                // possig = iff(reverse and pos == 1, -1,
                //         iff(reverse and pos == -1, 1, pos))

                int possig = reverse ? -1 * pos : pos;
                
				if (possig == 1)
                {
                    // strategy.entry("Long", strategy.long)
                    Print("possig" + possig.ToString());
                    EnterLong("Long");
                }                    
                else if (possig == -1)
                {
                    // strategy.entry("Short", strategy.short)	   	    
                    EnterShort("Short");
                }                    

            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }

        }

        private double Highest(int period)
        {
            double highest = double.MinValue;
            for (int i = 0; i < period; i++)
            {
                if (Highs[0][i] > highest)
                {
                    highest = Highs[0][i];
                }
            }
            return highest;
        }

        private double Lowest(int period)
        {
            double lowest = double.MaxValue;
            for (int i = 0; i < period; i++)
            {
                if (Lows[0][i] < lowest)
                {
                    lowest = Lows[0][i];
                }
            }
            return lowest;
        }
    }
}
