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
namespace NinjaTrader.NinjaScript.Strategies.TradeSimpleStrategies
{
	public class ReversalStrat : Strategy
	{
		#region Reversal Pattern
		
		private double percentageCalc;
		private double priceCalc;
		private double tickCalc;
		private double candleBarOffset;
		
		private bool currentBullRev;
		private bool currentBearRev;
		
		#endregion
		
		#region entry Offset
		
		private double entryAreaLong;
		private double entryAreaShort;
		
		private double percentageCalcEntry;
		private double priceCalcEntry;
		private double tickCalcEntry;
		private double candleBarOffsetEntry;
				
		private double enterLong;
		private double enterShort;
		
		#endregion
		
		#region stop Offset
		
		private double stopAreaLong;
		private double stopAreaShort;
		
		private double percentageCalcStop;
		private double priceCalcStop;
		private double tickCalcStop;
		private double candleBarOffsetStop;
				
		private double stopLong;
		private double stopShort;
		
		#endregion
		
		#region Breakeven 
		
		private double breakevenTriggerLong;
		private double breakevenTriggerShort;
		
		private bool myFreeBELong;
		private bool myFreeBEShort;
		
		private double breakevenLong;
		private double breakevenShort;
		
		#endregion
		
		#region Trail Stop
		
		private double trailAreaLong;
		private double trailAreaShort;
		
		private double percentageCalcTrail;
		private double priceCalcTrail;
		private double tickCalcTrail;
		private double candleBarOffsetTrail;
		
		private double trailLong;
		private double trailShort;
		
		private double trailTriggerLong;
		private double trailTriggerShort;
		
		private bool myFreeTrail;
		
		
		private bool trailTriggeredCandle;
		#endregion
		
		#region Trade Limits
		
		private bool countOnce;
		private int currentCount;
		
		private double totalPnL;
		
		private double cumPnL;
		private double dailyPnL;
		
		#endregion
		
		private bool myFreeTradeLong;
		private bool myFreeTradeShort;
		
		#region TradeSimple Social
		
		private string author 								= "TradeSimple Dre";
		private string version 								= "Version 1.2 // November 2022";
		
		private string youtube								= "https://youtu.be/egqUore32fs"; // Part 1: https://youtu.be/ccysupvsd1U, Part 2: https://youtu.be/egqUore32fs
		private string discord								= "https://discord.gg/2YU9GDme8j";
		
		private bool showSocials;
		
		private bool youtubeButtonClicked;
		private bool discordButtonClicked;
		
		private System.Windows.Controls.Button youtubeButton;
		private System.Windows.Controls.Button discordButton;
		
		
		private System.Windows.Controls.Grid myGrid29;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Real Time Only strategy that uses reversal pattern";
				Name										= "ReversalStrat";
				Calculate									= Calculate.OnPriceChange;
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
				BarsRequiredToTrade							= 1;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
			#region Default Settings
				
				//Reversal Candle Offset
				PriceOffset									= 0.01;
				PercentageOffset							= 0;
				TickOffset									= 1;
				
				//Entry Offset
				PriceOffsetEntry							= 0.01;
				PercentageOffsetEntry						= 0;
				TickOffsetEntry								= 1;
				
				// Stop Offset
				PriceOffsetStop								= 0.01;
				PercentageOffsetStop						= 0;
				TickOffsetStop								= 1;
				
				//Trail Offset
				PriceOffsetTrail							= 0.01;
				PercentageOffsetTrail						= 0;
				TickOffsetTrail								= 1;
				TrailTriggerAmount							= 20;
				
				//Breakeven
				BreakevenTriggerAmount						= 10;
				
				//Daily Limits
				DailyProfitLimit							= 1000;
				DailyLossLimit								= 500;
				X											= 5;
				Start										= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				End											= DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				
				//Position Size
				PositionSize								= 2;
				ProfitTargetTicks							= 50;
				
				//User Option to set the following
				StopLoss									= true;
				ProfitTarget								= true;
				SetBreakeven								= true;
				SetTrail									= true;
				
