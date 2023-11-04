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
    public class PeaceBringer : Strategy
    {
        private Series<double> UpRmaSeries;
        private RMA UpRma;
        private Series<double> DownRmaSeries;
        private RMA DownRma;

        private Series<double> UpRmaSeries2;
        private RMA Up2Rma;
        private Series<double> DownRmaSeries2;
        private RMA Down2Rma;

        private Series<double> MaRsiSeries;
        private SMA MaRsi;
        private Series<double> MaRsiSeries2;
        private SMA MaRsi2;

        private string atmStrategyId = string.Empty;
        private string orderId = string.Empty;
        private bool isAtmStrategyCreated = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Peace Bringer";
                Name = "PeaceBringer";
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

                EnableAtmMode = false;
                Profit_Target = 20;
                Stop_Loss = 10;
                AtmStrategyTemplateId = "your atm";

                RsiLength = 9;
                VRsiLength = 9;
                MarsiLength = 5;
                MaVrsiLength = 5;

                Time_1 = true;
                Start_Time_1 = DateTime.Parse("01:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_1 = DateTime.Parse("16:45", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (State == State.Configure)
            {
                ClearOutputWindow();

                UpRmaSeries = new Series<double>(this);
                UpRma = RMA(UpRmaSeries, RsiLength);
                DownRmaSeries = new Series<double>(this);
                DownRma = RMA(DownRmaSeries, RsiLength);

                UpRmaSeries2 = new Series<double>(this);
                Up2Rma = RMA(UpRmaSeries2, VRsiLength);
                DownRmaSeries2 = new Series<double>(this);
                Down2Rma = RMA(DownRmaSeries2, VRsiLength);

                MaRsiSeries = new Series<double>(this);
                MaRsi = SMA(MaRsiSeries, MarsiLength);
                MaRsiSeries2 = new Series<double>(this);
                MaRsi2 = SMA(MaRsiSeries2, MaVrsiLength);

                AddPlot(Brushes.Red, "RMA1");
            }
            else if (State == State.DataLoaded)
            {
                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade || CurrentBar < RsiLength)
                return;

            if (BarsInProgress != 0 || CurrentBars[0] < 1)
                return;

            List<TimePeriod> timePeriods = new List<TimePeriod>
            {
               new TimePeriod(Time_1, Start_Time_1, Stop_Time_1),
            };

            foreach (var timePeriod in timePeriods)
            {
                if (timePeriod.isTimeConditionMet(Time[0]))
                {
                    //                    up = ta.rma(math.max(ta.change(src), 0), len)
                    UpRmaSeries[0] = Math.Max(Close[0] - Close[1], 0);
                    double upRmaValue = UpRma[0];

                    //down = ta.rma(-math.min(ta.change(src), 0), len)
                    DownRmaSeries[0] = -Math.Min(Close[0] - Close[1], 0);
                    double downRmaValue = DownRma[0];

                    //up2 = ta.rma(math.max(ta.change(src2), 0), len2)
                    UpRmaSeries2[0] = Math.Max(Volume[0] - Volume[1], 0);
                    double up2RmaValue = Up2Rma[0];

                    //down2 = ta.rma(-math.min(ta.change(src2), 0), len2)
                    DownRmaSeries2[0] = -Math.Min(Volume[0] - Volume[1], 0);
                    double down2RmaValue = Down2Rma[0];

                    // rsi = down == 0 ? 100 : up == 0 ? 0 : 100 - 100 / (1 + up / down)
                    MaRsiSeries[0] = downRmaValue == 0 ? 100 : upRmaValue == 0 ? 0 : 100 - 100 / (1 + upRmaValue / downRmaValue);
                    //rsi2 = down2 == 0 ? 100 : up2 == 0 ? 0 : 100 - 100 / (1 + up2 / down2)
                    MaRsiSeries2[0] = down2RmaValue == 0 ? 100 : up2RmaValue == 0 ? 0 : 100 - 100 / (1 + up2RmaValue / down2RmaValue);

                    //// MA(=Moving Average) of RSI(close, ="MARSI") and RSI(Volume, ="MAVRSI")
                    //len3 = input.int(5, minval=1, title='Length MARSI')
                    //marsi = ta.sma(rsi, len3)
                    double marsiValue = MaRsi[0];
                    //len4 = input.int(5, minval=1, title='Length MAVRSI')
                    //marsi2 = ta.sma(rsi2, len4)
                    double marsiValue2 = MaRsi2[0];

                    /////Long Entry///
                    //longCondition = marsi > marsi[1]
                    //if longCondition and timeIsAllowed
                    //    strategy.entry('Long', strategy.long)

                    bool longCondition = MaRsi[0] > MaRsi[1];

                    // Submits an entry limit order at the current low price to initiate an ATM Strategy if both order id and strategy id are in a reset state
                    // **** YOU MUST HAVE AN ATM STRATEGY TEMPLATE NAMED 'AtmStrategyTemplate' CREATED IN NINJATRADER (SUPERDOM FOR EXAMPLE) FOR THIS TO WORK ****
                    if (EnableAtmMode && State == State.Realtime && orderId.Length == 0 && atmStrategyId.Length == 0)
                    {
                        isAtmStrategyCreated = false;  // reset atm strategy created check to false
                        atmStrategyId = GetAtmStrategyUniqueId();
                        orderId = GetAtmStrategyUniqueId();

                        OrderAction orderAction = longCondition ? OrderAction.Buy : OrderAction.Sell;
                        double entryPrice = longCondition ? Low[0] : High[0];
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
                            if (longCondition)
                                EnterLong("");
                            else
                                EnterShort("");
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
        [Range(1, int.MaxValue)]
        [Display(Name = "Rsi Length", Order = 1, GroupName = "Parameters")]
        public int RsiLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Vol Rsi Length", Order = 2, GroupName = "Parameters")]
        public int VRsiLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MaRsi Length", Order = 3, GroupName = "Parameters")]
        public int MarsiLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MaVRsi Length", Order = 4, GroupName = "Parameters")]
        public int MaVrsiLength
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

        #endregion

    }
}
