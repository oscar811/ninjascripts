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
using NinjaTrader.NinjaScript.Strategies.RajAlgos;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class IncredibleImpossibleV4 : Strategy
	{
		private Indicators.RajIndicators.SwingRays2c SwingRays2c1;
		private MACD MACD1;
		private EMA EMA1;
		private EMA EMA2;
		private MACD MACD2;
		private List<TimePeriod> timePeriods;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "IncredibleImpossibleV4";
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
				Profit_Target					= 20;
				Stop_Loss					= 10;
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
				Time_8					= true;
				Start_Time_8						= DateTime.Parse("18:36", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_8						= DateTime.Parse("18:36", System.Globalization.CultureInfo.InvariantCulture);
				Time_9					= true;
				Start_Time_9						= DateTime.Parse("18:37", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_9						= DateTime.Parse("18:37", System.Globalization.CultureInfo.InvariantCulture);
				Time_10					= true;
				Start_Time_10						= DateTime.Parse("18:55", System.Globalization.CultureInfo.InvariantCulture);
				Stop_Time_10						= DateTime.Parse("18:55", System.Globalization.CultureInfo.InvariantCulture);
				Fast_Macd					= 3;
				Slow_Macd					= 8;
				Signal_Macd					= 8;
				Fast_EMA					= 50;
				Slow_Ema					= 200;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				SwingRays2c1				= SwingRays2c(Close, 7, 0, true, 1);
				MACD1				= MACD(Close, Convert.ToInt32(Fast_Macd), Convert.ToInt32(Slow_Macd), Convert.ToInt32(Signal_Macd));
				EMA1				= EMA(Close, Convert.ToInt32(Fast_EMA));
				EMA2				= EMA(Close, Convert.ToInt32(Slow_Ema));
				MACD2				= MACD(Close, Convert.ToInt32(Fast_Macd), Convert.ToInt32(Slow_Macd), Convert.ToInt32(Signal_Macd));
				MACD1.Plots[0].Brush = Brushes.DarkCyan;
				MACD1.Plots[1].Brush = Brushes.Crimson;
				MACD1.Plots[2].Brush = Brushes.Transparent;
				EMA1.Plots[0].Brush = Brushes.LightSkyBlue;
				EMA2.Plots[0].Brush = Brushes.Goldenrod;
				AddChartIndicator(SwingRays2c1);
				AddChartIndicator(MACD1);
				AddChartIndicator(EMA1);
				AddChartIndicator(EMA2);
				SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
				SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 2)
				return;

			 // Set 1
			if ((Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0]))
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 2
			if ((Times[0][0].TimeOfDay >= Start_Time_2.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_2.TimeOfDay)
				 && (Time_2 == true)
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0]))
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 3
			if ((Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 4
			if ((Times[0][0].TimeOfDay >= Start_Time_3.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_3.TimeOfDay)
				 && (Time_3 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 5
			if ((Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 6
			if ((Times[0][0].TimeOfDay >= Start_Time_4.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_4.TimeOfDay)
				 && (Time_4 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 7
			if ((Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 8
			if ((Times[0][0].TimeOfDay >= Start_Time_5.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_5.TimeOfDay)
				 && (Time_5 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 9
			if ((Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 10
			if ((Times[0][0].TimeOfDay >= Start_Time_6.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_6.TimeOfDay)
				 && (Time_6 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 11
			if ((Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 12
			if ((Times[0][0].TimeOfDay >= Start_Time_7.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_7.TimeOfDay)
				 && (Time_7 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 13
			if ((Times[0][0].TimeOfDay >= Start_Time_8.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_8.TimeOfDay)
				 && (Time_8 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 14
			if ((Times[0][0].TimeOfDay >= Start_Time_8.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_8.TimeOfDay)
				 && (Time_8 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Thursday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 15
			if ((Times[0][0].TimeOfDay >= Start_Time_9.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_9.TimeOfDay)
				 && (Time_9 == true)
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 16
			if ((Times[0][0].TimeOfDay >= Start_Time_9.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_9.TimeOfDay)
				 && (Time_9 == true)
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 17
			if ((Times[0][0].TimeOfDay >= Start_Time_10.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_10.TimeOfDay)
				 && (Time_10 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsHighBroken[0] == 1)
				 && (CrossBelow(MACD1.Default, MACD1.Avg, 2))
				 && (EMA1[0] < EMA2[0])))
			{
				EnterShort(Convert.ToInt32(CONTRACTS), "");
			}
			
			 // Set 18
			if ((Times[0][0].TimeOfDay >= Start_Time_10.TimeOfDay)
				 && (Times[0][0].TimeOfDay <= Stop_Time_10.TimeOfDay)
				 && (Time_10 == true)
				 // Condition group 1
				 && ((Times[0][0].DayOfWeek == DayOfWeek.Monday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Tuesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Wednesday)
				 || (Times[0][0].DayOfWeek == DayOfWeek.Friday))
				 // Condition group 1
				 && ((SwingRays2c1.IsLowBroken[0] == 1)
				 && (CrossAbove(MACD1.Default, MACD2.Avg, 2))
				 && (EMA1[0] > EMA2[0])))
			{
				EnterLong(Convert.ToInt32(CONTRACTS), "");
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
		[Display(Name="Profit_Target", Order=2, GroupName="Parameters")]
		public int Profit_Target
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop_Loss", Order=3, GroupName="Parameters")]
		public int Stop_Loss
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_2", Order=4, GroupName="Parameters")]
		public bool Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_2", Description="3:22am-London", Order=5, GroupName="Parameters")]
		public DateTime Start_Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_2", Description="4:07am London", Order=6, GroupName="Parameters")]
		public DateTime Stop_Time_2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_3", Order=7, GroupName="Parameters")]
		public bool Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_3", Description="New York 7:52am", Order=8, GroupName="Parameters")]
		public DateTime Start_Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_3", Description="New York  08:37am", Order=9, GroupName="Parameters")]
		public DateTime Stop_Time_3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_4", Order=10, GroupName="Parameters")]
		public bool Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_4", Description="9:22am", Order=11, GroupName="Parameters")]
		public DateTime Start_Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_4", Description="10:07am", Order=12, GroupName="Parameters")]
		public DateTime Stop_Time_4
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_5", Order=13, GroupName="Parameters")]
		public bool Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_5", Order=14, GroupName="Parameters")]
		public DateTime Start_Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_5", Description="2:37pm", Order=15, GroupName="Parameters")]
		public DateTime Stop_Time_5
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_6", Order=16, GroupName="Parameters")]
		public bool Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_6", Description="3:22pm", Order=17, GroupName="Parameters")]
		public DateTime Start_Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_6", Description="AMD sayd 4:07 but that is usually and exit time so we have this programmed to stop trading at 3:55pm", Order=18, GroupName="Parameters")]
		public DateTime Stop_Time_6
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_7", Order=19, GroupName="Parameters")]
		public bool Time_7
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_7", Description="1:52am  Asia AMD entry time", Order=20, GroupName="Parameters")]
		public DateTime Start_Time_7
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_7", Description="2:37am", Order=21, GroupName="Parameters")]
		public DateTime Stop_Time_7
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_8", Order=22, GroupName="Parameters")]
		public bool Time_8
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_8", Order=23, GroupName="Parameters")]
		public DateTime Start_Time_8
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_8", Order=24, GroupName="Parameters")]
		public DateTime Stop_Time_8
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_9", Order=25, GroupName="Parameters")]
		public bool Time_9
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_9", Order=26, GroupName="Parameters")]
		public DateTime Start_Time_9
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_9", Order=27, GroupName="Parameters")]
		public DateTime Stop_Time_9
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Time_10", Order=28, GroupName="Parameters")]
		public bool Time_10
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start_Time_10", Order=29, GroupName="Parameters")]
		public DateTime Start_Time_10
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Stop_Time_10", Order=30, GroupName="Parameters")]
		public DateTime Stop_Time_10
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast_Macd", Order=31, GroupName="Parameters")]
		public int Fast_Macd
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow_Macd", Order=32, GroupName="Parameters")]
		public int Slow_Macd
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Signal_Macd", Order=33, GroupName="Parameters")]
		public int Signal_Macd
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast_EMA", Order=34, GroupName="Parameters")]
		public int Fast_EMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow_Ema", Order=35, GroupName="Parameters")]
		public int Slow_Ema
		{ get; set; }
		#endregion

	}
}
