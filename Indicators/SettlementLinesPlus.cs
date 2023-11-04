using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;

// This indicator plots the prior two day's settlements as a horizontal lines.  Currently,
// only the CME Index Futures settlement time is used for determining the settlement price.  It is the
// same time for many other futures products, which is the closing price at 3:15 PM Central Time.
//
// Author:  Steven Lynn
// Email:   smlguard24-ninjatrader@yahoo.com

/*
 * Versions
 * - 3.1.0 - May 31, 2021 - Added input parameter to select the trading hours for settlement lines.
 *                          Default is CME Index Futures RTH.
 * - 3.1.1 - July 6, 2021 - Settlement for holiday trading hours did not work correctly after July 4 holiday.
 *                          Added support for holidays and partial holidays as a full holiday.
 */

// Modification table:
// 11-21-2022 Tasker_182   Changed Name to SettlementLinesPlus:
// Added Percent lines based off of settlement price per FIO: https://futures.io/elite-circle/1261-want-your-ninjatrader-indicator-created-free-888.html#post875647


namespace NinjaTrader.NinjaScript.Indicators
{
	public class SettlementLinesPlus : Indicator
	{
		private double settlementPrice;
		private double priorSettlementPrice;

		private SessionIterator sessionIterator;

		private readonly List<int> newSessionBarIdxArr = new List<int>();
		private DateTime currentDate = Globals.MinDate;
		private DateTime cacheSessionDate = Globals.MinDate;
		private DateTime cacheSessionEnd = Globals.MinDate;
		private DateTime sessionDateTmp = Globals.MinDate;
		private TradingHours tradingHours;
		private Dictionary<DateTime, string> holidays;
		private Dictionary<DateTime, PartialHoliday> partialHolidays;
		private const string DEFAULT_TRADING_HOURS = "CME US Index Futures RTH";
		private double AA, AB, BB, BC, CC, CD, DD, DE, EE, EF;		// 11-21-2022

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = "Futures Settlement Lines v3.0.1 - RTH Session Close Price, Plus +/- Percent from settlement lines";
				Name = "SettlementLinesPlus";
				Calculate = Calculate.OnPriceChange;
				IsAutoScale = false;
				IsOverlay = true;
				IsSuspendedWhileInactive = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				PaintPriceMarkers = true;
				ScaleJustification = ScaleJustification.Right;
				tradingHours = TradingHours.Get("CME US Index Futures RTH");
				Show0point5		= false;
				Show1point0		= true;
				Show1point5		= true;
				Show2point0		= true;
				Show2point5		= false;
				
				holidays = new Dictionary<DateTime, string>();
				foreach(KeyValuePair<DateTime, string> h in tradingHours.Holidays)
				{
					holidays.Add(h.Key, h.Value);
				}
				foreach (KeyValuePair<DateTime, PartialHoliday> ph in tradingHours.PartialHolidays)
				{
					holidays.Add(ph.Key, ph.Value.Description);
				}
				
