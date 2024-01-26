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

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class Time400am : Strategy
    {
        private SessionHighLow SessionHighLow1;
        private List<TradingCondition> tradingConditions;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Wicky AMD programmed to take specified high and lows of each session during AMD time periods";
                Name = "Time400am";
                Calculate = Calculate.OnPriceChange;
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
                IsInstantiatedOnEachOptimizationIteration = false;
                CONTRACTS = 1;
                Profit_Target = 100;
                Stop_Loss = 100;
                Time_2 = true;
                Start_Time_2 = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_2 = DateTime.Parse("06:28", System.Globalization.CultureInfo.InvariantCulture);
                Time_3 = true;
                Start_Time_3 = DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_3 = DateTime.Parse("09:28", System.Globalization.CultureInfo.InvariantCulture);
                Time_4 = true;
                Start_Time_4 = DateTime.Parse("11:30", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_4 = DateTime.Parse("12:28", System.Globalization.CultureInfo.InvariantCulture);
                Time_5 = true;
                Start_Time_5 = DateTime.Parse("13:30", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_5 = DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
                Time_6 = true;
                Start_Time_6 = DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_6 = DateTime.Parse("16:28", System.Globalization.CultureInfo.InvariantCulture);
                Time_7 = true;
                Start_Time_7 = DateTime.Parse("23:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_7 = DateTime.Parse("23:28", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                SessionHighLow1 = SessionHighLow(Close, DateTime.Parse("4:00 AM"), DateTime.Parse("4:01 AM"));
                SessionHighLow1.Plots[0].Brush = Brushes.HotPink;
                SessionHighLow1.Plots[1].Brush = Brushes.HotPink;
                AddChartIndicator(SessionHighLow1);

                SetProfitTarget("", CalculationMode.Ticks, Profit_Target);
                SetStopLoss("", CalculationMode.Ticks, Stop_Loss, false);

                tradingConditions = new List<TradingCondition>
                {
                    new TradingCondition(Time_2, Start_Time_2, Stop_Time_2, new[] { DayOfWeek.Monday }, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_2, Start_Time_2, Stop_Time_2, new[] { DayOfWeek.Monday }, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true),
                    new TradingCondition(Time_3, Start_Time_3, Stop_Time_3, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_3, Start_Time_3, Stop_Time_3, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true),
                    new TradingCondition(Time_4, Start_Time_4, Stop_Time_4, null, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_4, Start_Time_4, Stop_Time_4, null, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true),
                    new TradingCondition(Time_5, Start_Time_5, Stop_Time_5, null, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_5, Start_Time_5, Stop_Time_5, null, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true),

                    new TradingCondition(Time_6, Start_Time_6, Stop_Time_6, new[] { DayOfWeek.Tuesday }, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_6, Start_Time_6, Stop_Time_6, new[] { DayOfWeek.Tuesday }, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true),

                    new TradingCondition(Time_7, Start_Time_7, Stop_Time_7, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }, () => CrossAbove(High, SessionHighLow1.Session_Low, 1), isLongTrade: false),
                    new TradingCondition(Time_7, Start_Time_7, Stop_Time_7, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }, () => CrossBelow(High, SessionHighLow1.Session_Low, 1), isLongTrade: true)
                };

            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                // Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                // Print("Time[0]: " + Time[0].ToString());
                // Print("CurrentBar: " + CurrentBar);

                DateTime currentTime = Time[0];
                DayOfWeek currentDay = Time[0].DayOfWeek;
                
                Print("currentTime: " + currentTime);
                Print("currentDay:" + currentDay);

                foreach (var condition in tradingConditions)
                {
                    Print("condition: " + condition.Start);

                    if (condition.isTimeConditionMet(currentTime) && (condition.Days == null || condition.Days.Contains(currentDay)) && condition.CrossCondition())
                    {
                        if (condition.IsLongTrade)
                            EnterLong(quantity: CONTRACTS, signalName: "");
                        else
                            EnterShort(quantity: CONTRACTS, signalName: "");
                    }
                }
            }
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "CONTRACTS", Order = 1, GroupName = "Parameters")]
        public int CONTRACTS
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Profit_Target", Order = 2, GroupName = "Parameters")]
        public int Profit_Target
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop_Loss", Order = 3, GroupName = "Parameters")]
        public int Stop_Loss
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_2", Order = 4, GroupName = "Parameters")]
        public bool Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_2", Description = "3:22am-London", Order = 5, GroupName = "Parameters")]
        public DateTime Start_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_2", Description = "4:07am London", Order = 6, GroupName = "Parameters")]
        public DateTime Stop_Time_2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_3", Order = 7, GroupName = "Parameters")]
        public bool Time_3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_3", Description = "New York 7:52am", Order = 8, GroupName = "Parameters")]
        public DateTime Start_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_3", Description = "New York  08:37am", Order = 9, GroupName = "Parameters")]
        public DateTime Stop_Time_3
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_4", Order = 10, GroupName = "Parameters")]
        public bool Time_4
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_4", Description = "9:22am", Order = 11, GroupName = "Parameters")]
        public DateTime Start_Time_4
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_4", Description = "10:07am", Order = 12, GroupName = "Parameters")]
        public DateTime Stop_Time_4
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_5", Order = 13, GroupName = "Parameters")]
        public bool Time_5
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_5", Order = 14, GroupName = "Parameters")]
        public DateTime Start_Time_5
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_5", Description = "2:37pm", Order = 15, GroupName = "Parameters")]
        public DateTime Stop_Time_5
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_6", Order = 16, GroupName = "Parameters")]
        public bool Time_6
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_6", Description = "3:22pm", Order = 17, GroupName = "Parameters")]
        public DateTime Start_Time_6
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_6", Description = "AMD sayd 4:07 but that is usually and exit time so we have this programmed to stop trading at 3:55pm", Order = 18, GroupName = "Parameters")]
        public DateTime Stop_Time_6
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_7", Order = 19, GroupName = "Parameters")]
        public bool Time_7
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_7", Description = "1:52am  Asia AMD entry time", Order = 20, GroupName = "Parameters")]
        public DateTime Start_Time_7
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_7", Description = "2:37am", Order = 21, GroupName = "Parameters")]
        public DateTime Stop_Time_7
        { get; set; }
        #endregion

    }
}
