using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Media;
using System.ComponentModel;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class AMDCycles : Strategy
    {
        private double bbdn, bbup, dist;

        private double periodHigh = double.MinValue;
        private double periodLow = double.MaxValue;
        private bool isInTimeFrame = false;

        private const int LINE_LENGTH_TICKS = 100000;
        private const string TAG_SUFFIX = "_VertLineAtTime";

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Converted strategy from PineScript.";
                Name = "AMDCycles";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
            }
            else if (State == State.Configure)
            {
                // Configure your strategy here
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20)
                return;

            DateTime todayLineTime = Time[0].Date.Add(LineTime.TimeOfDay);
            if (Time[0] >= todayLineTime && ((Time[1].TimeOfDay < LineTime.TimeOfDay && Time[0].Date.Equals(Time[1].Date)) || Time[1].Date == Time[0].Date.AddDays(-1)))
            {
                string tag = Time[0].ToString() + TAG_SUFFIX + "AboveBar";
                double startY = High[0] + (BarToLineOffsetInTicks * TickSize);
                double endY = High[0] + (LINE_LENGTH_TICKS * TickSize);
                Draw.Line(this, tag, false, 0, startY, 0, endY, LineColor, LineDashStyle, LineThickness);
                tag = Time[0].ToString() + TAG_SUFFIX + "BelowBar";
                startY = 0;
                endY = Low[0] - (BarToLineOffsetInTicks * TickSize);
                Draw.Line(this, tag, false, 0, startY, 0, endY, LineColor, LineDashStyle, LineThickness);
            }

            // Convert the current bar's time to Eastern Time (ET)
            //         DateTime etTime = Time[0].AddHours(-5); // Assuming data is in UTC, adjust if necessary

            //         if (etTime.TimeOfDay == new TimeSpan(9, 0, 0)) // Start of the timeframe
            //         {
            //             isInTimeFrame = true;
            //             periodHigh = double.MinValue;
            //             periodLow = double.MaxValue;
            //         }

            //         if (isInTimeFrame)
            //         {
            //             periodHigh = Math.Max(periodHigh, High[0]);
            //             periodLow = Math.Min(periodLow, Low[0]);
            //         }

            //         if (etTime.TimeOfDay == new TimeSpan(10, 30, 0)) // End of the timeframe
            //         {
            //             isInTimeFrame = false;

            //             // Draw the rectangle for the 90-min timeframe
            //             Draw.Rectangle(this, "rectangle" + CurrentBar, false, Time[0], periodHigh, Time[0].AddMinutes(-90), periodLow, Brushes.Blue, Brushes.Transparent, 50);
            //         }

            //Draw.Line(this, "tag1", false, 10, 1000, 0, 1001, Brushes.LimeGreen, DashStyleHelper.Dot, 2);

            // SuperBollingerTrend logic
            // This is a very simplified version. Pine Script version has more features.
            // bbup = SMA(High, 12)[0] + StdDev(High, 12)[0] * 2.0;
            // bbdn = SMA(Low, 12)[0] - StdDev(Low, 12)[0] * 2.0;

            // // Add your strategy logic here
            // // For instance, if you want to enter a long position:
            // if (Close[0] > bbup)
            // {
            //     EnterLong("Long");
            // }

            // // Similarly, for short position:
            // if (Close[0] < bbdn)
            // {
            //     EnterShort("Short");
            // }

            // Continue your strategy logic and conversion here...

        }

        #region Properties

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "LineTime", Description = "The time to draw the line for.", Order = 1, GroupName = "Parameters")]
        public DateTime LineTime
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "LineColor", Description = "Color of the vertical line", Order = 2, GroupName = "Parameters")]
        public Brush LineColor
        { get; set; }

        [Browsable(false)]
        public string LineColorSerializable
        {
            get { return Serialize.BrushToString(LineColor); }
            set { LineColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "LineDashStyle", Description = "Dash style of the line.", Order = 3, GroupName = "Parameters")]
        public DashStyleHelper LineDashStyle
        { get; set; }

        [Browsable(false)]
        public string LineDashStyleSerializable
        {
            get { return LineDashStyle.ToString(); }
            set { LineDashStyle = DeSerializeDashStyle(value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "LineThickness", Description = "Vertical line's thickness", Order = 4, GroupName = "Parameters")]
        public int LineThickness
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BarToLineOffsetInTicks", Description = "Space between the bar and the line.", Order = 5, GroupName = "Parameters")]
        public int BarToLineOffsetInTicks
        { get; set; }

        // DashStyle DeSerializer
        public DashStyleHelper DeSerializeDashStyle(string dashStyle)
        {
            if (dashStyle == null) return DashStyleHelper.Solid;

            if (dashStyle.Equals("Dash")) return DashStyleHelper.Dash;
            else if (dashStyle.Equals("DashDot")) return DashStyleHelper.DashDot;
            else if (dashStyle.Equals("DashDotDot")) return DashStyleHelper.DashDotDot;
            else if (dashStyle.Equals("Dot")) return DashStyleHelper.Dot;
            else if (dashStyle.Equals("Solid")) return DashStyleHelper.Solid;
            return DashStyleHelper.Solid;   // Deafult if XML is messed up
        }

        #endregion
    }
}
