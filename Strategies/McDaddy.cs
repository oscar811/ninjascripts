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

//This namespace holds Strategies in this folder && is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class McDaddy : Strategy
	{
        private int fastLength = 12;
        private int slowLength = 26;
        private int signalLength = 9;
        private int crossscore = 10;
        private int indiside = 8;
        private int histside = 2;
        private bool shotsl = false;
        private double Mult = 1;
        private int Period = 10;
        private bool lookaheadi = true;

        private MACD MACD1;
        private MACD MACD2;

        private Series<double> Result;

        private ATR atr1;
        private Series<double> TUp;
        private Series<double> TDown;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"MACD Enhanced Strategy MTF";
				Name										= "McDaddy";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 600;
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
                Result = new Series<double>(this);
                TUp = new Series<double>(this);
                TDown = new Series<double>(this);

                AddDataSeries(Data.BarsPeriodType.Minute, 15);
			}
			else if (State == State.DataLoaded)
			{
                MACD1               = MACD(Close, 12, 26, 9);
                MACD2				= MACD(BarsArray[1], 12, 26, 9);

                atr1                = ATR(Period);

//                SetTrailStop(@"5", CalculationMode.Percent, 0, false);
				
				ClearOutputWindow(); //Clears Output window every time strategy is enabled
			}
		}

        private T Nz<T>(T? value, T defaultValue = default(T)) where T : struct
        {
            return value.HasValue ? value.Value : defaultValue;
        }

        protected override void OnBarUpdate()
        {
			try{
	            if (State == State.Realtime && IsFirstTickOfBar)  // Ignore intrabar ticks
					return;
				
				if (BarsInProgress != 0)
	                return;

	            if (Bars.BarsSinceNewTradingDay < 1) //Needs more than 1 bar on new day to begin trading. (Prevents trades if previous day closed as a pattern for our entry)			
	                return;

	            if (CurrentBar < BarsRequiredToTrade) 
					return;

	            //int HTF = "15";
	            int calc = 5;

	            double Anlys = count(MACD1);
	            //AnlysHfrm = lookaheadi ? request.security(syminfo.tickerid, HTF, count(), lookahead = barmerge.lookahead_on) :
	            //request.security(syminfo.tickerid, HTF, count(), lookahead = barmerge.lookahead_off);

	            double AnlysHfrm = count(MACD2);
	            Result[0] = (AnlysHfrm * calc + Anlys) / (calc + 1);

	            bool longCondition = Result[0] == Result[1] && Result[0] > 0;
	            if (longCondition)
	                EnterLong();

	            bool shortCondition = Result[0] == Result[1] && Result[0] < 0;
	            if (shortCondition)
	                EnterShort();            


	            int pos = longCondition ? 1 : -1;
	            double stline = 0.0;
	            double countstop__1 = countstop(pos);

	            // security_1 = request.security(syminfo.tickerid, HTF, countstop__1)
	            // stline:= ta.change(time(HTF)) != 0 or longCondition or shortCondition ? security_1:
	            // nz(stline[1])
			}
			catch (Exception e)
	        {
	            Print("Exception caught: " + e.Message);
	            Print("Stack Trace: " + e.StackTrace);
				
				// CloseStrategy("Error");
	        }
        }
        private double count(MACD macd)
        {
            //double indi = ta.ema(close, fastLength) - ta.ema(close, slowLength);
            double indi = macd.Default[0];
            double indi1 = macd.Default[1];
            double indi2 = macd.Default[2];
            //signal = ta.ema(indi, signalLength);
            double signal = macd.Avg[0];
            double signal1 = macd.Avg[1];
            double signal2 = macd.Avg[2];

            double Anlyse = 0.0;
            // direction of indi && histogram
            double hist = indi - signal;
            double hist1 = indi1 - signal1;

            Anlyse = indi > indi1 ? hist > hist1 ? indiside + histside : hist == hist1 ? indiside : indiside - histside : 0;
            Anlyse = Anlyse + (indi < indi1 ? hist < hist1 ? -(indiside + histside)
                : hist == hist1 ? -indiside : -(indiside - histside) : 0);
            Anlyse = Anlyse + (indi == indi1 ? hist > hist1 ? histside : hist < hist1 ? -histside : 0 : 0);

            // cross now earlier ?
            double countcross = indi >= signal && indi1 < signal1 ? crossscore
                                   : indi <= signal && indi1 > signal1 ? -crossscore : 0;

            double countcross1 = indi1 >= signal1 && indi2 < signal2 ? crossscore
                                   : indi1 <= signal1 && indi2 > signal2 ? -crossscore : 0;

            countcross = countcross + Nz(countcross1, 0.0) * 0.6;
            Anlyse = Anlyse + countcross;

            return Nz(Anlyse, 0.0);
        }

        private double countstop (int pos)
        {
            double hl2 = (Highs[0][0] + Lows[0][0]) / 2.0;
            double Upt = hl2 - Mult * atr1[0];
            double Dnt = hl2 + Mult * atr1[0];

            //double TUp = 0.0;
            //double TDown = 0.0;
            TUp[0] = Close[1] > TUp[1] ? Math.Max(Upt, TUp[1]) : Upt;
            TDown[0] = Close[1] < TDown[1] ? Math.Min(Dnt, TDown[1]) : Dnt;
            double tslmtf = pos == 1 ? TUp[0] : TDown[0];
            return tslmtf;
        }
    }
}
