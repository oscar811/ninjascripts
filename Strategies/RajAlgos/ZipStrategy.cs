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
using NinjaTrader.NinjaScript.Indicators.RajIndicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class ZipStrategy : Strategy
    {
        private SMA shortSMA, mediumSMA, longSMA;
        private RSI rsi;
        private ADX adx;
        private ATR atr;

        private Series<bool> RsiBuyConditionSeries;
        private Series<bool> RsiSellConditionSeries;

        private string atmStrategyId = string.Empty;
        private string orderId = string.Empty;
        private bool isAtmStrategyCreated = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"A trend following strategy";
                Name = "ZipStrategy";
                Calculate = Calculate.OnEachTick;
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

                EnableSMAFilter = true;
                ShortSMALength = 7;
                MediumSMALength = 14;
                LongSMALength = 21;

                EnableRSIFilter = false;
                RsiLength = 14;
                RsiOverbought = 70;
                RsiOversold = 30;

                EnableADXFilter = true;
                AdxThreshold = 15;

                Time_1 = true;
                Start_Time_1 = DateTime.Parse("8:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_1 = DateTime.Parse("16:45", System.Globalization.CultureInfo.InvariantCulture);

                EnableAtmMode = false;
                AtmStrategyTemplateId = "AtmStrategyTemplate";
                Profit_Target = 800;
                Stop_Loss = 400;

                AtrLength = 26;
                AtrTpMultiplier = 4;
                AtrSlMultiplier = 2;
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                shortSMA = SMA(Close, ShortSMALength);
                mediumSMA = SMA(Close, MediumSMALength);
                longSMA = SMA(Close, LongSMALength);
                rsi = RSI(Close, RsiLength, 1);
                adx = ADX(Close, AdxThreshold);

                atr = ATR(AtrLength);

                RsiBuyConditionSeries = new Series<bool>(this);
                RsiSellConditionSeries = new Series<bool>(this);

                AddPlot(Brushes.Red, "ShortSMA");
                AddPlot(Brushes.Green, "MediumSMA");
                AddPlot(Brushes.Yellow, "LongSMA");
            }
            else if (State == State.DataLoaded)
            {
                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade || CurrentBar < longSMA || CurrentBar < RsiLength)
                    return;

                if (BarsInProgress != 0)
                    return;

                List<TimePeriod> timePeriods = new List<TimePeriod>
                {
                    new TimePeriod(Time_1, Start_Time_1, Stop_Time_1),
                };

                foreach (var timePeriod in timePeriods)
                {
                    if (timePeriod.isTimeConditionMet(Time[0]))
                    {
                        // rsiBuyCondition = false
                        // rsiSellCondition = false
                        // [_, _, adxValue] = ta.dmi(14, 14)
                        // adxCondtion = adxValue > adxFilter
                        bool adxCondtion = adx[0] > AdxThreshold;

                        bool smaBuyCondition = shortSMA > mediumSMA && mediumSMA > longSMA;
                        // rsiBuyCondition := (rsiBuyCondition[1] or rsiValue < rsiOversold) and rsiValue < rsiOverbought
                        RsiBuyConditionSeries[0] = (RsiBuyConditionSeries[1] || rsi[0] < RsiOversold) && rsi[0] < RsiOverbought;
                        // bool buyCondition = (not enableSmaFilter or smaBuyCondition) and (not enableRsiFilter or rsiBuyCondition) and (not enableAdxFilter or adxCondtion)
                        bool buyCondition = (!EnableSMAFilter || smaBuyCondition) && (!EnableRSIFilter || RsiBuyConditionSeries[0]) && (!EnableADXFilter || adxCondtion);

                        bool smaSellCondition = shortSMA < mediumSMA && mediumSMA < longSMA;
                        // rsiSellCondition := (rsiSellCondition[1] or rsiValue > rsiOverbought) and rsiValue > rsiOversold
                        RsiSellConditionSeries[0] = (RsiSellConditionSeries[1] || rsi[0] > RsiOverbought) && rsi[0] > RsiOversold;
                        // bool sellCondition = (not enableSmaFilter or smaSellCondition) and (not enableRsiFilter or rsiSellCondition) and (not enableAdxFilter or adxCondtion)
                        bool sellCondition = (!EnableSMAFilter || smaSellCondition) && (!EnableRSIFilter || RsiSellConditionSeries[0]) && (!EnableADXFilter || adxCondtion);

                        // Submits an entry limit order at the current low price to initiate an ATM Strategy if both order id and strategy id are in a reset state
                        // **** YOU MUST HAVE AN ATM STRATEGY TEMPLATE NAMED 'AtmStrategyTemplate' CREATED IN NINJATRADER (SUPERDOM FOR EXAMPLE) FOR THIS TO WORK ****
                        if (buyCondition || sellCondition)
                        {
                            if (EnableAtmMode && State == State.Realtime && orderId.Length == 0 && atmStrategyId.Length == 0)
                            {
                                isAtmStrategyCreated = false;  // reset atm strategy created check to false
                                atmStrategyId = GetAtmStrategyUniqueId();
                                orderId = GetAtmStrategyUniqueId();

                                OrderAction orderAction = buyCondition ? OrderAction.Buy : OrderAction.Sell;
                                double entryPrice = buyCondition ? Low[0] : High[0];
                                AtmStrategyCreate(orderAction, OrderType.Market, entryPrice, 0, TimeInForce.Day, orderId, AtmStrategyTemplateId, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) =>
                                {
                                    //check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
                                    if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
                                        isAtmStrategyCreated = true;
                                });
                            }
                            else
                            {
                                if (!EnableAtmMode)
                                    if (buyCondition)
                                        EnterLong("");
                                    if (sellCondition)
                                        EnterShort("");
                            }
                        }
                    }
                }

                // Check that atm strategy was created before checking other properties
                if (!isAtmStrategyCreated)
                    return;

                // Check for a pending entry order
                if (orderId.Length > 0)
                {
                    string[] status = GetAtmStrategyEntryOrderStatus(orderId);

                    // If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
                    if (status.GetLength(0) > 0)
                    {
                        // Print out some information about the order to the output window
                        Print("The entry order average fill price is: " + status[0]);
                        Print("The entry order filled amount is: " + status[1]);
                        Print("The entry order order state is: " + status[2]);

                        // If the order state is terminal, reset the order id value
                        if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
                            orderId = string.Empty;
                    }
                } // If the strategy has terminated reset the strategy id
                else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
                    atmStrategyId = string.Empty;

                if (atmStrategyId.Length > 0)
                {
                    // You can change the stop price
                    if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
                        AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", atmStrategyId);

                    // Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
                    // the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
                    Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyId));
                    Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
                    Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId));
                    Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

