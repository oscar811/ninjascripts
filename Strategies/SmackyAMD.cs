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
	public class SmackyAMD : Strategy
	{
		private MACD MACD1;
		private SMA SMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "SmackyAMD";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 60;
				IsFillLimitOnTouch							= true;
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
				CONTRACTS					= 2;
				Macd_Signal					= 26;
				Macd_Fast					= 12;
				Macd_Diff					= 9;
				Profit_Target					= 20;
				Stop_Loss					= 10;
				Enable_SMA_Filter					= true;
				SMA_Filter					= 100;
				Time_2					= false;
				Start_Time_2						= DateTime.Parse("03:22", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_2						= DateTime.Parse("04:07", System.Globalization.CultureInfo.InvariantCulture);
				Time_3					= false;
				Start_Time_3						= DateTime.Parse("07:52", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_3						= DateTime.Parse("08:37", System.Globalization.CultureInfo.InvariantCulture);
				Time_4					= false;
				Start_Time_4						= DateTime.Parse("09:22", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_4						= DateTime.Parse("10:07", System.Globalization.CultureInfo.InvariantCulture);
				Time_5					= false;
				Start_Time_5						= DateTime.Parse("13:52", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_5						= DateTime.Parse("14:37", System.Globalization.CultureInfo.InvariantCulture);
				Time_6					= false;
				Start_Time_6						= DateTime.Parse("15:22", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_6						= DateTime.Parse("15:55", System.Globalization.CultureInfo.InvariantCulture);
				Time_7					= false;
				Start_Time_7						= DateTime.Parse("01:52", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_7						= DateTime.Parse("02:37", System.Globalization.CultureInfo.InvariantCulture);
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				MACD1				= MACD(Close, Convert.ToInt32(Macd_Fast), Convert.ToInt32(Macd_Signal), Convert.ToInt32(Macd_Diff));
				SMA1				= SMA(Close, Convert.ToInt32(SMA_Filter));
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
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 2
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 3
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Monday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 4
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 5
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && (Times[0][0].DayOfWeek == DayOfWeek.Tuesday))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 6
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 7
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 8
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Monday)))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 9
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 10
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && (Times[0][0].DayOfWeek == DayOfWeek.Tuesday))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 11
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 12
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 13
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 14
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday)))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 15
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Monday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 16
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Monday)))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 17
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 18
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Friday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 19
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && (Times[0][0].DayOfWeek == DayOfWeek.Tuesday))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 20
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && (Times[0][0].DayOfWeek == DayOfWeek.Tuesday))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 21
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) <= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 22
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)))
			{
				ExitShort(Convert.ToInt32(CONTRACTS), "", "");
			}
			
			 // Set 23
			if ((CrossAbove(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 && (Enable_SMA_Filter == true)
				 && (GetCurrentAsk(0) >= SMA1[0])
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 24
			if ((CrossBelow(MACD1.Default, MACD1.Avg, 1))
				 && (Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 && (Enable_SMA_Filter == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)))
			{
				ExitLong(Convert.ToInt32(CONTRACTS), "", "");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CONTRACTS", Order=1, GroupName="Parameters")]
		public int CONTRACTS
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Macd_Signal", Description="Macd Signal", Order=2, GroupName="Parameters")]
		public int Macd_Signal
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Macd_Fast", Description="Macd 12,26,9 Default", Order=3, GroupName="Parameters")]
		public int Macd_Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Macd_Diff", Description="Macd Default 12,26,9", Order=4, GroupName="Parameters")]
		public int Macd_Diff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Profit_Target", Order=5, GroupName="Parameters")]
		public int Profit_Target
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop_Loss", Order=6, GroupName="Parameters")]
		public int Stop_Loss
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Enable_SMA_Filter", Description="Filter out long and shorts based on if price is below or above the choosed MA", Order=7, GroupName="Parameters")]
		public bool Enable_SMA_Filter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SMA_Filter", Description="Turn on or off", Order=8, GroupName="Parameters")]
		public int SMA_Filter
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_2", Order=9, GroupName="Parameters")]
		public bool Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_2", Description="3:22am-London", Order=10, GroupName="Parameters")]
		public DateTime Start_Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_2", Description="4:07am London", Order=11, GroupName="Parameters")]
		public DateTime Stop_Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_3", Order=12, GroupName="Parameters")]
		public bool Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_3", Description="New York 7:52am", Order=13, GroupName="Parameters")]
		public DateTime Start_Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_3", Description="New York  08:37am", Order=14, GroupName="Parameters")]
		public DateTime Stop_Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_4", Order=15, GroupName="Parameters")]
		public bool Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_4", Description="9:22am", Order=16, GroupName="Parameters")]
		public DateTime Start_Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_4", Description="10:07am", Order=17, GroupName="Parameters")]
		public DateTime Stop_Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_5", Order=18, GroupName="Parameters")]
		public bool Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_5", Order=19, GroupName="Parameters")]
		public DateTime Start_Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_5", Description="2:37pm", Order=20, GroupName="Parameters")]
		public DateTime Stop_Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_6", Order=21, GroupName="Parameters")]
		public bool Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_6", Description="3:22pm", Order=22, GroupName="Parameters")]
		public DateTime Start_Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_6", Description="AMD sayd 4:07 but that is usually and exit time so we have this programmed to stop trading at 3:55pm", Order=23, GroupName="Parameters")]
		public DateTime Stop_Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_7", Order=24, GroupName="Parameters")]
		public bool Time_7
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_7", Description="1:52am  Asia AMD entry time", Order=25, GroupName="Parameters")]
		public DateTime Start_Time_7
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_7", Description="2:37am", Order=26, GroupName="Parameters")]
		public DateTime Stop_Time_7
		{ get; set; }
		#endregion

	}
}
