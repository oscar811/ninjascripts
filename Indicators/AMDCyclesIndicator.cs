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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AMDCyclesIndicator : Indicator
	{
        private List<TimeWindow> timeWindows = new List<TimeWindow>();

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"AMDCyclesIndicator";
				Name										= "AMDCyclesIndicator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;

                StartTime1      = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime1        = DateTime.Parse("19:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime2      = DateTime.Parse("19:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime2        = DateTime.Parse("21:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime3      = DateTime.Parse("21:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime3        = DateTime.Parse("22:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime4      = DateTime.Parse("22:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime4        = DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);

                StartTime5      = DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime5        = DateTime.Parse("01:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime6      = DateTime.Parse("01:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime6        = DateTime.Parse("03:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime7      = DateTime.Parse("03:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime7        = DateTime.Parse("04:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime8      = DateTime.Parse("04:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime8        = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);

                StartTime9      = DateTime.Parse("06:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime9        = DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime10      = DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime10        = DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime11      = DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime11        = DateTime.Parse("10:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime12      = DateTime.Parse("10:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime12        = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);

                StartTime13      = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime13        = DateTime.Parse("13:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime14      = DateTime.Parse("13:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime14        = DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
                StartTime15      = DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
				EndTime15        = DateTime.Parse("16:30", System.Globalization.CultureInfo.InvariantCulture);
                StartTime16      = DateTime.Parse("16:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime16        = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);

				AddPlot(Brushes.Transparent,"Asian_A_High");
                AddPlot(Brushes.Transparent,"Asian_A_Low");
                AddPlot(Brushes.Transparent,"Asian_M_High");
                AddPlot(Brushes.Transparent,"Asian_M_Low");
                AddPlot(Brushes.Transparent,"Asian_D_High");
                AddPlot(Brushes.Transparent,"Asian_D_Low");
                AddPlot(Brushes.Transparent,"Asian_R_High");
                AddPlot(Brushes.Transparent,"Asian_R_Low");

                AddPlot(Brushes.Transparent,"London_A_High");
                AddPlot(Brushes.Transparent,"London_A_Low");
                AddPlot(Brushes.Transparent,"London_M_High");
                AddPlot(Brushes.Transparent,"London_M_Low");
                AddPlot(Brushes.Transparent,"London_D_High");
                AddPlot(Brushes.Transparent,"London_D_Low");
                AddPlot(Brushes.Transparent,"London_R_High");
                AddPlot(Brushes.Transparent,"London_R_Low");

                AddPlot(Brushes.Transparent,"Ny_Am_A_High");
                AddPlot(Brushes.Transparent,"Ny_Am_A_Low");
                AddPlot(Brushes.Transparent,"Ny_Am_M_High");
                AddPlot(Brushes.Transparent,"Ny_Am_M_Low");
                AddPlot(Brushes.Transparent,"Ny_Am_D_High");
                AddPlot(Brushes.Transparent,"Ny_Am_D_Low");
                AddPlot(Brushes.Transparent,"Ny_Am_R_High");
                AddPlot(Brushes.Transparent,"Ny_Am_R_Low");

                AddPlot(Brushes.Transparent,"Ny_Pm_A_High");
                AddPlot(Brushes.Transparent,"Ny_Pm_A_Low");
                AddPlot(Brushes.Transparent,"Ny_Pm_M_High");
                AddPlot(Brushes.Transparent,"Ny_Pm_M_Low");
                AddPlot(Brushes.Transparent,"Ny_Pm_D_High");
                AddPlot(Brushes.Transparent,"Ny_Pm_D_Low");
                AddPlot(Brushes.Transparent,"Ny_Pm_R_High");
                AddPlot(Brushes.Transparent,"Ny_Pm_R_Low");
			}
			else if (State == State.Configure)
			{
                timeWindows.Add(new TimeWindow(1, StartTime1, EndTime1, Asian_A_High, Asian_A_Low));
                timeWindows.Add(new TimeWindow(2, StartTime2, EndTime2, Asian_D_High, Asian_D_Low));
                timeWindows.Add(new TimeWindow(3, StartTime3, EndTime3, Asian_M_High, Asian_M_Low));
                timeWindows.Add(new TimeWindow(4, StartTime4, EndTime4, Asian_R_High, Asian_R_Low));

                timeWindows.Add(new TimeWindow(5, StartTime5, EndTime5, London_A_High, London_A_Low));
                timeWindows.Add(new TimeWindow(6, StartTime6, EndTime6, London_D_High, London_D_Low));
                timeWindows.Add(new TimeWindow(7, StartTime7, EndTime7, London_M_High, London_M_Low));
                timeWindows.Add(new TimeWindow(8, StartTime8, EndTime8, London_R_High, London_R_Low));

                timeWindows.Add(new TimeWindow(9, StartTime9, EndTime9, Ny_Am_A_High, Ny_Am_A_Low));
                timeWindows.Add(new TimeWindow(10, StartTime10, EndTime10, Ny_Am_D_High, Ny_Am_D_Low));
                timeWindows.Add(new TimeWindow(11, StartTime11, EndTime11, Ny_Am_M_High, Ny_Am_M_Low));
                timeWindows.Add(new TimeWindow(12, StartTime12, EndTime12, Ny_Am_R_High, Ny_Am_R_Low));

                timeWindows.Add(new TimeWindow(13, StartTime13, EndTime13, Ny_Pm_A_High, Ny_Pm_A_Low));
                timeWindows.Add(new TimeWindow(14, StartTime14, EndTime14, Ny_Pm_D_High, Ny_Pm_D_Low));
                timeWindows.Add(new TimeWindow(15, StartTime15, EndTime15, Ny_Pm_M_High, Ny_Pm_M_Low));
                timeWindows.Add(new TimeWindow(16, StartTime16, EndTime16, Ny_Pm_R_High, Ny_Pm_R_Low));

				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
			try
			{
	            if (CurrentBar < 20)
	                return;

				DateTime estTime = Time[0];
				int i = 0;

                foreach (var window in timeWindows)
	            {
                    if (estTime.TimeOfDay >= window.StartTime.TimeOfDay && estTime.TimeOfDay <= window.EndTime.TimeOfDay)
	                {
	                    window.IsActive = true;
	                }
	                else
	                {
	                    window.StartBar = 0;
	                    window.IsActive = false;
	                }

	                if (window.IsActive)
	                {
	                    if (window.StartBar == 0)
	                    {
	                        window.StartBar = CurrentBar;
	                        window.HighPrices[0] = High[0];
	                        window.LowPrices[0] = Low[0];												
	                    }
						else
						{
							window.HighPrices[0] = Math.Max(window.HighPrices[1], High[0]);
		                    window.LowPrices[0] = Math.Min(window.LowPrices[1], Low[0]);
							
							int barsSinceStart = CurrentBar - window.StartBar;

	                    	RemoveDrawObject("MyBox" + window.StartBar.ToString());
	                    	Draw.Rectangle(this, "MyBox" + window.StartBar.ToString(), true, barsSinceStart, window.HighPrices[0], 0, window.LowPrices[0], Brushes.Blue, Brushes.Transparent, 50);
						}
	                }
					else 
					{
						if (window.HighPrices[1] > 0) window.HighPrices[0] = window.HighPrices[1];
						if (window.LowPrices[1] > 0) window.LowPrices[0] = window.LowPrices[1];
						
						Print("window.HighPrices[0] : " + window.HighPrices[0]);
					}
					
					i++;
	            }
			}
            catch (Exception e)
            {
                Print("Exception caught: " + e.Message);
                Print("Stack Trace: " + e.StackTrace);
            }
		}

        private class TimeWindow
        {
            public int Index { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public Series<double> HighPrices { get; set; }
            public Series<double>  LowPrices { get; set; }

            public int StartBar { get; set; }
            public bool IsActive { get; set; }

            public TimeWindow(int index, DateTime start, DateTime end, Series<double> highPrices, Series<double> lowPrices)
            {
                Index = index;
                StartTime = start;
                EndTime = end;
                HighPrices = highPrices;
                LowPrices = lowPrices;
            }
        }

		#region Properties

		[Browsable(true)]
        [XmlIgnore]
        public Series<double> Asian_A_High
        {
            get { return Values[0]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Asian_A_Low
        {
            get { return Values[1]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Asian_M_High
        {
            get { return Values[2]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_M_Low
        {
            get { return Values[3]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_D_High
        {
            get { return Values[4]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_D_Low
        {
            get { return Values[5]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_R_High
        {
            get { return Values[6]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Asian_R_Low
        {
            get { return Values[7]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> London_A_High
        {
            get { return Values[8]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> London_A_Low
        {
            get { return Values[9]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> London_M_High
        {
            get { return Values[10]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_M_Low
        {
            get { return Values[11]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_D_High
        {
            get { return Values[12]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_D_Low
        {
            get { return Values[13]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_R_High
        {
            get { return Values[14]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> London_R_Low
        {
            get { return Values[15]; }
        }

        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Am_A_High
        {
            get { return Values[16]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Am_A_Low
        {
            get { return Values[17]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Am_M_High
        {
            get { return Values[18]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_M_Low
        {
            get { return Values[19]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_D_High
        {
            get { return Values[20]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_D_Low
        {
            get { return Values[21]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_R_High
        {
            get { return Values[22]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Am_R_Low
        {
            get { return Values[23]; }
        }

        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Pm_A_High
        {
            get { return Values[24]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Pm_A_Low
        {
            get { return Values[25]; }
        }
        [Browsable(true)]
        [XmlIgnore]
        public Series<double> Ny_Pm_M_High
        {
            get { return Values[26]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_M_Low
        {
            get { return Values[27]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_D_High
        {
            get { return Values[28]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_D_Low
        {
            get { return Values[29]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_R_High
        {
            get { return Values[30]; }
        }
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Ny_Pm_R_Low
        {
            get { return Values[31]; }
        }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia A start", Order = 3, GroupName = "TimeWindows")]
        public DateTime StartTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia A end", Order = 4, GroupName = "TimeWindows")]
        public DateTime EndTime1 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia M start", Order = 5, GroupName = "TimeWindows")]
        public DateTime StartTime2 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia M end", Order = 6, GroupName = "TimeWindows")]
        public DateTime EndTime2 { get; set; }

        [NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia D start", Order = 7, GroupName = "TimeWindows")]
        public DateTime StartTime3 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia D end", Order = 8, GroupName = "TimeWindows")]
        public DateTime EndTime3 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia R start", Order = 9, GroupName = "TimeWindows")]
        public DateTime StartTime4 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia R end", Order = 10, GroupName = "TimeWindows")]
        public DateTime EndTime4 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London A start", Order = 11, GroupName = "TimeWindows")]
        public DateTime StartTime5 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London A end", Order = 12, GroupName = "TimeWindows")]
        public DateTime EndTime5 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London M start", Order = 13, GroupName = "TimeWindows")]
        public DateTime StartTime6 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London M end", Order = 14, GroupName = "TimeWindows")]
        public DateTime EndTime6 { get; set; }

        [NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London D start", Order = 15, GroupName = "TimeWindows")]
        public DateTime StartTime7 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London D end", Order = 16, GroupName = "TimeWindows")]
        public DateTime EndTime7 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London R start", Order = 17, GroupName = "TimeWindows")]
        public DateTime StartTime8 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London R end", Order = 18, GroupName = "TimeWindows")]
        public DateTime EndTime8 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am A start", Order = 19, GroupName = "TimeWindows")]
        public DateTime StartTime9 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am A end", Order = 20, GroupName = "TimeWindows")]
        public DateTime EndTime9 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am M start", Order = 21, GroupName = "TimeWindows")]
        public DateTime StartTime10 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am M end", Order = 22, GroupName = "TimeWindows")]
        public DateTime EndTime10 { get; set; }

        [NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am D start", Order = 23, GroupName = "TimeWindows")]
        public DateTime StartTime11 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am D end", Order = 24, GroupName = "TimeWindows")]
        public DateTime EndTime11 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am R start", Order = 25, GroupName = "TimeWindows")]
        public DateTime StartTime12 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Am R end", Order = 26, GroupName = "TimeWindows")]
        public DateTime EndTime12 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm A start", Order = 27, GroupName = "TimeWindows")]
        public DateTime StartTime13 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm A end", Order = 28, GroupName = "TimeWindows")]
        public DateTime EndTime13 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm M start", Order = 29, GroupName = "TimeWindows")]
        public DateTime StartTime14 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm M end", Order = 30, GroupName = "TimeWindows")]
        public DateTime EndTime14 { get; set; }

        [NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm D start", Order = 31, GroupName = "TimeWindows")]
        public DateTime StartTime15 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm D end", Order = 32, GroupName = "TimeWindows")]
        public DateTime EndTime15 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm R start", Order = 33, GroupName = "TimeWindows")]
        public DateTime StartTime16 { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Ny Pm R end", Order = 34, GroupName = "TimeWindows")]
        public DateTime EndTime16 { get; set; }


		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AMDCyclesIndicator[] cacheAMDCyclesIndicator;
		public AMDCyclesIndicator AMDCyclesIndicator(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			return AMDCyclesIndicator(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4, startTime5, endTime5, startTime6, endTime6, startTime7, endTime7, startTime8, endTime8, startTime9, endTime9, startTime10, endTime10, startTime11, endTime11, startTime12, endTime12, startTime13, endTime13, startTime14, endTime14, startTime15, endTime15, startTime16, endTime16);
		}

		public AMDCyclesIndicator AMDCyclesIndicator(ISeries<double> input, DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			if (cacheAMDCyclesIndicator != null)
				for (int idx = 0; idx < cacheAMDCyclesIndicator.Length; idx++)
					if (cacheAMDCyclesIndicator[idx] != null && cacheAMDCyclesIndicator[idx].StartTime1 == startTime1 && cacheAMDCyclesIndicator[idx].EndTime1 == endTime1 && cacheAMDCyclesIndicator[idx].StartTime2 == startTime2 && cacheAMDCyclesIndicator[idx].EndTime2 == endTime2 && cacheAMDCyclesIndicator[idx].StartTime3 == startTime3 && cacheAMDCyclesIndicator[idx].EndTime3 == endTime3 && cacheAMDCyclesIndicator[idx].StartTime4 == startTime4 && cacheAMDCyclesIndicator[idx].EndTime4 == endTime4 && cacheAMDCyclesIndicator[idx].StartTime5 == startTime5 && cacheAMDCyclesIndicator[idx].EndTime5 == endTime5 && cacheAMDCyclesIndicator[idx].StartTime6 == startTime6 && cacheAMDCyclesIndicator[idx].EndTime6 == endTime6 && cacheAMDCyclesIndicator[idx].StartTime7 == startTime7 && cacheAMDCyclesIndicator[idx].EndTime7 == endTime7 && cacheAMDCyclesIndicator[idx].StartTime8 == startTime8 && cacheAMDCyclesIndicator[idx].EndTime8 == endTime8 && cacheAMDCyclesIndicator[idx].StartTime9 == startTime9 && cacheAMDCyclesIndicator[idx].EndTime9 == endTime9 && cacheAMDCyclesIndicator[idx].StartTime10 == startTime10 && cacheAMDCyclesIndicator[idx].EndTime10 == endTime10 && cacheAMDCyclesIndicator[idx].StartTime11 == startTime11 && cacheAMDCyclesIndicator[idx].EndTime11 == endTime11 && cacheAMDCyclesIndicator[idx].StartTime12 == startTime12 && cacheAMDCyclesIndicator[idx].EndTime12 == endTime12 && cacheAMDCyclesIndicator[idx].StartTime13 == startTime13 && cacheAMDCyclesIndicator[idx].EndTime13 == endTime13 && cacheAMDCyclesIndicator[idx].StartTime14 == startTime14 && cacheAMDCyclesIndicator[idx].EndTime14 == endTime14 && cacheAMDCyclesIndicator[idx].StartTime15 == startTime15 && cacheAMDCyclesIndicator[idx].EndTime15 == endTime15 && cacheAMDCyclesIndicator[idx].StartTime16 == startTime16 && cacheAMDCyclesIndicator[idx].EndTime16 == endTime16 && cacheAMDCyclesIndicator[idx].EqualsInput(input))
						return cacheAMDCyclesIndicator[idx];
			return CacheIndicator<AMDCyclesIndicator>(new AMDCyclesIndicator(){ StartTime1 = startTime1, EndTime1 = endTime1, StartTime2 = startTime2, EndTime2 = endTime2, StartTime3 = startTime3, EndTime3 = endTime3, StartTime4 = startTime4, EndTime4 = endTime4, StartTime5 = startTime5, EndTime5 = endTime5, StartTime6 = startTime6, EndTime6 = endTime6, StartTime7 = startTime7, EndTime7 = endTime7, StartTime8 = startTime8, EndTime8 = endTime8, StartTime9 = startTime9, EndTime9 = endTime9, StartTime10 = startTime10, EndTime10 = endTime10, StartTime11 = startTime11, EndTime11 = endTime11, StartTime12 = startTime12, EndTime12 = endTime12, StartTime13 = startTime13, EndTime13 = endTime13, StartTime14 = startTime14, EndTime14 = endTime14, StartTime15 = startTime15, EndTime15 = endTime15, StartTime16 = startTime16, EndTime16 = endTime16 }, input, ref cacheAMDCyclesIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AMDCyclesIndicator AMDCyclesIndicator(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			return indicator.AMDCyclesIndicator(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4, startTime5, endTime5, startTime6, endTime6, startTime7, endTime7, startTime8, endTime8, startTime9, endTime9, startTime10, endTime10, startTime11, endTime11, startTime12, endTime12, startTime13, endTime13, startTime14, endTime14, startTime15, endTime15, startTime16, endTime16);
		}

		public Indicators.AMDCyclesIndicator AMDCyclesIndicator(ISeries<double> input , DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			return indicator.AMDCyclesIndicator(input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4, startTime5, endTime5, startTime6, endTime6, startTime7, endTime7, startTime8, endTime8, startTime9, endTime9, startTime10, endTime10, startTime11, endTime11, startTime12, endTime12, startTime13, endTime13, startTime14, endTime14, startTime15, endTime15, startTime16, endTime16);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AMDCyclesIndicator AMDCyclesIndicator(DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			return indicator.AMDCyclesIndicator(Input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4, startTime5, endTime5, startTime6, endTime6, startTime7, endTime7, startTime8, endTime8, startTime9, endTime9, startTime10, endTime10, startTime11, endTime11, startTime12, endTime12, startTime13, endTime13, startTime14, endTime14, startTime15, endTime15, startTime16, endTime16);
		}

		public Indicators.AMDCyclesIndicator AMDCyclesIndicator(ISeries<double> input , DateTime startTime1, DateTime endTime1, DateTime startTime2, DateTime endTime2, DateTime startTime3, DateTime endTime3, DateTime startTime4, DateTime endTime4, DateTime startTime5, DateTime endTime5, DateTime startTime6, DateTime endTime6, DateTime startTime7, DateTime endTime7, DateTime startTime8, DateTime endTime8, DateTime startTime9, DateTime endTime9, DateTime startTime10, DateTime endTime10, DateTime startTime11, DateTime endTime11, DateTime startTime12, DateTime endTime12, DateTime startTime13, DateTime endTime13, DateTime startTime14, DateTime endTime14, DateTime startTime15, DateTime endTime15, DateTime startTime16, DateTime endTime16)
		{
			return indicator.AMDCyclesIndicator(input, startTime1, endTime1, startTime2, endTime2, startTime3, endTime3, startTime4, endTime4, startTime5, endTime5, startTime6, endTime6, startTime7, endTime7, startTime8, endTime8, startTime9, endTime9, startTime10, endTime10, startTime11, endTime11, startTime12, endTime12, startTime13, endTime13, startTime14, endTime14, startTime15, endTime15, startTime16, endTime16);
		}
	}
}

#endregion