//        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice,
//                                      int quantity, int filled, double averageFillPrice,
//                                      OrderState orderState, DateTime time, ErrorCode error,
//                                      string nativeError)
//        {
//            if (orderState == OrderState.Filled)
//            {
//                // Force a recalculation of the strategy or indicator
//                ForceRefresh();
//            }
//        }

        private class TimePeriod
        {
            private bool useTimePeriod;
            private DateTime startTime;
            private DateTime stopTime;

            public TimePeriod(bool useTimePeriod, DateTime startTime, DateTime stopTime)
            {
                this.useTimePeriod = useTimePeriod;
                this.startTime = startTime;
                this.stopTime = stopTime;
            }

            public bool isTimeConditionMet(DateTime currentTime)
            {
                return useTimePeriod && (currentTime.TimeOfDay >= startTime.TimeOfDay && currentTime.TimeOfDay <= stopTime.TimeOfDay);
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Enable SMA Filter", Order = 1, GroupName = "SMA")]
        public bool EnableSMAFilter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Short SMA Length", Order = 2, GroupName = "SMA")]
        public int ShortSMALength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Medium SMA Length", Order = 3, GroupName = "SMA")]
        public int MediumSMALength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Long SMA Length", Order = 4, GroupName = "SMA")]
        public int LongSMALength
        { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "Enable RSI Filter", Order = 1, GroupName = "RSI")]
        public bool EnableRSIFilter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "RSI Length", Order = 2, GroupName = "RSI")]
        public int RsiLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Overbought", Order = 3, GroupName = "RSI")]
        public int RsiOverbought
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Oversold", Order = 4, GroupName = "RSI")]
        public int RsiOversold
        { get; set; }



        [NinjaScriptProperty]
        [Display(Name = "Enable ADX Filter", Order = 1, GroupName = "ADX")]
        public bool EnableADXFilter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Threshold", Order = 2, GroupName = "ADX")]
        public int AdxThreshold
        { get; set; }


        [NinjaScriptProperty]
        [Display(Name = "Time_1", Order = 1, GroupName = "Time")]
        public bool Time_1
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_1", Description = "Start Time", Order = 2, GroupName = "Time")]
        public DateTime Start_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_1", Description = "End Time", Order = 3, GroupName = "Time")]
        public DateTime Stop_Time_1
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Atm Strategy", Order = 10, GroupName = "Atm")]
        public bool EnableAtmMode
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ATM Strategy", Description = "Turn on or off", Order = 11, GroupName = "Atm")]
        public string AtmStrategyTemplateId
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Profit_Target", Order = 12, GroupName = "Atm")]
        public int Profit_Target
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop_Loss", Order = 13, GroupName = "Atm")]
        public int Stop_Loss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable ATR TP/SL", Order = 14, GroupName = "Atm")]
        public bool EnableAtrTpSl
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AtrLength", Order = 14, GroupName = "Atm")]
        public int AtrLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AtrTP multiplier", Order = 15, GroupName = "Atm")]
        public int AtrTpMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AtrSL  multiplier", Order = 16, GroupName = "Atm")]
        public int AtrSlMultiplier
        { get; set; }

        #endregion

    }
}
