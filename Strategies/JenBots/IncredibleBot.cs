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
using System.Diagnostics.Contracts;
using NinjaTrader.NinjaScript.Strategies.RajAlgos;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.JenBots
{
    public class IncredibleBot : Strategy
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
                Description = @"Incredible Impossible";
                Name = "IncredibleBot";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
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
                Profit_Target = 75;
                Stop_Loss = 75;
                Time_2 = true;
                Start_Time_2 = DateTime.Parse("01:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_2 = DateTime.Parse("01:58", System.Globalization.CultureInfo.InvariantCulture);
                Time_3 = false;
                Start_Time_3 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_3 = DateTime.Parse("12:30", System.Globalization.CultureInfo.InvariantCulture);
                Time_4 = true;
                Start_Time_4 = DateTime.Parse("05:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_4 = DateTime.Parse("05:58", System.Globalization.CultureInfo.InvariantCulture);
                Time_5 = true;
                Start_Time_5 = DateTime.Parse("10:30", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_5 = DateTime.Parse("12:30", System.Globalization.CultureInfo.InvariantCulture);
                Time_6 = true;
                Start_Time_6 = DateTime.Parse("13:31", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_6 = DateTime.Parse("14:58", System.Globalization.CultureInfo.InvariantCulture);
                Time_7 = true;
                Start_Time_7 = DateTime.Parse("15:30", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_7 = DateTime.Parse("15:58", System.Globalization.CultureInfo.InvariantCulture);
                Time_8 = true;
                Start_Time_8 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_8 = DateTime.Parse("18:28", System.Globalization.CultureInfo.InvariantCulture);
                Time_9 = true;
                Start_Time_9 = DateTime.Parse("20:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_9 = DateTime.Parse("20:30", System.Globalization.CultureInfo.InvariantCulture);
                Time_10 = true;
                Start_Time_10 = DateTime.Parse("22:00", System.Globalization.CultureInfo.InvariantCulture);
                Stop_Time_10 = DateTime.Parse("22:30", System.Globalization.CultureInfo.InvariantCulture);
                Fast_Macd = 3;
                Slow_Macd = 8;
                Signal_Macd = 8;
                Fast_EMA = 28;
                Slow_Ema = 200;
            }
            else if (State == State.Configure)
            {
                SwingRays2c1 = SwingRays2c(Close, 7, 0, true, 1);
                MACD1 = MACD(Close, Convert.ToInt32(Fast_Macd), Convert.ToInt32(Slow_Macd), Convert.ToInt32(Signal_Macd));
                EMA1 = EMA(Close, Convert.ToInt32(Fast_EMA));
                EMA2 = EMA(Close, Convert.ToInt32(Slow_Ema));
                MACD2 = MACD(Close, Convert.ToInt32(Fast_Macd), Convert.ToInt32(Slow_Macd), Convert.ToInt32(Signal_Macd));
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

                timePeriods = new List<TimePeriod>
                {
                    new TimePeriod(Time_2, Start_Time_2, Stop_Time_2, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }),
                    new TimePeriod(Time_3, Start_Time_3, Stop_Time_3, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    new TimePeriod(Time_4, Start_Time_4, Stop_Time_4, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }),
                    new TimePeriod(Time_5, Start_Time_5, Stop_Time_5, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    new TimePeriod(Time_6, Start_Time_6, Stop_Time_6, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    new TimePeriod(Time_7, Start_Time_7, Stop_Time_7, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
                    new TimePeriod(Time_8, Start_Time_8, Stop_Time_8, new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }),
                    new TimePeriod(Time_9, Start_Time_9, Stop_Time_9, null),
                    new TimePeriod(Time_10, Start_Time_10, Stop_Time_10, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday })
                };
            }
        }

        protected override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < BarsRequiredToTrade + 1)
                    return;

                if (BarsInProgress != 0 || CurrentBars[0] < 1)
                    return;

                // Draw.Text(this, "Tag_" + CurrentBar.ToString(), CurrentBar.ToString(), 0, Low[0] - TickSize * 10, Brushes.Red);
                // Print("Time[0]: " + Time[0].ToString());
                // Print("CurrentBar: " + CurrentBar);

                DateTime currentTime = Time[0];
                DayOfWeek currentDay = Time[0].DayOfWeek;


                foreach (var timePeriod in timePeriods)
                {
                    if (timePeriod.isTimeConditionMet(currentTime, currentDay))
                    {
                        if (SwingRays2c1.IsHighBroken[0] == 1.0 && CrossBelow(MACD1.Default, MACD1.Avg, 2) && EMA1[0] < EMA2[0])
                        {
                            EnterShort(quantity: CONTRACTS, signalName: "Short");
                        }

                        if (SwingRays2c1.IsLowBroken[0] == 1.0 && CrossAbove(MACD1.Default, MACD2.Avg, 2) && EMA1[0] > EMA2[0])
                        {
                            EnterLong(quantity: CONTRACTS, signalName: "Long");
                        }
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

        [NinjaScriptProperty]
        [Display(Name = "Time_8", Order = 22, GroupName = "Parameters")]
        public bool Time_8
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_8", Order = 23, GroupName = "Parameters")]
        public DateTime Start_Time_8
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_8", Order = 24, GroupName = "Parameters")]
        public DateTime Stop_Time_8
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_9", Order = 25, GroupName = "Parameters")]
        public bool Time_9
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_9", Order = 26, GroupName = "Parameters")]
        public DateTime Start_Time_9
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_9", Order = 27, GroupName = "Parameters")]
        public DateTime Stop_Time_9
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time_10", Order = 28, GroupName = "Parameters")]
        public bool Time_10
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start_Time_10", Order = 29, GroupName = "Parameters")]
        public DateTime Start_Time_10
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Stop_Time_10", Order = 30, GroupName = "Parameters")]
        public DateTime Stop_Time_10
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast_Macd", Order = 31, GroupName = "Parameters")]
        public int Fast_Macd
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow_Macd", Order = 32, GroupName = "Parameters")]
        public int Slow_Macd
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Signal_Macd", Order = 33, GroupName = "Parameters")]
        public int Signal_Macd
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast_EMA", Order = 34, GroupName = "Parameters")]
        public int Fast_EMA
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow_Ema", Order = 35, GroupName = "Parameters")]
        public int Slow_Ema
        { get; set; }
        #endregion

    }
}
