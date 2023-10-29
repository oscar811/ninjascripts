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
	public class FVGBot : Strategy
	{
		private int Status;
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		private Account myAccount;
		
		private MarketPosition side; 
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "FVGBot";
				Calculate									= Calculate.OnEachTick;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= false;
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
				ATM					= @"Jferheart";
				
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
				GMT					= 0;
				ss1						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se1						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss2						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se2						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss3						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se3						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss4						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se4						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss5						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se5						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss6						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se6						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss7						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se7						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss8						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se8						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss9						= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se9						= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss10					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se10					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss11					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se11					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss12					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se12					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss13					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se13					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss14					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se14					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss15					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se15					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				ss16					= DateTime.Parse("10:01", System.Globalization.CultureInfo.InvariantCulture);
				se16					= DateTime.Parse("10:59", System.Globalization.CultureInfo.InvariantCulture);
				act1					=false;
				act2					=false;
				act3					=false;
				act4					=false;
				act5					=false;
				act6					=false;
				act7					=false;
				act8					=false;
				act9					=false;
				act10					=false;
				act11					=false;
				act12					=false;
				act13					=false;
				act14					=false;
				act15					=false;
				act16					=false;
				Status					= 1;
				side = MarketPosition.Flat;
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 2)
				return;
			
			
			if(State != State.Historical && orderId.Length > 0){
				//string[] entryOrder = GetAtmStrategyEntryOrderStatus(orderId);
				//if (entryOrder.Length > 0)
    			//{
					//Print("Average fill price is " + entryOrder[0].ToString());
        			//Print("Filled amount is " + entryOrder[1].ToString());
        			//Print("Current state is " + entryOrder[2].ToString());
					//if(entryOrder[2].ToString()){
						
					//}
    			//}
				if(atmStrategyId.Length > 0){
					//if (GetAtmStrategyMarketPosition("id") == MarketPosition.Flat)
        			side = GetAtmStrategyMarketPosition(atmStrategyId);
					Print("side " + side);
				}
			}
			 // Set 1
			//Print(Times[0][0].TimeOfDay);
			if (Low[0] > High[2] && isInSession(Times[0][0].TimeOfDay))
			{
				if(orderId.Length > 0){
					AtmStrategyCancelEntryOrder(orderId);
				}
				//Print("a");
				if(State != State.Historical && side != MarketPosition.Long){
					if(atmStrategyId.Length>0){
						AtmStrategyClose(atmStrategyId);
					}
					//Print("b");
					//isAtmStrategyCreated = false;
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.Buy, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderId, ATM, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
					//if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						//isAtmStrategyCreated = true;
					});
					
				}
				//Draw.Dot(this, @"MyCustomStrategy Dot_1 " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (10 * TickSize)) , Brushes.CornflowerBlue);
				
				//EnterLong(Convert.ToInt32(DefaultQuantity), @"long");
				//AtmStrategyCreate(Cbi.OrderAction.Buy, OrderType.Stop, 0, High[1] + PrevBarsPlusTicks * TickSize, TimeInForce.Day, orderIdL, "TF_long_3W_3BR", atmStrategyIdL);
				Draw.Rectangle(this, @"MyCustomStrategy Rectangle_1 " + Convert.ToString(CurrentBars[0]), false, 0, High[2], 2, Low[0], Brushes.CornflowerBlue, Brushes.CornflowerBlue, 0);
			}
			//if (!isAtmStrategyCreated)
			//	return;
			 // Set 2
			if (High[0] < Low[2] && isInSession(Times[0][0].TimeOfDay))
			{
				Draw.Rectangle(this, @"MyCustomStrategy Rectangle_1 " + Convert.ToString(CurrentBars[0]), false, 0, Low[2], 2, High[0], Brushes.Red, Brushes.CornflowerBlue, 0);
				if(orderId.Length > 0){
					AtmStrategyCancelEntryOrder(orderId);
				}
				
				
				if(State != State.Historical && side != MarketPosition.Short){
					if(atmStrategyId.Length>0){
						AtmStrategyClose(atmStrategyId);
					}
					Print("b");
					//isAtmStrategyCreated = false;
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.Sell, OrderType.Limit, High[0], 0, TimeInForce.Day, orderId, ATM, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
					//if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						//isAtmStrategyCreated = true;
					});
				}
				//Draw.Diamond(this, @"MyCustomStrategy Diamond_1 " + Convert.ToString(Close[0]), false, 0, Low[0], Brushes.Red);
			//	EnterShort(Convert.ToInt32(DefaultQuantity), @"short");
			}
			
		}
		
		private bool isInSession(TimeSpan t){
			bool b = false;
			b = (t >= ss1.TimeOfDay)&& (t < se1.TimeOfDay) && act1;
			b = b||(t >= ss2.TimeOfDay)&& (t < se2.TimeOfDay) && act2;
			b = b||(t >= ss3.TimeOfDay)&& (t < se3.TimeOfDay) && act3;
			b = b||(t >= ss4.TimeOfDay)&& (t < se4.TimeOfDay) && act4;
			b = b||(t >= ss5.TimeOfDay)&& (t < se5.TimeOfDay) && act5;
			b = b||(t >= ss6.TimeOfDay)&& (t < se6.TimeOfDay) && act6;
			b = b||(t >= ss7.TimeOfDay)&& (t < se7.TimeOfDay) && act7;
			b = b||(t >= ss8.TimeOfDay)&& (t < se8.TimeOfDay) && act8;
			b = b||(t >= ss9.TimeOfDay)&& (t < se9.TimeOfDay) && act9;
			b = b||(t >= ss10.TimeOfDay)&& (t < se10.TimeOfDay) && act10;
			b = b||(t >= ss11.TimeOfDay)&& (t < se11.TimeOfDay) && act11;
			b = b||(t >= ss12.TimeOfDay)&& (t < se12.TimeOfDay) && act12;
			b = b||(t >= ss13.TimeOfDay)&& (t < se13.TimeOfDay) && act13;
			b = b||(t >= ss14.TimeOfDay)&& (t < se14.TimeOfDay) && act14;
			b = b||(t >= ss15.TimeOfDay)&& (t < se15.TimeOfDay) && act15;
			b = b||(t >= ss16.TimeOfDay)&& (t < se16.TimeOfDay) && act16;
			return b;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ATM", Order=4, GroupName="Parameters")]
		public string ATM
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-10, 10)]
		[Display(Name="GMT",  GroupName="TimeZome")]
		public int GMT
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 01")]
		public DateTime ss1
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 01")]
		public DateTime se1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 01")]
		public bool act1
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 02")]
		public DateTime ss2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 02")]
		public DateTime se2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 02")]
		public bool act2
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 03")]
		public DateTime ss3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 03")]
		public DateTime se3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 03")]
		public bool act3
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 04")]
		public DateTime ss4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 04")]
		public DateTime se4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 04")]
		public bool act4
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 05")]
		public DateTime ss5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 05")]
		public DateTime se5
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 05")]
		public bool act5
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 06")]
		public DateTime ss6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 06")]
		public DateTime se6
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 06")]
		public bool act6
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 07")]
		public DateTime ss7
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 07")]
		public DateTime se7
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 07")]
		public bool act7
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 08")]
		public DateTime ss8
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 08")]
		public DateTime se8
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 08")]
		public bool act8
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 09")]
		public DateTime ss9
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 09")]
		public DateTime se9
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 09")]
		public bool act9
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 10")]
		public DateTime ss10
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 10")]
		public DateTime se10
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 10")]
		public bool act10
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 11")]
		public DateTime ss11
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 11")]
		public DateTime se11
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 11")]
		public bool act11
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 12")]
		public DateTime ss12
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 12")]
		public DateTime se12
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 12")]
		public bool act12
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 13")]
		public DateTime ss13
		{ get; set; }

		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 13")]
		public DateTime se13
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 13")]
		public bool act13
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 14")]
		public DateTime ss14
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 14")]
		public DateTime se14
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 14")]
		public bool act14
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 15")]
		public DateTime ss15
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 15")]
		public DateTime se15
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 15")]
		public bool act15
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="start",  GroupName="Session 16")]
		public DateTime ss16
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="end",  GroupName="Session 16")]
		public DateTime se16
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Active", Order=3, GroupName="Session 16")]
		public bool act16
		{ get; set; }
		
		#endregion

	}
}