				//Will only Count once ber bar
				countOnce									= true;
				
				//Prints to Output
				SystemPrint									= true;
				ReversalBarPrints							= false;
				EntryPrints									= false;
				StopPrints									= false;
				CurrentCountPrints							= false;
				DailyPnlPrints								= false; //ADD AFTER!!!
				BreakevenPrints								= false; //ADD AFTER!!!
				TrailPrints									= false;
				
				
				//Shows Social Media Buttons
				showSocials									= true;
				
				TakeLongs									= true;
				TakeShorts									= true;
				
			#endregion
				
			}
			else if (State == State.Configure)
			{
			}
			
			//The next part below 'Add Buttons with Links' is how I add the links at the bottom of the chart. Details are not covered in this guide. 
			
			//Not part of the Strategy.
			#region Add Buttons with Links
			
		else if (State == State.Historical)
		{
			if (showSocials)
			{
				if (UserControlCollection.Contains(myGrid29))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid29 = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom
					};
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid29.ColumnDefinitions.Add(column1);
					myGrid29.ColumnDefinitions.Add(column2);
					
					youtubeButton = new System.Windows.Controls.Button
					{
						Name = "YoutubeButton", Content = "Youtube", Foreground = Brushes.White, Background = Brushes.Red
					};
					
					discordButton = new System.Windows.Controls.Button
					{
						Name = "DiscordButton", Content = "Discord", Foreground = Brushes.White, Background = Brushes.RoyalBlue
					};
					
					youtubeButton.Click += OnButtonClick;
					discordButton.Click += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(youtubeButton, 0);
					System.Windows.Controls.Grid.SetColumn(discordButton, 1);
					
					myGrid29.Children.Add(youtubeButton);
					myGrid29.Children.Add(discordButton);
					
					UserControlCollection.Add(myGrid29);
				}));
			}
			
			
			else if (State == State.Terminated)
			{
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid29 != null)
					{
						if (youtubeButton != null)
						{
							myGrid29.Children.Remove(youtubeButton);
							youtubeButton.Click -= OnButtonClick;
							youtubeButton = null;
						}
						
						if (discordButton != null)
						{
							myGrid29.Children.Remove(discordButton);
							discordButton.Click -= OnButtonClick;
							discordButton = null;
						}
								
					}
				}));
			}
		}
		#endregion 
			//Not part of the strategy.
		
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow(); //Clears Output window every time strategy is enabled
			}
		}

		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			#region CurrentCount
			
			if (Position.Quantity == PositionSize)
			{
				currentCount++; //Adds +1 to your currentCount every time a position is filled
				
				if (SystemPrint)
				{
					if (CurrentCountPrints)
					{
						Print("currentCount " + currentCount + " " + Time[1]);
					}
				}
			}

			#endregion
			
			#region Trail Stop Cancel
			
			if (Position.MarketPosition == MarketPosition.Flat) 
			{
				myFreeTrail = false; //Once the position is exited, cancel calculations for trail stop <<Typo here
				
				if (SystemPrint)
				{
					if (TrailPrints)
					{
						Print("myFree Trail ON Position " + myFreeTrail + " " + Time[1]);
					}
				}
				
			}
			
			#endregion
			
			#region Daily PNL
			
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
			{
				totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; ///Double that sets the total PnL 
				
				dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these

				
				if (SystemPrint)
				{
					if (DailyPnlPrints)
					{
						Print("Current PNL Realized >> " + totalPnL + " << " + Time[0]);
						
						Print("cumPnL >> " + cumPnL + " << " + Time[0]);
						Print("dailyPnL >> " + dailyPnL + " << " + Time[0]);	
					}
				}
				
				
				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
				{
					if (SystemPrint)
					{
						if (DailyPnlPrints)
						{
							Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
						}
					}
					
				}
				
				
				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
				{
					if (SystemPrint)
					{
						if (DailyPnlPrints)
						{
							Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
						}
					}	
				}
				
			}	
			#endregion
		}
		

		protected override void OnBarUpdate()
		{
			#region if Return
		
			
			if (State != State.Realtime) //Only trades realtime. Ignores historical trades.
			{
				return;
			}
			
			
			if (CurrentBars[0] < 2) //Need more than 2 bars to trade
			{
				return;
			}
			
			
			if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
			{
				currentCount 	= 0; ///Resets amount of trades you can take in a day
				
				
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
				
				
				if (SystemPrint)
				{
					if (CurrentCountPrints)
					{
						Print("currentCount First Bar " + currentCount + " " + Time[0]);
					}
					
					if (DailyPnlPrints)
					{
						Print("totalPnL First Bar " + totalPnL + " " + Time[0]);
						Print("cumPnL First Bar " + cumPnL + " " + Time[0]);
							
						Print("dailyPnL First Bar " + dailyPnL + " " + Time[0]);
					}
				}
				
			}
			

			if (Bars.BarsSinceNewTradingDay < 1 ) //Needs more than 1 bar on new day to begin trading. (Prevents trades if previous day closed as a pattern for our entry)
			{
				return;
			}
			
			#endregion
			
			if (countOnce)
			{
				
					#region Reversal Offset
			
			//Adds offset to allow user some customization of how a reversal candle is defined		
			percentageCalc 	= ((High[2] - Low[2]) * PercentageOffset);
			priceCalc 		= PriceOffset;
			tickCalc		= TickOffset * TickSize; ///Error in my code -> Switch TickOffsetEntry to TickOffset*
			
			//Will pick the highest of the 3 numbers	
			candleBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));
			
				#region Pattern Prints
				
			if (SystemPrint)
			{
				if (ReversalBarPrints)
				{
					Print("percentageCalc " + percentageCalc + " " + Time[1]);
					Print("priceCalc " + priceCalc + " " + Time[1]);
					Print("tickCalc " + tickCalc + " " + Time[1]);
					
					Print("candleBarOffset " + candleBarOffset + " " + Time[1]);
				}
				
			}
			
			#endregion
			
			#endregion
		
					#region Reversal Pattern Logic
			
			//Define Bullish Reversal Pattern 
			if (
				((Low[1] + (candleBarOffset)) <= Low[2]) 
					&& (Close[1] >= Close[2])
						&& (Open[1] < Close[1])
							&& (Open[2] > Close[2])
				)
							
			{
				currentBullRev = true;
			}
			else
			{
				currentBullRev = false;	
			}
			
			//Define Bearish Reversal PAttern
			if (
				((High[1] - (candleBarOffset)) >= High[2]) 
					&& (Close[1] <= Close[2])
						&& (Open[1] > Close[1])
							&& (Open[2] < Close[2])
				)
							
			{
				currentBearRev = true;
			}
			else
			{
				currentBearRev = false;	
			}
			
			#region Reversal Bool Prints
			
			if (SystemPrint)
			{
				if (ReversalBarPrints)
				{
					Print("currentBullRev " + currentBullRev + " " + Time[1]);
					Print("currentBearRev " + currentBearRev + " " + Time[1]);
				}
			}
			
			#endregion
			
			#endregion
			
				
				if ((Position.MarketPosition == MarketPosition.Flat) && (currentBullRev || currentBearRev))
				{	
					#region Entry Offset
			
			//Define what area you are entering (If it is based on the chart)
			entryAreaLong		= High[1];
			entryAreaShort		= Low[1];
			
			//Adds offset to your entry area. Gives user customization.
			percentageCalcEntry 	= ((High[1] - Low[1]) * PercentageOffsetEntry);
			priceCalcEntry 			= PriceOffsetEntry;
			tickCalcEntry			= TickOffsetEntry * TickSize;
			
			//Picks the highest of the 3 numbers
			candleBarOffsetEntry = Math.Max(percentageCalcEntry, Math.Max(priceCalcEntry, tickCalcEntry));
			
			//Add both of them together to define final entry point
			enterLong = entryAreaLong + candleBarOffsetEntry;
			enterShort = entryAreaShort - candleBarOffsetEntry;
			
			#region Entry Prints
			
			if (SystemPrint)
			{
				if (EntryPrints)
				{
					Print("percentageCalcEntry " + percentageCalcEntry + " " + Time[1]);
					Print("priceCalcEntry " + priceCalcEntry + " " + Time[1]);
					Print("tickCalcEntry " + tickCalcEntry + " " + Time[1]);
					
					Print("candleBarOffsetEntry" + candleBarOffsetEntry + " " + Time[1]);
					
					Print("enterLong" + enterLong + " " + Time[1]);
					Print("enterShort" + enterShort + " " + Time[1]);
				}
			}
			#endregion
			
			#endregion
			
					#region Stop Offset
			
			//Define what area you will set a stop (If it is based on the chart)
			stopAreaLong		= Low[1];
			stopAreaShort		= High[1];
			
			//Adds offset to your stop area. Gives user customization.
			percentageCalcStop 	= ((High[1] - Low[1]) * PercentageOffsetStop);
			priceCalcStop 			= PriceOffsetStop;
			tickCalcStop			= TickOffsetStop * TickSize;
			
			//Picks the highest of the 3 numbers
			candleBarOffsetStop = Math.Max(percentageCalcStop, Math.Max(priceCalcStop, tickCalcStop));
			
			//Add both of them together to define final stop point
			stopLong = stopAreaLong - candleBarOffsetStop;
			stopShort = stopAreaShort + candleBarOffsetStop;
			
			#region StopPrints
			
			if (SystemPrint)
			{
				if (StopPrints)
				{
					Print("percentageCalcStop " + percentageCalcStop + " " + Time[1]);
					Print("priceCalcStop " + priceCalcStop + " " + Time[1]);
					Print("tickCalcStop " + tickCalcStop + " " + Time[1]);
				
					Print("candleBarOffsetStop" + candleBarOffsetStop + " " + Time[1]);
					
					Print("stopLong" + stopLong + " " + Time[1]);
					Print("stopShort" + stopShort + " " + Time[1]);
				}
			}
			
			#endregion
			
			#endregion
				}
		
				countOnce = false; //Sets bool countOnce back to false. Needs to wait until new bar to make calculations again.
				
			}
			
			
			if (IsFirstTickOfBar)
			{
				countOnce = true; //Sets bool countOnce back to true. Allows Calculations to be made again.
				
				#region Trail Stop new bar Update
				
				//If the strategy is in a position. This allows the trail stop to keep making calculations and continue moving every new candle
				if (Position.MarketPosition != MarketPosition.Flat)
				{
					myFreeTrail = true;
					
					
					if (SystemPrint)
					{
						if (TrailPrints)
						{
							Print("myFree Trail First Tick " + myFreeTrail + " " + Time[1]);
						}
						
					}
					
				}

				#endregion
			}
			
			if (TakeLongs) ///These have been added as a request - Will explain in Pt.3 Video
			{
				#region Long Trade
			
			if (
				(currentBullRev) //Bullish Pattern
				&& (Close[0] >= enterLong) //Current Price hits our Entry Point
					&& (Position.MarketPosition == MarketPosition.Flat) //No other positions have been open on instrument
						&& ((Time[1].TimeOfDay >= Start.TimeOfDay) && (Time[1].TimeOfDay <= End.TimeOfDay)) //Trade is taking place within specified time
							&& (currentCount < X) //Has less than the amount of max trades specified
				
								&& (dailyPnL > -DailyLossLimit) //Loss remains 'above' limit
								&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
				
				///You will be able to enter 'short' after Profit/Stop from a long position. This just prevents entering right away in the same direction
				&& (BarsSinceExitExecution("MyStopLong") > 1 || BarsSinceExitExecution("MyStopLong") == -1) //Needs 1 candle to enter Long again after being stopped from a long position.
				&& (BarsSinceExitExecution("MyTargetLong") > 1 || BarsSinceExitExecution("MyTargetLong") == -1) //Needs 1 candle to enter Long again after hitting profit target from a long position.
				)
			
			{
				EnterLong(PositionSize, "MyEntryLong"); //Enters Long with specified Position Size
								
				myFreeTradeLong = true; //This will allow Stop and Profit Targets to move around freely once bool is set to false
			}
			
			#region Long Stop and Profit
			
			if (Position.MarketPosition == MarketPosition.Long && myFreeTradeLong == true)
			{
				if (StopLoss)
				{
					ExitLongStopMarket(0, true, Position.Quantity, stopLong, "MyStopLong", "MyEntryLong"); //Sets Stop for position quantity at specified point
				}
								
				if (ProfitTarget)
				{
					ExitLongLimit(0, true, Position.Quantity, Position.AveragePrice + (TickSize * ProfitTargetTicks),  "MyTargetLong", "MyEntryLong"); //Sets Profit at (Ticks) above entry fill price
				}
				
				breakevenTriggerLong = Position.AveragePrice + (TickSize * BreakevenTriggerAmount); //Define the target in which your Breakeven will trigger. (Fill Price + Ticks)
				myFreeBELong = true; //Activates Breakeven. You will be a ble to move the stop around after.
				
				trailTriggerLong = Position.AveragePrice + (TickSize * TrailTriggerAmount); //Define the target in which your trail stop will trigger. (Fill Price + Ticks)
				myFreeTrail = true; //Activates Trail Stop. You will be a ble to move the stop around after it is set.
				
				
				myFreeTradeLong = false; //Sets bool back to false after Profit and Stop are set. This allows them to be moved around freely. 
			}
			
			#endregion
			
			#endregion
			}
			
			if (TakeShorts)
			{
				#region Short Trade
					
			if (
				(currentBearRev) //Bearish Pattern
					&& (Close[0] <= enterShort) //Current Price hits our Entry Point
						&& (Position.MarketPosition == MarketPosition.Flat) //No other positions have been open on instrument
							&& ((Time[1].TimeOfDay >= Start.TimeOfDay) && (Time[1].TimeOfDay <= End.TimeOfDay)) //Trade is taking place within specified time
								&& (currentCount < X) //Has less than the amount of max trades specified
				
									&& (dailyPnL > -DailyLossLimit) //Loss remains 'above' limit
									&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
				
				///You will be able to enter 'long' after Profit/Stop from a short position. This just prevents entering right away in the same direction
				&& (BarsSinceExitExecution("MyStopShort") > 1 || BarsSinceExitExecution("MyStopShort") == -1) //Needs 1 candle to enter Short again after being stopped from a short position.
				&& (BarsSinceExitExecution("MyTargetShort") > 1 || BarsSinceExitExecution("MyTargetShort") == -1) //Needs 1 candle to enter Short again after hitting profit target from a shortposition.
				)
			{
				EnterShort(PositionSize, "MyEntryShort"); //Enters Long with specified Position Size
				
				myFreeTradeShort = true; //This will allow Stop and Profit Targets to move around freely once bool is set to false
			}
			
			#region Short Stop and Profit
			
			if (Position.MarketPosition == MarketPosition.Short && myFreeTradeShort == true) 
			{
				if (StopLoss)
				{
					ExitShortStopMarket(0, true, Position.Quantity, stopShort, "MyStopShort", "MyEntryShort"); //Sets Stop for position quantity at specified point
				}
				
				if (ProfitTarget)
				{
					ExitShortLimit(0, true, Position.Quantity, Position.AveragePrice - (TickSize * ProfitTargetTicks),  "MyTargetShort", "MyEntryShort"); //Sets Profit at (Ticks) below entry fill price
				}
				
				breakevenTriggerShort = Position.AveragePrice - (TickSize * BreakevenTriggerAmount); //Define the target in which your Breakeven will trigger. (Fill Price - Ticks)
				myFreeBEShort = true; //Activates Breakeven. You will be a ble to move the stop around after it is set.
				
				
				trailTriggerShort = Position.AveragePrice - (TickSize * TrailTriggerAmount); //Define the target in which your trail stop will trigger. (Fill Price - Ticks)
				myFreeTrail = true; //Activates Trail Stop. You will be a ble to move the stop around after it is set.
				
				
				myFreeTradeShort = false; //Sets bool back to false after Profit and Stop are set. This allows them to be moved around freely. 
					
			}
			
			#endregion
			
		#endregion
			}
			
		}
		
		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
				
				#region Long Breakeven
				
				if (SetBreakeven)
				{
					 if(
						 (Position.MarketPosition == MarketPosition.Long) //Has to be in a long position
						 && (marketDataUpdate.Price >= breakevenTriggerLong) //Price hits our breakeven trigger
						 && (myFreeBELong == true) //BE bool is true -> Once its false, you can move your stop freely
						 )
					 {
						 breakevenLong = Position.AveragePrice; //Define what a breakeven is. (We set it at the average filled price)
						 ExitLongStopMarket(0, true, Position.Quantity, breakevenLong, "MyStopLong", "MyEntryLong"); //Sets stop at area defined.
						 myFreeBELong = false; //BE bool is false after setting stop. You can now move it around freely.
					 }
				}
				
				#endregion
				
				#region Short Breakeven
					
					if (SetBreakeven)
					{
						 if(
							(Position.MarketPosition == MarketPosition.Short) //Has to be in a short position
								&& (marketDataUpdate.Price <= breakevenTriggerShort) //Price hits our breakeven trigger
									&& (myFreeBEShort == true) //BE bool is true -> Once its false, you can move your stop freely
							)
						 {
						 	breakevenShort = Position.AveragePrice; //Define what a breakeven is. (We set it at the average filled price)
							ExitShortStopMarket(0, true, Position.Quantity, breakevenShort, "MyStopShort", "MyEntryShort"); //Sets stop at area defined.
							 myFreeBEShort = false; //BE bool is false after setting stop. You can now move it around freely.
						 }
					}
					
					#endregion 
					
				
				#region Trail Stop
					
				if (SetTrail) 
				{
					if (
						(myFreeTrail == true) //Bool needs to be true (Gets Reset with IsFirstTickOfBar to allow trail to occur on each new candle)
						
						//Has to be either short and past short Trail Trigger OR long and past long trail trigger
						&& (((Position.MarketPosition == MarketPosition.Short) && (marketDataUpdate.Price <= trailTriggerShort)) 
						|| ((Position.MarketPosition == MarketPosition.Long) && (marketDataUpdate.Price >= trailTriggerLong)))
						)
					{
					
				#region Trail Offset
			
			//Define the area where stop will be set			
			trailAreaLong		= Low[1];
			trailAreaShort		= High[1];
			
			//Adds offset to your trail stop area. Gives user customization.		
			percentageCalcTrail 		= ((High[2] - Low[2]) * PercentageOffsetTrail);
			priceCalcTrail 			= PriceOffsetTrail;
			tickCalcTrail			= TickOffsetTrail * TickSize;
			
			//Picks the highest of the 3 numbers			
			candleBarOffsetTrail = Math.Max(percentageCalcTrail, Math.Max(priceCalcTrail, tickCalcTrail));
			
			//Add both of them together to define final entry point			
			trailLong = trailAreaLong - candleBarOffsetTrail;
			trailShort = trailAreaShort + candleBarOffsetTrail;
			
			#region Prints
			
			if (SystemPrint)
			{
				if (TrailPrints)
				{
					Print("Current Trail Price Offset is :  " + priceCalcTrail + " " + Time[1]);
					Print("Current Trail Percent Offset is :  " + percentageCalcTrail + " " + Time[1]);
					Print("Current Trail Tick Offset is :  " + tickCalcTrail + " " + Time[1]);
					
					Print("Current Trail Highest Offset Selected is :  " + candleBarOffsetTrail + " " + Time[1]);
					
					Print("Trail Long Price is :  " + trailLong + " " + Time[1]);	
					Print("Trail Short Price is :  " + trailShort + " " + Time[1]);	
				
					Print("myFree Trail Offset " + myFreeTrail + " " + Time[1]);
				}
				
			}
			#endregion
			
			
			#endregion 	
					
					trailTriggeredCandle = true; //Allows condition to move stop freely. 
					myFreeTrail = false; //Sets bool back to false. Needs to wait another candle for calculations to happen again	
			
			
					if (SystemPrint)
					{
						if (TrailPrints)
						{
							Print("myFree Trail Offset After " + myFreeTrail + " " + Time[1]);
						}
					}
		
			
					}
					
					#region Long Trail Stop
							
						if (
						(Position.MarketPosition == MarketPosition.Long) //Needs to be in a long position
				  			&& (trailTriggeredCandle) //Bool is true
								&& (Low[1] > Low[2]) //Ensure the trail will only move up if the new candles low is higher.	
							)
						{
							ExitLongStopMarket(0, true, Position.Quantity, trailLong, "MyStopLong", "MyEntryLong"); //Sets Stop
							
							trailTriggeredCandle = false; //You can move around trail stop freely until new candle.
						}
						
		
					#endregion 
						
					#region ShortTrail Stop
					
					
							
							if (
								(Position.MarketPosition == MarketPosition.Short) //Needs to be in a short position
									&& (trailTriggeredCandle) //Bool is true
										&& (High[1] < High[2]) //Ensure the trail will only move down if the new candles high is lower.
								)
							{
							ExitShortStopMarket(0, true, Position.Quantity, trailShort, "MyStopShort", "MyEntryShort"); //Sets Stop
							
							trailTriggeredCandle = false; //You can move around trail stop freely until new candle.
						
							}
							#endregion
				}
					
				#endregion
			}
		}

		
		//The next part below 'Button Click Event' is part of the buttons with links. Not part of this strategy 
			
		//Not part of the Strategy.
		#region Button Click Event
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			if (showSocials)
			{
				System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
				if (button == youtubeButton && button.Name == "YoutubeButton" && button.Content == "Youtube")
				{
					System.Diagnostics.Process.Start(youtube);
					return;
				}
				
				if (button == discordButton && button.Name == "DiscordButton" && button.Content == "Discord")
				{	
					System.Diagnostics.Process.Start(discord);
					return;
				}
			}
		}
	
	#endregion	
		//Not part of the Strategy.
		
		#region Properties
		
		#region 1. Reversal Candle Offset
		
		[NinjaScriptProperty]
		[Display(Name="Price Offset", Order=0, GroupName="1. Reversal Candle Offset")]
		public double PriceOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Percentage Offset", Order=1, GroupName="1. Reversal Candle Offset")]
		public double PercentageOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Tick Offset", Order=2, GroupName="1. Reversal Candle Offset")]
		public int TickOffset
		{ get; set; }
		
		#endregion
		
		#region 2. Entry Offset

		[NinjaScriptProperty]
		[Display(Name="Activate Longs", Order=0, GroupName="2. Entry Offset")]
		public bool TakeLongs
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Activate Shorts", Order=0, GroupName="2. Entry Offset")]
		public bool TakeShorts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Price Offset Entry", Order=0, GroupName="2. Entry Offset")]
		public double PriceOffsetEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Percentage Offset Entry", Order=1, GroupName="2. Entry Offset")]
		public double PercentageOffsetEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Tick Offset Entry", Order=2, GroupName="2. Entry Offset")]
		public int TickOffsetEntry
		{ get; set; }

		#endregion
		
		#region 3. Stop Offset
		
		[NinjaScriptProperty]
		[Display(Name="Set Stop Loss", Order=0, GroupName="3. Stop Offset")]
		public bool StopLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Price Offset Stop", Order=1, GroupName="3. Stop Offset")]
		public double PriceOffsetStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Percentage Offset Stop", Order=2, GroupName="3. Stop Offset")]
		public double PercentageOffsetStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Tick Offset Stop", Order=3, GroupName="3. Stop Offset")]
		public int TickOffsetStop
		{ get; set; }

		#endregion
		
		#region 4. Profit Target
		
		[NinjaScriptProperty]
		[Display(Name="Profit Target", Order=0, GroupName="4. Profit Target")]
		public bool ProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Position Size", Order=1, GroupName="4. Profit Target")]
		public int PositionSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Final Profit", Order=2, GroupName="4. Profit Target")]
		public int ProfitTargetTicks
		{ get; set; }
		
		#endregion
		
		#region 5. Breakeven
		
		[NinjaScriptProperty]
		[Display(Name="Set Breakeven", Order=1, GroupName="5. Breakeven")]
		public bool SetBreakeven
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Breakeven Trigger Amount", Order=2, GroupName="5. Breakeven")]
		public int BreakevenTriggerAmount
		{ get; set; }
		
		#endregion
		
		#region 6A. Trail Stop
		
		[NinjaScriptProperty]
		[Display(Name="Set Trail", Order=0, GroupName="6A. Trail Stop")]
		public bool SetTrail
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Trail TriggerAmount", Order=1, GroupName="6A. Trail Stop")]
		public int TrailTriggerAmount
		{ get; set; }
		
		#endregion
		
		#region 6B. Trail Stop
		
		[NinjaScriptProperty]
		[Display(Name="Price Offset Trail", Order=0, GroupName="6B. Trail Stop")]
		public double PriceOffsetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Percentage Offset Trail", Order=1, GroupName="6B. Trail Stop")]
		public double PercentageOffsetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Tick Offset Trail", Order=2, GroupName="6B. Trail Stop")]
		public int TickOffsetTrail
		{ get; set; }

		#endregion
		
		#region 7. Order Limits
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time", Order=0, GroupName="7. Order Limits")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time", Order=1, GroupName="7. Order Limits")]
		public DateTime End
		{ get; set; }
	
		
		[NinjaScriptProperty]
		[Display(Name="Daily Profit Limit", Order=2, GroupName="7. Order Limits")]
		public double DailyProfitLimit
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Daily Loss Limit", Order=3, GroupName="7. Order Limits")]
		public double DailyLossLimit
		{ get; set; }

	
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Max Trade Count", Description="How Many Trades System will Enter", Order=4, GroupName="7. Order Limits")]
		public int X
		{ get; set; }
	
		#endregion
		
		#region 8. Prints
		
		[NinjaScriptProperty]
		[Display(Name="SystemPrint", Order=1, GroupName="8. Prints")]
		public bool SystemPrint
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ReversalBarPrints", Order=2, GroupName="8. Prints")]
		public bool ReversalBarPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="EntryPrints", Order=3, GroupName="8. Prints")]
		public bool EntryPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="StopPrints", Order=4, GroupName="8. Prints")]
		public bool StopPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="CurrentCountPrints", Order=5, GroupName="8. Prints")]
		public bool CurrentCountPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="DailyPnlPrints", Order=6, GroupName="8. Prints")]
		public bool DailyPnlPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="BreakevenPrints", Order=7, GroupName="8. Prints")]
		public bool BreakevenPrints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="TrailPrints", Order=8, GroupName="8. Prints")]
		public bool TrailPrints
		{ get; set; }
		
		#endregion
		
		
		#region 9. TradeSimple Dre
		
		[NinjaScriptProperty]
		[Display(Name = "Show Social Media Buttons", Description = "", Order = 0, GroupName = "9. TradeSimple Dre")]
		public bool ShowSocials 
		{
		 	get{return showSocials;} 
			set{showSocials = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Explanation Video", Order=1, GroupName="9. TradeSimple Dre")]
		public  string Youtube
		{
		 	get{return youtube;} 
			set{youtube = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Discord Link", Order=2, GroupName="9. TradeSimple Dre")]
		public  string Discord
		{
		 	get{return discord;} 
			set{discord = (value);} 
		}
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name = "Author", GroupName = "9. TradeSimple Dre", Order = 3)]
		public string Author
		{
		 	get{return author;} 
			set{author = (value);} 
		}
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name = "Version", GroupName = "9. TradeSimple Dre", Order = 4)]
		public string Version
		{
		 	get{return version;} 
			set{version = (value);} 
		}
		
		
		#endregion
	
		#endregion

	}
}