				AddPlot(new Stroke(Brushes.Cyan, 2), PlotStyle.Hash, "Settlement");
				AddPlot(new Stroke(Brushes.HotPink, 2), PlotStyle.Hash, "Prior Settlement");
				// below added 11-21-2022
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Hash, "HalfPlus");
				AddPlot(new Stroke(Brushes.LightGreen, 1), PlotStyle.Hash, "HalfMinus");
				AddPlot(new Stroke(Brushes.Gold, 1), PlotStyle.Hash, "OnePlus");
				AddPlot(new Stroke(Brushes.LightYellow, 1), PlotStyle.Hash, "OneMinus");
				AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Hash, "OneandHalfPlus");
				AddPlot(new Stroke(Brushes.OrangeRed, 1), PlotStyle.Hash, "OneandHalfMinus");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Hash, "TwoPlus");
				AddPlot(new Stroke(Brushes.Crimson, 1), PlotStyle.Hash, "TwoMinus");
				AddPlot(new Stroke(Brushes.Magenta, 1), PlotStyle.Hash, "TwoandHalfPlus");
				AddPlot(new Stroke(Brushes.Lavender, 1), PlotStyle.Hash, "TwoandHalfMinus");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Instrument.FullName, new BarsPeriod { BarsPeriodType = BarsPeriodType.Day, Value = 1 }, tradingHours.Name, false);
			}
			else if (State == State.Historical)
			{
				// Displays a message if the bartype is not intraday
				if (!Bars.BarsType.IsIntraday)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", "Futures Settlement Lines Intraday Only Error", TextPosition.BottomRight);
					Log("Futures Settlement Lines only supports Intraday charts", LogLevel.Error);
				}
			}
			else if (State == State.DataLoaded)
			{
				//stores the sessions once bars are ready, but before OnBarUpdate is called
				sessionIterator = new SessionIterator(BarsArray[1]);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0)
				return;

			DateTime lastBarTimeStamp = GetLastBarSessionDate(Times[0][0]);

			if (currentDate != Globals.MinDate 
				&& (lastBarTimeStamp != currentDate && !holidays.ContainsKey(DaySeriesTime[0].Date)))
			{
				priorSettlementPrice = settlementPrice;
				settlementPrice = DaySeriesClose[0];
			}
			currentDate = lastBarTimeStamp;

			if (priorSettlementPrice != 0)
			{
				PriorSettlementPlotValues[0] = priorSettlementPrice;
			}

			if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)  // added 11-21-2022
			{
				if (settlementPrice > 0.0)
				{
					// Calculate the needs
					AA = settlementPrice * 1.00500;
					AB = settlementPrice * 0.99500;
					BB = settlementPrice * 1.01000;
					BC = settlementPrice * 0.99000;
					CC = settlementPrice * 1.01500;
					CD = settlementPrice * 0.98500;
					DD = settlementPrice * 1.02000;
					DE = settlementPrice * 0.98000;
					EE = settlementPrice * 1.02500;
					EF = settlementPrice * 0.97500;
				}
			}
			
			if (settlementPrice != 0)
			{
				SettlementPlotValues[0] = settlementPrice;
				
				// 11-21-2022 Draw the selected plots
				if (Show0point5)
				{
					HalfPlus[0] 		= AA;
					HalfMinus[0] 		= AB;
				}
				
				if (Show1point0)
				{
					OnePlus[0] 			= BB;
					OneMinus[0]			= BC;
				}
				
				if (Show1point5)
				{
					OneandHalfPlus[0] 	= CC;
					OneandHalfMinus[0]	= CD;
				}				

				if (Show2point0)
				{
					TwoPlus[0] 			= DD;
					TwoMinus[0]			= DE;
				}	
				
				if (Show2point5)
				{
					TwoandHalfPlus[0] 	= EE;
					TwoandHalfMinus[0]	= EF;
				}				
					
			}			
			
		}

		#region Misc
		private DateTime GetLastBarSessionDate(DateTime time)
		{
			// Check the time[0] against the previous session end
			if (time > cacheSessionEnd)
			{
				if (Bars.BarsType.IsIntraday)
				{
					// Make use of the stored session iterator to find the next session...
					sessionIterator.GetNextSession(time, true);
					// Store the actual session's end datetime as the session
					cacheSessionEnd = sessionIterator.ActualSessionEnd;
					// We need to convert that time from the session to the users time zone settings
					sessionDateTmp = TimeZoneInfo.ConvertTime(cacheSessionEnd.AddSeconds(-1), Globals.GeneralOptions.TimeZoneInfo, Bars.TradingHours.TimeZoneInfo).Date;
				}
				else
				{
					sessionDateTmp = time.Date;
				}
			}

			if (sessionDateTmp != cacheSessionDate)
			{
				if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
				{
					newSessionBarIdxArr.Add(CurrentBar);
				}
				cacheSessionDate = sessionDateTmp;
			}
			return sessionDateTmp;
		}
		#endregion


		#region Properties

		[Browsable(false)]
		[XmlIgnore()]
		public PriceSeries DaySeriesClose
		{
			get { return Closes[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public TimeSeries DaySeriesTime
		{
			get { return Times[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SettlementPlotValues
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorSettlementPlotValues
		{
			get { return Values[1]; }
		}

// below added 11-21-2022
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HalfPlus
		{
			get { return Values[2]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HalfMinus
		{
			get { return Values[3]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OnePlus
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OneMinus
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OneandHalfPlus
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OneandHalfMinus
		{
			get { return Values[7]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TwoPlus
		{
			get { return Values[8]; }
		}		

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TwoMinus
		{
			get { return Values[9]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TwoandHalfPlus
		{
			get { return Values[10]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TwoandHalfMinus
		{
			get { return Values[11]; }
		}			
		#endregion


		#region Input Parameters

		[NinjaScriptProperty]
		[Display(Name = "Trading Hours", GroupName = "Parameters", Order = 0)]
		[TypeConverter(typeof(TradingHoursDataConverter))]
		public string TradingHoursString
		{
			get { return tradingHours.ToString(); }
			set
			{
				if (Bars != null && value == TradingHours.UseInstrumentSettings)
					value = Instrument.MasterInstrument.TradingHours.Name;

				tradingHours = TradingHours.Get(value);
			}
		}
// Below added 11-21-2022		
		[NinjaScriptProperty]
		[Display(Name="Display 0.5% Line", Description="Shows the +/- 0.5% lines", Order=1, GroupName="PercentMoveLines")]
		public bool Show0point5
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Display 1.0% Line", Description="Shows the +/- 1.0% lines", Order=2, GroupName="PercentMoveLines")]
		public bool Show1point0
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Display 1.5% Line", Description="Shows the +/- 1.5% lines", Order=3, GroupName="PercentMoveLines")]
		public bool Show1point5
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Display 2.0 % Line", Description="Shows the +/- 2.0 % lines", Order=4, GroupName="PercentMoveLines")]
		public bool Show2point0
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Display 2.5 % Line", Description="Shows the +/- 2.5 % lines", Order=5, GroupName="PercentMoveLines")]
		public bool Show2point5
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SettlementLinesPlus[] cacheSettlementLinesPlus;
		public SettlementLinesPlus SettlementLinesPlus(string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			return SettlementLinesPlus(Input, tradingHoursString, show0point5, show1point0, show1point5, show2point0, show2point5);
		}

		public SettlementLinesPlus SettlementLinesPlus(ISeries<double> input, string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			if (cacheSettlementLinesPlus != null)
				for (int idx = 0; idx < cacheSettlementLinesPlus.Length; idx++)
					if (cacheSettlementLinesPlus[idx] != null && cacheSettlementLinesPlus[idx].TradingHoursString == tradingHoursString && cacheSettlementLinesPlus[idx].Show0point5 == show0point5 && cacheSettlementLinesPlus[idx].Show1point0 == show1point0 && cacheSettlementLinesPlus[idx].Show1point5 == show1point5 && cacheSettlementLinesPlus[idx].Show2point0 == show2point0 && cacheSettlementLinesPlus[idx].Show2point5 == show2point5 && cacheSettlementLinesPlus[idx].EqualsInput(input))
						return cacheSettlementLinesPlus[idx];
			return CacheIndicator<SettlementLinesPlus>(new SettlementLinesPlus(){ TradingHoursString = tradingHoursString, Show0point5 = show0point5, Show1point0 = show1point0, Show1point5 = show1point5, Show2point0 = show2point0, Show2point5 = show2point5 }, input, ref cacheSettlementLinesPlus);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SettlementLinesPlus SettlementLinesPlus(string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			return indicator.SettlementLinesPlus(Input, tradingHoursString, show0point5, show1point0, show1point5, show2point0, show2point5);
		}

		public Indicators.SettlementLinesPlus SettlementLinesPlus(ISeries<double> input , string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			return indicator.SettlementLinesPlus(input, tradingHoursString, show0point5, show1point0, show1point5, show2point0, show2point5);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SettlementLinesPlus SettlementLinesPlus(string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			return indicator.SettlementLinesPlus(Input, tradingHoursString, show0point5, show1point0, show1point5, show2point0, show2point5);
		}

		public Indicators.SettlementLinesPlus SettlementLinesPlus(ISeries<double> input , string tradingHoursString, bool show0point5, bool show1point0, bool show1point5, bool show2point0, bool show2point5)
		{
			return indicator.SettlementLinesPlus(input, tradingHoursString, show0point5, show1point0, show1point5, show2point0, show2point5);
		}
	}
}

#endregion
