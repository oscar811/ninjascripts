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
    public class SmackyWithAtm : Strategy
    {		
        private MACD MACD1;
        private SMA SMA1;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Smacky with Atm";
                Name = "SmackyWithAtm";
                Calculate = Calculate.OnBarClose;
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
                BarsRequiredToTrade = 20;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
                CONTRACTS = 2;
                Macd_Signal = 26;
                Macd_Fast = 12;
                Macd_Diff = 9;
                Profit_Target = 20;
                Stop_Loss = 10;
                Enable_SMA_Filter = true;
                SMA_Filter = 100;
				AtmStrategyId = "your atm";
				
                Time_2 = false;
                Start_Time_2 = DateTime.Parse("03:22", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_2 = DateTime.Parse("04:07", System.Globalization.CultureInfo.InvariantCulture);
                Time_3 = false;
                Start_Time_3 = DateTime.Parse("07:52", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_3 = DateTime.Parse("08:37", System.Globalization.CultureInfo.InvariantCulture);
                Time_4 = false;
                Start_Time_4 = DateTime.Parse("09:22", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_4 = DateTime.Parse("10:07", System.Globalization.CultureInfo.InvariantCulture);
                Time_5 = false;
                Start_Time_5 = DateTime.Parse("13:52", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_5 = DateTime.Parse("14:37", System.Globalization.CultureInfo.InvariantCulture);
                Time_6 = false;
                Start_Time_6 = DateTime.Parse("15:22", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_6 = DateTime.Parse("15:55", System.Globalization.CultureInfo.InvariantCulture);
                Time_7 = false;
                Start_Time_7 = DateTime.Parse("01:52", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_7 = DateTime.Parse("02:37", System.Globalization.CultureInfo.InvariantCulture);
                Time_8 = false;
                Start_Time_8 = DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_8 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                Time_9 = false;
                Start_Time_9 = DateTime.Parse("05:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_9 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                Time_10 = false;
                Start_Time_10 = DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_10 = DateTime.Parse("00:30", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (State == State.Configure)
            {
				ClearOutputWindow();
            }
            else if (State == State.DataLoaded)
            {
                MACD1 = MACD(Close, Convert.ToInt32(Macd_Fast), Convert.ToInt32(Macd_Signal), Convert.ToInt32(Macd_Diff));
                SMA1 = SMA(Close, Convert.ToInt32(SMA_Filter));
                MACD1.Plots[0].Brush = Brushes.DarkCyan;
                MACD1.Plots[1].Brush = Brushes.Crimson;
                MACD1.Plots[2].Brush = Brushes.DodgerBlue;
                SMA1.Plots[0].Brush = Brushes.LawnGreen;
                AddChartIndicator(MACD1);
                AddChartIndicator(SMA1);
                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
				
            }
        }

        protected override void OnBarUpdate()
        {
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
            if (BarsInProgress != 0 || CurrentBars[0] < 1)
                return;
			
//			// Submits an entry limit order at the current low price to initiate an ATM Strategy if both order id and strategy id are in a reset state
//			// **** YOU MUST HAVE AN ATM STRATEGY TEMPLATE NAMED 'AtmStrategyTemplate' CREATED IN NINJATRADER (SUPERDOM FOR EXAMPLE) FOR THIS TO WORK ****
//			if (orderId.Length == 0 && AtmStrategyId.Length == 0 && Close[0] > Open[0])
//			{
//				isAtmStrategyCreated = false;  // reset atm strategy created check to false
//				AtmStrategyId = GetAtmStrategyUniqueId();
//				orderId = GetAtmStrategyUniqueId();
//				AtmStrategyCreate(OrderAction.Buy, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderId, "AtmStrategyTemplate", AtmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
//					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
//					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == AtmStrategyId)
//						isAtmStrategyCreated = true;
//				});
//			}

//			// Check that atm strategy was created before checking other properties
//			if (!isAtmStrategyCreated)
//				return;

            List<ConditionSet> conditionSets = new List<ConditionSet>
            {
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_2, Stop_Time_2, Time_2, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_3, Stop_Time_3, Time_3, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_4, Stop_Time_4, Time_4, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_5, Stop_Time_5, Time_5, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_6, Stop_Time_6, Time_6, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_7, Stop_Time_7, Time_7, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_8, Stop_Time_8, Time_8, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_9, Stop_Time_9, Time_9, Enable_SMA_Filter, SMA1[0], Direction.Short),
                new ConditionSet(GetCurrentAsk(0), () => CrossBelow(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_10, Stop_Time_10, Time_10, Enable_SMA_Filter, SMA1[0], Direction.Short),

                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_2, Stop_Time_2, Time_2, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_3, Stop_Time_3, Time_3, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_4, Stop_Time_4, Time_4, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_5, Stop_Time_5, Time_5, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_6, Stop_Time_6, Time_6, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_7, Stop_Time_7, Time_7, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_8, Stop_Time_8, Time_8, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_9, Stop_Time_9, Time_9, Enable_SMA_Filter, SMA1[0], Direction.Long),
                new ConditionSet(GetCurrentAsk(0), () => CrossAbove(MACD1.Default, MACD1.Avg, 1), Times[0][0], Start_Time_10, Stop_Time_10, Time_10, Enable_SMA_Filter, SMA1[0], Direction.Long),
            };
			
			if (GetCurrentAsk(0) < SMA1[0])
			{
				Print("Enable_SMA_Filter: " + Enable_SMA_Filter);
				Print("GetCurrentAsk(0) < SMA1[0]: " + (GetCurrentAsk(0) <= SMA1[0]));
			}
			
			foreach (ConditionSet conditionSet in conditionSets)
            {				
                if (conditionSet.CheckCondition())
                {
                    if (conditionSet.EntryDirection == Direction.Short)
                    {						
						ExitLong(Convert.ToInt32(CONTRACTS), "", "");
                        EnterShort(Convert.ToInt32(CONTRACTS), "");
                    }
                    else if (conditionSet.EntryDirection == Direction.Long)
                    {
                        ExitShort(Convert.ToInt32(CONTRACTS), "", "");
                        EnterLong(Convert.ToInt32(CONTRACTS), "");
                    }
                }
            }
			
//			// Check for a pending entry order
//			if (orderId.Length > 0)
//			{
//				string[] status = GetAtmStrategyEntryOrderStatus(orderId);

//				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
//				if (status.GetLength(0) > 0)
//				{
//					// Print out some information about the order to the output window
//					Print("The entry order average fill price is: " + status[0]);
//					Print("The entry order filled amount is: " + status[1]);
//					Print("The entry order order state is: " + status[2]);

//					// If the order state is terminal, reset the order id value
//					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
//						orderId = string.Empty;
//				}
//			} // If the strategy has terminated reset the strategy id
//			else if (AtmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(AtmStrategyId) == Cbi.MarketPosition.Flat)
//				AtmStrategyId = string.Empty;

//			if (AtmStrategyId.Length > 0)
//			{
//				// You can change the stop price
//				if (GetAtmStrategyMarketPosition(AtmStrategyId) != MarketPosition.Flat)
//					AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", AtmStrategyId);

//				// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
//				// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
//				Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(AtmStrategyId));
//				Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(AtmStrategyId));
//				Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(AtmStrategyId));
//				Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(AtmStrategyId));
//			}
        }

        private class ConditionSet
        {
            private double currentValue;
            private Func<bool> condition;
            private DateTime currentTime;
            private DateTime startTime;
            private DateTime stopTime;
            private bool timeCondition;
            private bool smaFilter;
            private double smaValue;
            public Direction EntryDirection;

            public ConditionSet(double currentValue, Func<bool> condition, DateTime currentTime, DateTime startTime, DateTime stopTime, bool timeCondition, bool smaFilter, double smaValue, Direction direction)
            {
                this.currentValue = currentValue;
                this.condition = condition;
                this.currentTime = currentTime;
                this.startTime = startTime;
                this.stopTime = stopTime;
                this.timeCondition = timeCondition;
                this.smaFilter = smaFilter;
                this.smaValue = smaValue;
                EntryDirection = direction;
            }

            public bool CheckCondition()
            {
				bool smaFilterResult = !smaFilter || (EntryDirection == Direction.Short && currentValue <= smaValue) || (EntryDirection == Direction.Long && currentValue >= smaValue);
                
				return condition() &&
                       (timeCondition && (currentTime.TimeOfDay >= startTime.TimeOfDay && currentTime.TimeOfDay <= stopTime.TimeOfDay)) 
						&& smaFilterResult;
            }
        }
        
        private enum Direction
        {
            Long,
            Short
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "CONTRACTS", Order = 1, GroupName = "Parameters")]
        public int CONTRACTS
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "ATM Strategy", Description = "Turn on or off", Order = 1, GroupName = "Parameters")]
        public string AtmStrategyId
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Macd_Signal", Description = "Macd Signal", Order = 2, GroupName = "Parameters")]
        public int Macd_Signal
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Macd_Fast", Description = "Macd 12,26,9 Default", Order = 3, GroupName = "Parameters")]
        public int Macd_Fast
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Macd_Diff", Description = "Macd Default 12,26,9", Order = 4, GroupName = "Parameters")]
        public int Macd_Diff
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Profit_Target", Order = 5, GroupName = "Parameters")]
        public int Profit_Target
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop_Loss", Order = 6, GroupName = "Parameters")]
        public int Stop_Loss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable_SMA_Filter", Description = "Filter out long and shorts based on if price is below or above the choosed MA", Order = 7, GroupName = "Parameters")]
        public bool Enable_SMA_Filter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SMA_Filter", Description = "Turn on or off", Order = 8, GroupName = "Parameters")]
        public int SMA_Filter
        { get; set; }		

        [NinjaScriptProperty]
        [Display(Name = "Time_2", Order = 9, GroupName = "Parameters")]
        public bool Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_2", Description = "3:22am-London", Order = 10, GroupName = "Parameters")]
        public DateTime Start_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_2", Description = "4:07am London", Order = 11, GroupName = "Parameters")]
        public DateTime Stop_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_3", Order = 12, GroupName = "Parameters")]
        public bool Time_3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_3", Description = "New York 7:52am", Order = 13, GroupName = "Parameters")]
        public DateTime Start_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_3", Description = "New York  08:37am", Order = 14, GroupName = "Parameters")]
        public DateTime Stop_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_4", Order = 15, GroupName = "Parameters")]
        public bool Time_4
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_4", Description = "9:22am", Order = 16, GroupName = "Parameters")]
        public DateTime Start_Time_4
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_4", Description = "10:07am", Order = 17, GroupName = "Parameters")]
        public DateTime Stop_Time_4
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_5", Order = 18, GroupName = "Parameters")]
        public bool Time_5
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_5", Order = 19, GroupName = "Parameters")]
        public DateTime Start_Time_5
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_5", Description = "2:37pm", Order = 20, GroupName = "Parameters")]
        public DateTime Stop_Time_5
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_6", Order = 21, GroupName = "Parameters")]
        public bool Time_6
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_6", Description = "3:22pm", Order = 22, GroupName = "Parameters")]
        public DateTime Start_Time_6
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_6", Description = "AMD sayd 4:07 but that is usually and exit time so we have this programmed to stop trading at 3:55pm", Order = 23, GroupName = "Parameters")]
        public DateTime Stop_Time_6
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_7", Order = 24, GroupName = "Parameters")]
        public bool Time_7
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_7", Description = "1:52am  Asia AMD entry time", Order = 25, GroupName = "Parameters")]
        public DateTime Start_Time_7
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_7", Description = "2:37am", Order = 26, GroupName = "Parameters")]
        public DateTime Stop_Time_7
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_8", Order = 27, GroupName = "Parameters")]
        public bool Time_8
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_8", Order = 28, GroupName = "Parameters")]
        public DateTime Start_Time_8
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_8", Order = 29, GroupName = "Parameters")]
        public DateTime Stop_Time_8
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_9", Order = 30, GroupName = "Parameters")]
        public bool Time_9
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_9", Order = 31, GroupName = "Parameters")]
        public DateTime Start_Time_9
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_9", Order = 32, GroupName = "Parameters")]
        public DateTime Stop_Time_9
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_10", Order = 33, GroupName = "Parameters")]
        public bool Time_10
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_10", Description = "12:00am", Order = 34, GroupName = "Parameters")]
        public DateTime Start_Time_10
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_10", Order = 35, GroupName = "Parameters")]
        public DateTime Stop_Time_10
        { get; set; }
        #endregion

    }
}
