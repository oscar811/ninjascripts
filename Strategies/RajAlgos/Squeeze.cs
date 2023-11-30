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
	public class SqueezeStrategy : Strategy
	{
        //private pjsQQE pjsQQE1;
        private AntoQQE antoQQE;
        private RSqueeze rSqueeze;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Squeeze Momentum.";
				Name										= "Squeeze";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;

				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
                ClearOutputWindow();                
            }
            else if (State == State.DataLoaded)
			{
                //pjsQQE1 = pjsQQE(Close, 14, 5);
                //antoQQE = AntoQQE(Close, 14, 5, 5, 7, 1);
                antoQQE = AntoQQE(Close, 6, 6, 4.2, 10, 1);
                rSqueeze = RSqueeze(Close, RSqueezeTypes.RSqueezeStyle.BBSqueeze);

                AddChartIndicator(antoQQE);
                AddChartIndicator(rSqueeze);
            }
        }

		protected override void OnBarUpdate()
		{
            try
            {
                if (BarsInProgress != 0)
                    return;

                if (CurrentBars[0] < BarsRequiredToTrade)
                    return;


                Print("rSqueeze.SqueezeDots[0]: " + rSqueeze.SqueezeDots[0]);
                //Print("rSqueeze.PlotBrushes[1][0]: " + (rSqueeze.PlotBrushes[1][0] == rSqueeze.SqueezeDotBrush));
                Print("rSqueeze.PlotBrushes[1][0]: " + (rSqueeze.PlotBrushes[0][0] == rSqueeze.HistAboveZeroFalling));

                if (antoQQE.hist[0] > 0 && antoQQE.FastAtrrsi1[0] >= antoQQE.Rsi_index1[0]
                    && rSqueeze.PlotBrushes[1][0] == rSqueeze.NormalDotBrush && rSqueeze.PlotBrushes[0][0] == rSqueeze.HistAboveZeroRising)
                {
                    EnterLong(Convert.ToInt32(DefaultQuantity), "");
                }

                if (Position.MarketPosition == MarketPosition.Long
                    && rSqueeze.PlotBrushes[0][0] == rSqueeze.HistAboveZeroFalling && rSqueeze.PlotBrushes[0][1] == rSqueeze.HistAboveZeroFalling
                    && rSqueeze.PlotBrushes[0][2] == rSqueeze.HistAboveZeroFalling)
                {
                    ExitLong();
                }

                //if (antoQQE.hist[0] < 0
                //    && rSqueeze.PlotBrushes[1][0] == rSqueeze.NormalDotBrush && rSqueeze.PlotBrushes[0][0] == rSqueeze.HistBelowZeroFalling)
                //{
                //    EnterShort(Convert.ToInt32(DefaultQuantity), "");
                //}

                //if (Position.MarketPosition == MarketPosition.Short
                //    && rSqueeze.PlotBrushes[0][0] == rSqueeze.HistBelowZeroRising && rSqueeze.PlotBrushes[0][1] == rSqueeze.HistBelowZeroRising
                //    && rSqueeze.PlotBrushes[0][2] == rSqueeze.HistBelowZeroRising)
                //{
                //    ExitShort();
                //}
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }

        }
	}
}
