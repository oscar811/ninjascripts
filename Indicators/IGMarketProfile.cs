#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace IGMarketProfileEnums
{
	public enum ColorMode
	{
		AllLettersSameColor,
		EachLetterDifferentColor
	}

	public enum SessionType
	{
		RTHAndETH = 0,
		Day,
		Week,
		Month
	}

	public enum TradingHours
	{
		RTH,
		ETH,
		Day,
		Week,
		Month,
		None
	}
}

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("Session", 1)]
	[Gui.CategoryOrder("IB", 2)]
	[Gui.CategoryOrder("TPO", 3)]
	[Gui.CategoryOrder("POC", 4)]
	[Gui.CategoryOrder("Virgin POC", 5)]
	[Gui.CategoryOrder("Price Histogram", 6)]
	[Gui.CategoryOrder("Volume Histogram", 7)]
	[Gui.CategoryOrder("Data", 8)]
	public class IGMarketProfile : Indicator
	{
		private List<MPHelper>		alMPHelper, alMPHelperDay, alMPHelperWeek, alMPHelperMonth;
		private int					arrowXOffset, barsPerDay, iBDistance, letterIndex, mPHelperCurrentIndex, newVolumePerPlotRange, numberOfPlots, pOCIndex, previousMonth, previousWeek, ticksPerPlotRange, tPOCount, tPOsAbove, tPOsBelow, vA;
		private ChartTab			chartTab;
		private Chart				chartWindow;
		private double				ceiling, floor, highDecimal, keyHigh, keyLow, lowDecimal, tickSize_x_TicksPerPlot, vol;
		private bool				iBDistanceinvalidPeriodType, invalidPeriodType, isBusy, isToolBarButtonAdded, panelActive, refreshTPO, toolBarButtonsAdded;

		private Brush				candleOutlineColor, downColor, upColor;
		private MPBar				currentTPOBar;
		private MPHelper			currentTPOSession, currentTPOSessionDay, currentTPOSessionWeek, currentTPOSessionMonth;
		private DateTimeFormatInfo	dateTimeFormatInfo;

		private System.Windows.Controls.Button		phBtn, tpoBtn;
		private System.Windows.Controls.TabItem		tabItem;
		private IGMarketProfileEnums.TradingHours	tradingHours;
		
		private Brush[]				TPOLetterColors;

		private TimeSpan			ts1000 = new TimeSpan(1, 0, 0, 0); // 1 day
		private SimpleFont			windings3Font;

		private const string		closeArrowString	= "";
		private const string		openArrowString		= "";
		private const string		tPOLetters			= "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"TPO and Volume Profile Chart.";
				Name										= "IGMarketProfile";
				Calculate									= Calculate.OnPriceChange;
				IsAutoScale									= false;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				//return;
				ArrowSize									= 8;
				CloseArrowColor								= Brushes.Fuchsia;
				DrawArrowsBool								= true;
				DrawIBBool									= true;
				DrawPOCBool									= true;
				DrawPriceHistogramBool						= true;
				DrawTPOBool									= false;
				DrawTPOIDBool								= true;
				DrawVirginPOCBool							= true;
				DrawVolumeHistogramBool						= true;
				ETHBeginTime								= DateTime.Parse("15:16");
				ETHEndTime									= DateTime.Parse("8:29");
				IB150Color									= Brushes.Transparent;
				IB200Color									= Brushes.Transparent;
				IB300Color									= Brushes.Transparent;
				IBColor										= Brushes.Red;
				IBOpenRangeSize								= 10;
				IBOpenRangeColor							= Brushes.Blue;
				IBOpacity									= 80;
				IBSize										= 2;
				IBWidth										= 4;
				MPSessionType								= IGMarketProfileEnums.SessionType.RTHAndETH;
				OpenArrowColor								= Brushes.Lime;
				PlotETHBool									= false;
				POCStroke									= new Stroke(Brushes.Blue, DashStyleHelper.Solid, 5);
				POCVAHStroke								= new Stroke(Brushes.Red, DashStyleHelper.Solid, 2);
				POCVALStroke								= new Stroke(Brushes.Green, DashStyleHelper.Solid, 2);
				PriceHistogramVAHColor						= Brushes.LightCoral;
				PriceHistogramVAColor						= Brushes.Yellow;
				PriceHistogramVALColor						= Brushes.LightGreen;
				PriceHistogramBorderColor					= Brushes.Black;
				PriceHistogramOpacity						= 80;
				RTHBeginTime								= DateTime.Parse("8:30");
				RTHEndTime									= DateTime.Parse("15:15");
				ShowBars									= true;				
				TPOColor									= Brushes.LightGray;
				TPOColorMode								= IGMarketProfileEnums.ColorMode.EachLetterDifferentColor;
				TPOFont										= new SimpleFont("Arial", 10);
				TPOSize										= 30;
				TPOSpaceBetweenLetters						= 4;
				TPOIDFont									= new SimpleFont("Tahoma", 8);
				TPOIDColor									= Brushes.LightGray;
				TPOIDYPixelOffset							= 50;
				ValueAreaSize								= 68;
				POCVAHStroke								= new Stroke(Brushes.Red, DashStyleHelper.Solid, 2);
				VirginPOCStroke								= new Stroke(Brushes.Blue, DashStyleHelper.Dash, 5);
				VirginPOCVAHStroke							= new Stroke(Brushes.Red, DashStyleHelper.Dash, 2);
				VirginPOCVALStroke							= new Stroke(Brushes.Green, DashStyleHelper.Dash, 2);
				VolumeHistogramBorderColor					= Brushes.Black;
				VolumeHistogramStroke						= new Stroke(Brushes.PaleGreen, DashStyleHelper.Solid, 34, 80);	
			}
			else if (State == State.DataLoaded)
			{
				SetZOrder(500);

				alMPHelper				= new List<MPHelper>(366 * 10);
				alMPHelperDay			= new List<MPHelper>(366 * 10);
				alMPHelperMonth			= new List<MPHelper>(12 * 10);
				alMPHelperWeek			= new List<MPHelper>(53 * 10);
				arrowXOffset			= DrawPriceHistogramBool ? -1 : 1;
				barsPerDay				= ((24 * 60) / BarsPeriod.Value);
				dateTimeFormatInfo		= DateTimeFormatInfo.CurrentInfo;
				iBDistance				= 2;
				invalidPeriodType		= false;
				letterIndex				= 25;
				mPHelperCurrentIndex	= 0;
				tradingHours = IGMarketProfileEnums.TradingHours.None;
				vol						= 0;
				windings3Font			= new SimpleFont("Wingdings 3", ArrowSize);

				TPOLetterColors			= new Brush[] {
					Brushes.Black, Brushes.CadetBlue, Brushes.Brown, Brushes.Navy, Brushes.Goldenrod, Brushes.Purple, Brushes.Peru, Brushes.SlateGray, Brushes.DarkRed, Brushes.Olive, Brushes.Blue, Brushes.IndianRed, Brushes.Green
				};
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					downColor			= ChartBars.Properties.ChartStyle.DownBrush;
					upColor				= ChartBars.Properties.ChartStyle.UpBrush;

					if (!ShowBars)
					{
						ChartBars.Properties.ChartStyle.DownBrush	= Brushes.Transparent;
						ChartBars.Properties.ChartStyle.UpBrush		= Brushes.Transparent;
					}

					CheckInstrumentAndBarType();

					if (!toolBarButtonsAdded)
					{
						ChartControl.Dispatcher.InvokeAsync((Action)(() =>
						{
							CreateWPFControls();
						}));
					}
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					if (ChartBars.Properties.ChartStyle.DownBrush == Brushes.Transparent)
					{
						ChartBars.Properties.ChartStyle.DownBrush	= downColor;
						ChartBars.Properties.ChartStyle.UpBrush		= upColor;
					}

					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						DisposeWPFControls();
					}));
				}
			}
		}

		/// <summary>
		/// The POC is the price at which the most TPO's have printed.
		/// If there is more than 1 price with the same 'most' TPO's then the price closest to the mid-point of the range (high - low) is used.
		/// If the 2 'most' TPO prices are equi-distance from the mid-point then the price on the side of the mid-point with the most TPO's is used.
		/// If there are equal number of TPO's on each side then the lower price is used.
		/// </summary>
		private void CalculatePOC(MPHelper helper)
		{
			if (mPHelperCurrentIndex > 0 && refreshTPO)
			{
				// 1st pass
				int intTPOs = 0;
				foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
					intTPOs = Math.Max(intTPOs, kvp.Value.Count);

				if (intTPOs > 0)
				{
					// 2nd pass
					List<string> lKeys = new List<string>();
					foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
					{
						if (intTPOs == kvp.Value.Count) lKeys.Add(kvp.Key);
					}

					if (lKeys.Count > 0)
					{
						// 3rd pass
						double dblMidPointOfTheRange = (helper.HighestHigh + helper.LowestLow) / 2;
						double dblClosest = double.MaxValue;
						foreach (string strKey in lKeys)
						{
							if (Math.Abs(dblMidPointOfTheRange - double.Parse(strKey)) < dblClosest)
							{
								dblClosest = Math.Abs(dblMidPointOfTheRange - double.Parse(strKey));
								helper.POCPrice = double.Parse(strKey);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Count the number of TPOs in the profile and take _intValueAreaSize% (= VA).
		/// Count the number of TPOS in the POC (= TPO count).
		/// Count the number of TPOS in the price above the POC and the price below the POC.
		/// If TPOs price above > TPOs price below then 
		/// 	add TPOs price above to TPO count 
		/// 	VAH = price above
		/// else If TPOs price above TPOs price below then
		/// 	add TPOs price below to TPO count
		/// 	VAL = price below
		/// else
		/// 	add TPOs price above to TPO count 
		/// 	add TPOs price below to TPO count
		/// 	VAH = price above
		/// 	VAL = price below
		/// </summary>
		private void CalculateVAHAndVAL(MPHelper helper)
		{
			if (mPHelperCurrentIndex > 0 && refreshTPO)
			{
				vA = 0;
				foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
					vA += kvp.Value.Count;

				vA			= (vA * ValueAreaSize) / 100;
				tPOCount	= helper.TPOLetters[helper.POCPrice.ToString()].Count;
				keyHigh		= helper.POCPrice + tickSize_x_TicksPerPlot;
				keyLow		= helper.POCPrice - tickSize_x_TicksPerPlot;

				string strKeyHigh = keyHigh.ToString();
				string strKeyLow = keyLow.ToString();

				while (helper.TPOLetters.ContainsKey(strKeyHigh) || helper.TPOLetters.ContainsKey(strKeyLow))
				{
					if (helper.TPOLetters.ContainsKey(strKeyHigh))
						tPOsAbove = helper.TPOLetters[strKeyHigh].Count;
					else
						tPOsAbove = 0;

					if (helper.TPOLetters.ContainsKey(strKeyLow))
						tPOsBelow = helper.TPOLetters[strKeyLow].Count;
					else
						tPOsBelow = 0;

					if (tPOsAbove > tPOsBelow)
					{
						tPOCount			+= tPOsAbove;
						helper.POCVAHPrice	= keyHigh + tickSize_x_TicksPerPlot;
						keyHigh				+= tickSize_x_TicksPerPlot;
						strKeyHigh			= keyHigh.ToString();
					}
					else if (tPOsAbove < tPOsBelow)
					{
						tPOCount			+= tPOsBelow;
						helper.POCVALPrice	= keyLow;
						keyLow				-= tickSize_x_TicksPerPlot;
						strKeyLow			= keyLow.ToString();
					}
					else
					{
						tPOCount			+= tPOsAbove + tPOsBelow;
						helper.POCVAHPrice	= keyHigh + tickSize_x_TicksPerPlot;
						helper.POCVALPrice	= keyLow;
						keyHigh				+= tickSize_x_TicksPerPlot;
						strKeyHigh			= keyHigh.ToString();
						keyLow				-= tickSize_x_TicksPerPlot;
						strKeyLow			= keyLow.ToString();
					}

					if (tPOCount >= vA)
						break;
				}
			}
		}

		private void CheckInstrumentAndBarType()
		{
			if (Bars.BarsPeriod.BarsPeriodType != Data.BarsPeriodType.Minute)
			{
				Draw.TextFixed(this, "errormsg1", "This Indicator only work on minute charts (5 minutes or less). Sorry for the inconvenience.", TextPosition.BottomLeft, ChartControl.Properties.ChartText, new SimpleFont("Tahoma", 21), ChartControl.Properties.AxisPen.Brush, Brushes.Transparent, 100);
				invalidPeriodType = true;
				return;
			}

			if (Bars.BarsPeriod.Value > 5)
			{
				Draw.TextFixed(this, "errormsg2", "This Indicator only work on 5 minute period or less. Sorry for the inconvenience.", TextPosition.BottomLeft, ChartControl.Properties.ChartText, new SimpleFont("Tahoma", 21), ChartControl.Properties.AxisPen.Brush, Brushes.Transparent, 100);
				invalidPeriodType = true;
				return;
			}

			if (ChartBars.Properties.ChartStyleType != ChartStyleType.CandleStick && ChartBars.Properties.ChartStyleType != ChartStyleType.OHLC)
			{
				Draw.TextFixed(this, "errormsg3", "This Indicator only work on CandleStick and OHLC charts. Sorry for the inconvenience.", TextPosition.BottomLeft, ChartControl.Properties.ChartText, new SimpleFont("Tahoma", 21), ChartControl.Properties.AxisPen.Brush, Brushes.Transparent, 100);
				invalidPeriodType = true;
				return;
			}
			
			switch (Instrument.MasterInstrument.InstrumentType)
			{
				case Cbi.InstrumentType.Forex:
					ticksPerPlotRange	= 2;
					break;
				case Cbi.InstrumentType.Future:
					ticksPerPlotRange	= 1;
					break;
				case Cbi.InstrumentType.Index:
					ticksPerPlotRange	= 10;
					break;
				case Cbi.InstrumentType.Option:
					ticksPerPlotRange	= 1;
					break;
				case Cbi.InstrumentType.Stock:
					if (High[0] < 89)
						ticksPerPlotRange	= 1;
					else if (High[0] < 377)
						ticksPerPlotRange	= 2;
					else if (High[0] < 610)
						ticksPerPlotRange	= 3;
					else
						ticksPerPlotRange	= 5;
					break;
				case Cbi.InstrumentType.Unknown:
					ticksPerPlotRange = 1;
					break;
				default:
					ticksPerPlotRange = 1;
					break;
			}

			tickSize_x_TicksPerPlot = TickSize * ticksPerPlotRange;
		}

		public void CreateWPFControls()
		{
			chartWindow	= Window.GetWindow(ChartControl.Parent) as Chart;

			tpoBtn = new System.Windows.Controls.Button()
			{
				Content		= "TPO",
				ToolTip		= "Display TPO",
				Background	= Brushes.PaleGreen,
				Foreground	= Brushes.Black
			};

			tpoBtn.Click += TpoBtn_Click;

			phBtn = new System.Windows.Controls.Button()
			{
				Content		= "PH",
				ToolTip		= "Display Price Histogram",
				Background	= Brushes.PaleGreen,
				Foreground	= Brushes.Black
			};

			phBtn.Click += PhBtn_Click;

			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private MPBar CreateTPOBar(TimeSpan begintime, TimeSpan endtime)
		{
			if (letterIndex >= (tPOLetters.Length - 1))
				letterIndex = -1;

			letterIndex++;

			endtime		= endtime > ts1000 ? endtime.Subtract(ts1000) : endtime;

			MPBar bar	= new MPBar(
				TPOColorMode == IGMarketProfileEnums.ColorMode.AllLettersSameColor ? TPOColor : TPOLetterColors[(letterIndex >= TPOLetterColors.Length - 1) ? letterIndex % TPOLetterColors.Length : letterIndex],
				begintime, endtime, Close[0], High[0], Low[0], Open[0], tPOLetters.Substring(letterIndex, 1), "");

			vol = 0;

			return bar;
		}

		private MPHelper CreateTPOSession(List<MPHelper> alMPHelper, IGMarketProfileEnums.TradingHours tradingHours)
		{
			mPHelperCurrentIndex += 1;

			MPHelper helper = new MPHelper(
				mPHelperCurrentIndex, tradingHours.ToString().Substring(0, 2) + mPHelperCurrentIndex.ToString(),
				tradingHours, Time[0].Date, CurrentBar,	CurrentBar,	Open[0], tradingHours == IGMarketProfileEnums.TradingHours.RTH ? RTHBeginTime.TimeOfDay : ETHBeginTime.TimeOfDay, tradingHours == IGMarketProfileEnums.TradingHours.RTH ? RTHEndTime.TimeOfDay : ETHEndTime.TimeOfDay.Add(new TimeSpan(0, (Time[0].DayOfWeek == DayOfWeek.Friday ? 120 : 0), 0))); // 120 : 0), 0)));

			switch (tradingHours)
			{
				case IGMarketProfileEnums.TradingHours.RTH:
					break;
				case IGMarketProfileEnums.TradingHours.ETH:
					break;
				case IGMarketProfileEnums.TradingHours.Day:
					helper.BeginTime	= RTHBeginTime.TimeOfDay;
					helper.EndTime		= ETHEndTime.TimeOfDay.Add(new TimeSpan(0, (Time[0].DayOfWeek == DayOfWeek.Friday ? 120 : 0), 0));
					break;
				case IGMarketProfileEnums.TradingHours.Week:
					helper.BeginTime	= RTHBeginTime.TimeOfDay;
					helper.EndTime		= ETHEndTime.TimeOfDay.Add(new TimeSpan(0, (Time[0].DayOfWeek == DayOfWeek.Friday ? 120 : 0), 0));
					break;
				case IGMarketProfileEnums.TradingHours.Month:
					helper.BeginTime	= RTHBeginTime.TimeOfDay;
					helper.EndTime		= ETHEndTime.TimeOfDay.Add(new TimeSpan(0, (Time[0].DayOfWeek == DayOfWeek.Friday ? 120 : 0), 0));
					break;
			}

			helper.IBEndTime		= helper.BeginTime.Add(new TimeSpan(0, (IBSize * TPOSize) - 0, 0));
			helper.Index			= alMPHelper.Count;
			helper.LetterIndex		= 99; //tradinghours == TradingHours.RTH ? 99 : this._alMPHelper.Count > 0 ? this._CurrentTPOSession.LetterIndex : 99;
			helper.OpenRangeEndTime	= helper.BeginTime.Add(new TimeSpan(0, IBOpenRangeSize - 0, 0));
			helper.OpenRangeHigh	= Math.Max(this.High[0], helper.OpenRangeHigh);
			helper.OpenRangeLow		= Math.Min(this.Low[0], helper.OpenRangeLow);
			helper.POCEndBar		= 0;

			return helper;
		}

		/// <summary>
		/// calculate auxiliary values to assist updating TPO letters and volume histogram
		/// </summary>
		private void CalculateAuxiliaryValues()
		{
			lowDecimal				= Low[0] % 1; // decimal part of Low
			floor					= (double)decimal.Truncate((decimal)Low[0]) + lowDecimal - (lowDecimal % tickSize_x_TicksPerPlot);
			highDecimal				= High[0] % 1; // decimal part of High
			ceiling					= (double)decimal.Truncate((decimal)High[0]) + highDecimal - (highDecimal % tickSize_x_TicksPerPlot);
			numberOfPlots			= (Int32)(((ceiling - floor) / tickSize_x_TicksPerPlot) + 1);
			newVolumePerPlotRange	= (Int32)(Math.Round(1.0 * Math.Abs(Volume[0] - vol) / numberOfPlots)); // update only new volume -> Math.Abs(dblVolume - this._dblVolume)
			vol						= Volume[0]; // save to only update new volume 
		}

		public override string DisplayName
		{
			get	{ return (State == State.DataLoaded || State == State.Historical || State == State.Realtime) ? string.Format("IGMarketProfile ({0} ({1} {2}))", Instrument.FullName, BarsPeriod.Value, BarsPeriod.BarsPeriodType) : "IGMarketProfile"; }
		}

		public void DisposeWPFControls()
		{
			if (phBtn != null)
				phBtn.Click -= PhBtn_Click;

			if (tpoBtn != null)
				tpoBtn.Click -= TpoBtn_Click;

			RemoveWPFControls();
		}

		// g2g
		private void DrawIB(MPHelper helper)
		{
			if (DrawIBBool && helper.TradingHours != IGMarketProfileEnums.TradingHours.ETH && helper.IBCompleted == false)
			{
				if (TimeBetweenExclusive(Time[0].TimeOfDay, helper.BeginTime, helper.IBEndTime))
				{
					helper.IBHigh	= Math.Max(High[0], helper.IBHigh);
					helper.IBLow	= Math.Min(Low[0], helper.IBLow);

					// IB
					Draw.Rectangle(this, "IB100" + helper.CurrentIndexString,
						IsAutoScale,
						CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
						helper.IBLow, // StartY
						CurrentBar - helper.StartBar + iBDistance, // end bars ago
						helper.IBHigh, // EndY
						Brushes.Transparent,
						IBColor,
						IBOpacity).OutlineStroke.Width = 1;

					// IB150
					if (IB150Color != Brushes.Transparent)
					{
						Draw.Rectangle(this, "IB150U" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBHigh, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBHigh + (helper.IBHigh - helper.IBLow) / 4, // EndY
							Brushes.Transparent,
							IB150Color,
							IBOpacity).OutlineStroke.Width = 1;
						Draw.Rectangle(this, "IB150L" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBLow, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBLow - (helper.IBHigh - helper.IBLow) / 4, // EndY
							Brushes.Transparent,
							IB150Color,
							IBOpacity).OutlineStroke.Width = 1;
					}
					// IB200
					if (IB200Color != Brushes.Transparent)
					{
						Draw.Rectangle(this, "IB200U" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBHigh + (helper.IBHigh - helper.IBLow) / 4, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBHigh + (helper.IBHigh - helper.IBLow) / 2, // EndY
							Brushes.Transparent,
							IB200Color,
							IBOpacity).OutlineStroke.Width = 1;
						Draw.Rectangle(this, "IB200L" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBLow - (helper.IBHigh - helper.IBLow) / 4, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBLow - (helper.IBHigh - helper.IBLow) / 2, // EndY
							Brushes.Transparent,
							IB200Color,
							IBOpacity).OutlineStroke.Width = 1;
					}
					// IB300
					if (IB300Color != Brushes.Transparent)
					{
						Draw.Rectangle(this, "IB300U" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBHigh + (helper.IBHigh - helper.IBLow) / 2, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBHigh + (helper.IBHigh - helper.IBLow) / 1, // EndY
							Brushes.Transparent,
							IB300Color,
							IBOpacity).OutlineStroke.Width = 1;
						Draw.Rectangle(this, "IB300L" + helper.CurrentIndexString,
							IsAutoScale,
							CurrentBar - helper.StartBar + IBWidth + iBDistance, // start bars ago
							helper.IBLow - (helper.IBHigh - helper.IBLow) / 2, // StartY
							CurrentBar - helper.StartBar + iBDistance, // end bars ago
							helper.IBLow - (helper.IBHigh - helper.IBLow) / 1, // EndY
							Brushes.Transparent,
							IB300Color,
							IBOpacity).OutlineStroke.Width = 1;
					}
				}
				else
				{
					helper.IBCompleted = true;
				}
			}

			if (helper.TradingHours == IGMarketProfileEnums.TradingHours.RTH && helper.IBCompleted == false)
			{
				// Open Range
				if (TimeBetweenExclusive(Time[0].TimeOfDay, helper.BeginTime, helper.OpenRangeEndTime))
				{
					helper.OpenRangeHigh	= Math.Max(this.High[0], helper.OpenRangeHigh);
					helper.OpenRangeLow		= Math.Min(this.Low[0], helper.OpenRangeLow);
					Draw.Rectangle(this, "OpenRange" + helper.CurrentIndexString,
						IsAutoScale,
						CurrentBar - helper.StartBar + IBWidth + IBWidth + iBDistance, // start bars ago
						helper.OpenRangeLow, // StartY
						CurrentBar - helper.StartBar + IBWidth + iBDistance, // end bars ago
						helper.OpenRangeHigh, // EndY
						Brushes.Transparent,
						IBOpenRangeColor,
						IBOpacity).OutlineStroke.Width = 1;
				}
			}
		}

		private void DrawOpenCloseArrows(MPHelper helper, MPBar bar)
		{
			if (DrawArrowsBool)
			{
				int intBarsAgo	= helper.TradingHours == IGMarketProfileEnums.TradingHours.RTH ? CurrentBar - helper.StartBar : CurrentBar - helper.FirstBar;
				Draw.Text(this, "ArrowOpen" + helper.CurrentIndexString, IsAutoScale, openArrowString, intBarsAgo + 1, helper.Open, 0, OpenArrowColor, windings3Font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				Draw.Text(this, "ArrowClose" + helper.CurrentIndexString, IsAutoScale, closeArrowString, intBarsAgo - helper.CloseBarEndBarsAgo + arrowXOffset, bar.Close, 0, CloseArrowColor, windings3Font, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
		}

		private void DrawPOC(MPHelper helper)
		{
			if (DrawPOCBool)
			{
				Draw.Line(this, "POC" + helper.CurrentIndexString,
					IsAutoScale,
					helper.POCStartBar,
					helper.POCPrice,
					helper.POCStartBar >= helper.POCEndBar ? helper.POCEndBar : 0,
					helper.POCPrice,
					POCStroke.Brush,
					POCStroke.DashStyleHelper,
					(int)POCStroke.Width);
				Draw.Line(this, "POCVAH" + helper.CurrentIndexString,
					IsAutoScale,
					helper.POCStartBar,
					helper.POCVAHPrice,
					helper.POCStartBar >= helper.POCEndBar ? helper.POCEndBar : 0,
					helper.POCVAHPrice,
					POCVAHStroke.Brush,
					POCVAHStroke.DashStyleHelper,
					(int)POCVAHStroke.Width);
				Draw.Line(this, "POCVAL" + helper.CurrentIndexString,
					IsAutoScale,
					helper.POCStartBar,
					helper.POCVALPrice,
					helper.POCStartBar >= helper.POCEndBar ? helper.POCEndBar : 0,
					helper.POCVALPrice,
					POCVALStroke.Brush,
					POCVALStroke.DashStyleHelper,
					(int)POCVALStroke.Width);

				helper.VirginPOCStartBar = CurrentBar;
			}
		}

		private void DrawPriceHistogram(MPHelper helper)
		{
			Brush colorPriceHistogram;
			double dblPrice;
			Int32 intBarsAgo	= helper.TradingHours == IGMarketProfileEnums.TradingHours.RTH ? CurrentBar - helper.StartBar : CurrentBar - helper.FirstBar;

			if (DrawPriceHistogramBool)
			{
				foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
				{
					dblPrice = double.Parse(kvp.Key);

					if (dblPrice == helper.POCPrice)
					{
						colorPriceHistogram		= PriceHistogramVAColor;
						helper.POCStartBar		= intBarsAgo - (kvp.Value.Count * TPOSpaceBetweenLetters) - 1;
					}
					else if (dblPrice >= helper.POCVAHPrice)
					{
						colorPriceHistogram		= PriceHistogramVAHColor;
					}
					else if (dblPrice < helper.POCVALPrice)
					{
						colorPriceHistogram		= PriceHistogramVALColor;
					}
					else
					{
						colorPriceHistogram		= PriceHistogramVAColor;
					}

					Draw.Rectangle(this, "PH" + helper.CurrentIndexString + kvp.Key,
						IsAutoScale, // autoscale
						intBarsAgo, // start bars ago
						dblPrice, // StartY
						intBarsAgo - (kvp.Value.Count * TPOSpaceBetweenLetters) + 1, // end bars ago
						dblPrice + tickSize_x_TicksPerPlot, // EndY
						PriceHistogramBorderColor,
						colorPriceHistogram,
						PriceHistogramOpacity).OutlineStroke.Width = 1;
				}
			}
			else
			{
				foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
				{
					dblPrice = double.Parse(kvp.Key);
					if (dblPrice == helper.POCPrice)
					{
						helper.POCStartBar = intBarsAgo - (kvp.Value.Count * TPOSpaceBetweenLetters) - 1;
						break;
					}
				}
			}
		}

		private void DrawTPO(MPHelper helper)
		{
			if (DrawTPOBool && refreshTPO)
			{
				double dblPrice;
				helper.TagsTPO = new List<string>(2584);
				Int32 int1 = 0;
				Int32 intBarsAgo = helper.TradingHours == IGMarketProfileEnums.TradingHours.RTH ? CurrentBar - helper.StartBar : CurrentBar - helper.FirstBar;

				foreach (KeyValuePair<string, List<TPOLetter>> kvp in helper.TPOLetters)
				{
					dblPrice = double.Parse(kvp.Key);
					int1 = 0;
					kvp.Value.ForEach(delegate (TPOLetter letter)
					{
						Draw.Text(this, string.Format("TPO{0}{1}{2}", helper.CurrentIndexString, kvp.Key, int1), // tag 
						IsAutoScale, // auto scale
						letter.Letter, // text
						intBarsAgo - (int1 * TPOSpaceBetweenLetters), // barsAgo
						dblPrice, // y
						0, // yPixelOffset
						letter.LetterColor, // textColor
						TPOFont, // font
						TextAlignment.Center,
						Brushes.Transparent,
						Brushes.Transparent,
						0);
						helper.TagsTPO.Add(string.Format("TPO{0}{1}{2}", helper.CurrentIndexString, kvp.Key, int1));
						int1++;
					});
				}
			}
		}

		private void DrawTPOID(MPHelper helper)
		{
			if (DrawTPOIDBool && refreshTPO)
			{
				int intBarsAgo = helper.TradingHours == IGMarketProfileEnums.TradingHours.RTH ? CurrentBar - helper.StartBar : CurrentBar - helper.FirstBar;

				Draw.Text(this, "TPOID" + helper.CurrentIndexString, // tag 
					IsAutoScale, // auto scale
					helper.ID, // text
					intBarsAgo, // barsAgo
					helper.HighestHigh, // y
					TPOIDYPixelOffset, // yPixelOffset
					TPOIDColor, // textColor
					TPOIDFont, // font
					TextAlignment.Left,
					Brushes.Transparent,
					Brushes.Transparent,
					0);
			}
		}

		private void DrawVirginPOC(List<MPHelper> alMPHelper)
		{
			if (DrawVirginPOCBool)
			{
				foreach (MPHelper helper in alMPHelper)
				{
					if (helper.POCCompleted && (helper.VirginVAHCompleted == false || helper.VirginPOCCompleted == false || helper.VirginVALCompleted == false))
					{
						if (helper.VirginVAHCompleted == false)
						{
							Draw.Line(this, "VPOCVAH" + helper.CurrentIndexString,
								IsAutoScale,
								CurrentBar - helper.VirginPOCStartBar,
								helper.POCVAHPrice,
								0,
								helper.POCVAHPrice,
								VirginPOCVAHStroke.Brush,
								VirginPOCVAHStroke.DashStyleHelper,
								(int)VirginPOCVAHStroke.Width);
							helper.VirginVAHCompleted = helper.POCVAHPrice >= Low[0] && helper.POCVAHPrice <= High[0];
						}
						if (helper.VirginPOCCompleted == false)
						{
							Draw.Line(this, "VPOC" + helper.CurrentIndexString,
								IsAutoScale,
								CurrentBar - helper.VirginPOCStartBar,
								helper.POCPrice,
								0,
								helper.POCPrice,
								VirginPOCStroke.Brush,
								VirginPOCStroke.DashStyleHelper,
								(int)VirginPOCStroke.Width);
							helper.VirginPOCCompleted = helper.POCPrice >= Low[0] && helper.POCPrice <= High[0];
						}
						if (helper.VirginVALCompleted == false)
						{
							Draw.Line(this, "VPOCVAL" + helper.CurrentIndexString,
								IsAutoScale,
								CurrentBar - helper.VirginPOCStartBar,
								helper.POCVALPrice,
								0,
								helper.POCVALPrice,
								VirginPOCVALStroke.Brush,
								VirginPOCVALStroke.DashStyleHelper,
								(int)VirginPOCVALStroke.Width);
							helper.VirginVALCompleted = helper.POCVALPrice >= Low[0] && helper.POCVALPrice <= High[0];
						}
					}
				}
			}
		}

		private void DrawVolumeHistogram(MPHelper helper)
		{
			if (DrawVolumeHistogramBool && refreshTPO)
			{
				double dblVolumePercent;
				int intMaxBarsAgo = helper.POCStartBar - 3;

				foreach (KeyValuePair<string, int> kvp in helper.VolumeHistogram)
				{
					dblVolumePercent = (1.0 * (int)kvp.Value / helper.MaxVolume) / 1;
					Draw.Rectangle(
						this, "VH" + helper.CurrentIndexString + kvp.Key,
						IsAutoScale,
						intMaxBarsAgo, // StartBarsAgo
						double.Parse(kvp.Key), // StartYBase,
						intMaxBarsAgo - (int)Math.Round(VolumeHistogramStroke.Width * dblVolumePercent), // EndBarsAgo - The end bar (x axis co-ordinate) where the draw object will terminate
						(double.Parse(kvp.Key) + tickSize_x_TicksPerPlot),
						VolumeHistogramBorderColor,
						VolumeHistogramStroke.Brush,
						VolumeHistogramStroke.Opacity).OutlineStroke.Width = 1;

				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (invalidPeriodType)
				return;

			CandleOutlineBrush = ShowBars ? candleOutlineColor : Brushes.Transparent;

			// CREATE SPECIFIC METHOD - DO NOT PASS PARAMETERS
			if (TimeBetweenExclusive(Time[0].TimeOfDay, RTHBeginTime.TimeOfDay, RTHEndTime.TimeOfDay.Add(new TimeSpan(0, (Time[0].DayOfWeek == DayOfWeek.Friday ? 120 : 0), 0)))) // 120 : 0), 0)))) // inside regular trading hours - RTH
			{
				if (tradingHours != IGMarketProfileEnums.TradingHours.RTH)
				{
					if (mPHelperCurrentIndex > 0)
						currentTPOSession.POCCompleted = true;

					tradingHours = IGMarketProfileEnums.TradingHours.RTH;
					letterIndex = 99;

					// RTH
					currentTPOSession	= CreateTPOSession(alMPHelper, IGMarketProfileEnums.TradingHours.RTH);
					currentTPOBar		= CreateTPOBar(RTHBeginTime.TimeOfDay, RTHBeginTime.TimeOfDay.Add(new TimeSpan(0, TPOSize, 0)));
					currentTPOSession.MPBars.Add(currentTPOBar);
					alMPHelper.Add(currentTPOSession);

					// day session
					if (mPHelperCurrentIndex > 0 && currentTPOSessionDay != null)
						currentTPOSessionDay.POCCompleted = true;
					currentTPOSessionDay	= CreateTPOSession(alMPHelperDay, IGMarketProfileEnums.TradingHours.Day);
					currentTPOSessionDay.MPBars.Add(currentTPOBar);
					alMPHelperDay.Add(currentTPOSessionDay);

					// week session
					if (dateTimeFormatInfo.Calendar.GetWeekOfYear(Time[0].Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) != previousWeek)
					{
						if (mPHelperCurrentIndex > 0 && currentTPOSessionWeek != null)
							currentTPOSessionWeek.POCCompleted = true;

						currentTPOSessionWeek	= CreateTPOSession(alMPHelperWeek, IGMarketProfileEnums.TradingHours.Week);
						currentTPOSessionWeek.MPBars.Add(currentTPOBar);
						alMPHelperWeek.Add(currentTPOSessionWeek);
						previousWeek			= dateTimeFormatInfo.Calendar.GetWeekOfYear(Time[0].Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
					}

					// month session
					if (Time[0].Month != previousMonth)
					{
						if (mPHelperCurrentIndex > 0 && currentTPOSessionMonth != null)
							currentTPOSessionMonth.POCCompleted = true;

						currentTPOSessionMonth	= CreateTPOSession(alMPHelperMonth, IGMarketProfileEnums.TradingHours.Month);
						currentTPOSessionMonth.MPBars.Add(currentTPOBar);
						alMPHelperMonth.Add(currentTPOSessionMonth);
						previousMonth			= Time[0].Month;
					}
				}
			}

			// CREATE SPECIFIC METHOD
			else if (TimeBetweenExclusive(Time[0].TimeOfDay, ETHBeginTime.TimeOfDay, ETHEndTime.TimeOfDay)) // inside extended trading hours - ETH
			{
				if (tradingHours != IGMarketProfileEnums.TradingHours.ETH)
				{
					if (mPHelperCurrentIndex > 0)
						currentTPOSession.POCCompleted = true;

					tradingHours		= IGMarketProfileEnums.TradingHours.ETH;
					letterIndex			= 99;
					// ETH
					currentTPOSession	= CreateTPOSession(alMPHelper, IGMarketProfileEnums.TradingHours.ETH);
					currentTPOBar		= CreateTPOBar(ETHBeginTime.TimeOfDay, ETHBeginTime.TimeOfDay.Add(new TimeSpan(0, TPOSize, 0)));
					currentTPOSession.MPBars.Add(currentTPOBar);
					alMPHelper.Add(currentTPOSession);

					// week session
					if (dateTimeFormatInfo.Calendar.GetWeekOfYear(Time[0].Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) != previousWeek)
					{
						if (mPHelperCurrentIndex > 0 && currentTPOSessionWeek != null)
							currentTPOSessionWeek.POCCompleted = true;
						currentTPOSessionWeek = CreateTPOSession(alMPHelperWeek, IGMarketProfileEnums.TradingHours.Week);
						currentTPOSessionWeek.MPBars.Add(currentTPOBar);
						alMPHelperWeek.Add(currentTPOSessionWeek);
						previousWeek = dateTimeFormatInfo.Calendar.GetWeekOfYear(Time[0].Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
					}

					// month session
					if (Time[0].Month != previousMonth)
					{
						if (mPHelperCurrentIndex > 0 && currentTPOSessionMonth != null)
							currentTPOSessionMonth.POCCompleted = true;

						currentTPOSessionMonth = CreateTPOSession(alMPHelperMonth, IGMarketProfileEnums.TradingHours.Month);
						currentTPOSessionMonth.MPBars.Add(currentTPOBar);
						alMPHelperMonth.Add(currentTPOSessionMonth);
						previousMonth = Time[0].Month;
					}
				}
			}

			if (TimeBetweenExclusiveBar() == false)
			{
				if (mPHelperCurrentIndex > 0)
				{
					currentTPOBar = CreateTPOBar(currentTPOBar.EndTime, currentTPOBar.EndTime.Add(new TimeSpan(0, TPOSize, 0)));
					currentTPOSession.MPBars.Add(currentTPOBar);
					if (currentTPOSessionDay != null)
						currentTPOSessionDay.MPBars.Add(currentTPOBar);
					currentTPOSessionWeek.MPBars.Add(currentTPOBar);
					currentTPOSessionMonth.MPBars.Add(currentTPOBar);
				}
			}

			CalculateAuxiliaryValues();
			UpdateCurrentTPOSession(currentTPOSession);
			UpdateCurrentTPOBar(currentTPOBar);
			refreshTPO = false;
			UpdateTPOLetters(currentTPOSession);
			UpdateVolumeHistogram(currentTPOSession);
			CalculatePOC(currentTPOSession);
			CalculateVAHAndVAL(currentTPOSession);

			if (isBusy)
				return;

			if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH && currentTPOSession.TradingHours == IGMarketProfileEnums.TradingHours.ETH && PlotETHBool == false)
				return;

			if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH && alMPHelper.Count > 1)
			{
				DrawIB(currentTPOSession);
				DrawPriceHistogram(currentTPOSession);
				DrawOpenCloseArrows(currentTPOSession, currentTPOBar);
				DrawTPO(currentTPOSession);
				DrawTPOID(currentTPOSession);
				DrawVolumeHistogram(currentTPOSession);
				DrawPOC(currentTPOSession);
				DrawVirginPOC(alMPHelper);
			}
			else if (MPSessionType == IGMarketProfileEnums.SessionType.Day && alMPHelperDay.Count > 1) // && this._CurrentTPOSessionDay != null)
			{
				DrawIB(currentTPOSessionDay);
				DrawPriceHistogram(currentTPOSessionDay);
				DrawOpenCloseArrows(currentTPOSessionDay, currentTPOBar);
				DrawTPO(currentTPOSessionDay);
				DrawTPOID(currentTPOSessionDay);
				DrawVolumeHistogram(currentTPOSessionDay);
				DrawPOC(currentTPOSessionDay);
				DrawVirginPOC(alMPHelperDay);
			}
			else if (MPSessionType == IGMarketProfileEnums.SessionType.Week && alMPHelperWeek.Count > 1)
			{
				DrawIB(currentTPOSessionWeek);
				DrawPriceHistogram(currentTPOSessionWeek);
				DrawOpenCloseArrows(currentTPOSessionWeek, currentTPOBar);
				DrawTPO(currentTPOSessionWeek);
				DrawTPOID(currentTPOSessionWeek);
				DrawVolumeHistogram(currentTPOSessionWeek);
				DrawPOC(currentTPOSessionWeek);
				DrawVirginPOC(alMPHelperWeek);
			}
		}

		public void InsertWPFControls()
		{
			if (panelActive)
				return;

			// add the menu which contains all menu items to the chart
			chartWindow.MainMenu.Add(tpoBtn);
			chartWindow.MainMenu.Add(phBtn);			

			panelActive = true;
		}

		private void PhBtn_Click(object sender, RoutedEventArgs e)
		{
			isBusy					= true;
			DrawPriceHistogramBool	= !DrawPriceHistogramBool;

			if (DrawPriceHistogramBool)
			{
				if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH)
				{
					refreshTPO = true;

					TriggerCustomEvent((ob) =>
					{
						foreach (MPHelper helper in alMPHelper)
							DrawPriceHistogram(helper);
					}, 0, null);
				}
			}
			else
			{
				if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH)
				{
					foreach (MPHelper helper in alMPHelper)
					{
						TriggerCustomEvent((ob) =>
						{
							foreach (string tag in helper.TagsPriceHistogram)
								RemoveDrawObject(tag);
						}, 0, null);
					}
				}
			}

			phBtn.ToolTip		= DrawPriceHistogramBool ? "Hide Price Histogram." : "Display Price Histogram.";
			phBtn.Background	= DrawPriceHistogramBool ? Brushes.LightGreen : Brushes.LightSlateGray;
			phBtn.Foreground	= DrawPriceHistogramBool ? Brushes.Black : Brushes.White;
			isBusy				= false;

			ForceRefresh();
		}

		public void RemoveWPFControls()
		{
			if (!panelActive)
				return;

			if (phBtn != null)
				chartWindow.MainMenu.Remove(phBtn);

			if (tpoBtn != null)
				chartWindow.MainMenu.Remove(tpoBtn);

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}

		private bool TimeBetween(TimeSpan tsInputTime, TimeSpan tsStartTime, TimeSpan tsEndTime)
		{
			if (tsStartTime < tsEndTime)
				return ((tsInputTime >= tsStartTime) && (tsInputTime < tsEndTime));
			else
				return ((tsInputTime >= tsStartTime && tsInputTime <= new TimeSpan(23, 59, 59)) || (tsInputTime >= new TimeSpan(0, 0, 0) && tsInputTime < tsEndTime));
		}

		private bool TimeBetweenExclusive(TimeSpan tsInputTime, TimeSpan tsStartTime, TimeSpan tsEndTime)
		{
			if (tsStartTime < tsEndTime) // && tsInputTime != ts000)
				return ((tsInputTime > tsStartTime) && (tsInputTime <= tsEndTime));
			else
				return ((tsInputTime > tsStartTime && tsInputTime <= new TimeSpan(23, 59, 59)) || (tsInputTime >= new TimeSpan(0, 0, 0) && tsInputTime < tsEndTime));
		}

		private bool TimeBetweenExclusiveBar()
		{
			if (currentTPOBar.StartTime < currentTPOBar.EndTime) // && tsInputTime != ts000)
				return ((Time[0].TimeOfDay > currentTPOBar.StartTime) && (Time[0].TimeOfDay <= currentTPOBar.EndTime));
			else
				return ((Time[0].TimeOfDay > currentTPOBar.StartTime && Time[0].TimeOfDay <= new TimeSpan(23, 59, 59)) || (Time[0].TimeOfDay >= new TimeSpan(0, 0, 0) && Time[0].TimeOfDay < currentTPOBar.EndTime));
		}

		private bool TimeBetweenInclusive(TimeSpan tsInputTime, TimeSpan tsStartTime, TimeSpan tsEndTime)
		{
			if (tsStartTime < tsEndTime)
				return ((tsInputTime >= tsStartTime) && (tsInputTime <= tsEndTime));
			else
			{
				return ((tsInputTime >= tsStartTime && tsInputTime <= new TimeSpan(23, 59, 59)) || (tsInputTime >= new TimeSpan(0, 0, 0) && tsInputTime <= tsEndTime));
			}
		}

		private void TpoBtn_Click(object sender, RoutedEventArgs e)
		{
			isBusy			= true;
			DrawTPOBool		= !DrawTPOBool;

			if (DrawTPOBool)
			{
				if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH)
				{
					refreshTPO = true;

					TriggerCustomEvent((ob) =>
					{
						foreach (MPHelper helper in alMPHelper)
							DrawTPO(helper);

						Draw.Dot(this, "temp", false, 0, 0, Brushes.Transparent);
						RemoveDrawObject("temp");
						isBusy	= false;
					}, 0, null);
				}
			}
			else
			{
				if (MPSessionType == IGMarketProfileEnums.SessionType.RTHAndETH)
				{
					foreach (MPHelper helper in alMPHelper)
					{
						TriggerCustomEvent((ob) =>
						{
							foreach (string tag in helper.TagsTPO)
								RemoveDrawObject(tag);
						}, 0, null);
					}
				}
			}

			tpoBtn.ToolTip		= DrawTPOBool ? "Hide TPO" : "Display TPO";
			tpoBtn.Background	= DrawTPOBool ? Brushes.LightGreen : Brushes.LightSlateGray;
			tpoBtn.Foreground	= DrawTPOBool ? Brushes.Black : Brushes.White;

			ForceRefresh();
		}

		private void UpdateCurrentTPOBar(MPBar bar)
		{
			bar.Close	= Close[0];
			bar.High	= Math.Max(High[0], currentTPOBar.High);
			bar.Low		= Math.Min(Low[0], currentTPOBar.Low);
		}

		private void UpdateCurrentTPOSession(MPHelper helper)
		{
			helper.Close		= Close[0];
			helper.HighestHigh	= Math.Max(High[0], helper.HighestHigh);
			helper.LowestLow	= Math.Min(Low[0], helper.LowestLow);
		}


		private void UpdateTPOLetters(MPHelper helper)
		{
			//Color colorLetter = helper.MPBars[helper.MPBars.Count - 1].Color;
			double		dblKey; // CHANGE TO CLASS VARIABLE TO IMPROVE PERFORMANCE
			int			intCount;
			string		strKey;
			string		strLetter	= helper.MPBars[helper.MPBars.Count - 1].Letter;
			TPOLetter	tpoletter;

			for (Int32 int1 = 0; int1 < numberOfPlots; int1++)
			{
				dblKey = floor + (int1 * tickSize_x_TicksPerPlot);
				strKey = dblKey.ToString();
				if (helper.TPOLetters.ContainsKey(strKey) == false)
				{
					helper.TPOLetters.Add(strKey, new List<TPOLetter>(233));

					helper.TPOLetters[strKey].Add(new TPOLetter(helper.MPBars[helper.MPBars.Count - 1].Color, strLetter));

					helper.TotalTPOCount += 1;
					helper.TagsPriceHistogram.Add("PH" + helper.CurrentIndexString + strKey);
					refreshTPO = true;
				}
				else
				{
					tpoletter = (helper.TPOLetters[strKey])[(helper.TPOLetters[strKey]).Count - 1];
					if (tpoletter.Letter != strLetter)
					{
						(helper.TPOLetters[strKey]).Add(new TPOLetter(helper.MPBars[helper.MPBars.Count - 1].Color, strLetter));
						helper.TotalTPOCount += 1;
						refreshTPO = true;
					}
				}

				if (dblKey <= Close[0])
				{
					intCount = (helper.TPOLetters[strKey]).Count;
					helper.CloseBarEndBarsAgo = DrawTPOBool ? (intCount * TPOSpaceBetweenLetters) + TPOSpaceBetweenLetters - 1 : (intCount * TPOSpaceBetweenLetters) + TPOSpaceBetweenLetters - 1; // intCount + 1;
				}
			}
		}

		private void UpdateVolumeHistogram(MPHelper helper)
		{
			if (DrawVolumeHistogramBool)
			{
				string strKey;
				for (int i = 0; i < numberOfPlots; i++)
				{
					strKey = (floor + (i * tickSize_x_TicksPerPlot)).ToString();
					if (helper.VolumeHistogram.ContainsKey(strKey) == false)
					{
						helper.VolumeHistogram.Add(strKey, 0);
						helper.TagsVolumeHistogram.Add("VH" + helper.CurrentIndexString + strKey);
					}
					helper.VolumeHistogram[strKey] += newVolumePerPlotRange; // update volume
					helper.MaxVolume = Math.Max(helper.MaxVolume, (Int32)helper.VolumeHistogram[strKey]);
				}
			}
		}

		public class MPBar
		{
			public Brush	Color;
			public TimeSpan	StartTime;
			public TimeSpan EndTime;
			public double	Close;
			public double	High;
			public double	Low;
			public double	Open;
			public string	Letter;
			public string	Tag;

			public MPBar(Brush _color, TimeSpan _starttime, TimeSpan _endtime, double _close, double _high, double _low, double _open, string _letter, string _tag)
			{
				Color		= _color;
				StartTime	= _starttime;
				EndTime		= _endtime;
				Close		= _close;
				High		= _high;
				Low			= _low;
				Open		= _open;
				Letter		= _letter;
				Tag			= _tag;
			}
		}

		public class MPHelper
		{
			public bool			IBCompleted				= false;
			public bool			OpenRangeCompleted		= false;

			public bool			VAHCompleted			= false;
			public bool			VALCompleted			= false;
			public bool			POCCompleted			= false;

			public bool			VirginVAHCompleted		= false;
			public bool			VirginVALCompleted		= false;
			public bool			VirginPOCCompleted		= false;

			public DateTime		BeginDate;
			public DateTime		EndDate;

			public double		Close;
			public double		High;
			public double		HighestHigh				= double.MinValue;
			public double		IBHigh					= double.MinValue;
			public double		IBLow					= double.MaxValue;
			public double		Low;
			public double		LowestLow				= double.MaxValue;
			public double		Open;
			public double		OpenRangeHigh			= double.MinValue;
			public double		OpenRangeLow			= double.MaxValue;
			public double		POCPrice;
			public double		POCVAHPrice;
			public double		POCVALPrice;

			public int			CloseBarEndBarsAgo;
			public int			CurrentIndex;

			/// TPO draw end bar
			public int			EndBar;

			/// TPO first price bar
			public int			FirstBar; // DON'T NEED	 
			public int			Index;

			/// TPO last price bar
			public int			LastBar; // DON'T NEED		  
			public int			LetterIndex;

			public int			MaxVolume				= 0;
			public int			POCStartBar; // POC line start bar
			public int			POCEndBar				= 0; // POC line end bar
			public int			StartBar; // TPO draw start bar 
			public int			TotalTPOCount			= 0; // TPO draw start bar 
			public int			VirginPOCStartBar; // Virgin POC line start bar

			public List<MPBar>	MPBars					= new List<MPBar>(89);

			public List<string>	TagsArrows				= new List<string>(2);
			public List<string>	TagsIB					= new List<string>(13);
			public List<string>	TagsLines				= new List<string>(21);
			public List<string>	TagsPOC					= new List<string>(13);
			public List<string>	TagsPriceHistogram		= new List<string>(377);
			public List<string>	TagsTPO					= new List<string>(2584);
			public List<string>	TagsVolumeHistogram		= new List<string>(377);

			/// <summary>
			/// A string Dictionary can be optimized. This requires a small change that requires no algorithmic analysis. The Dictionary provides 
			/// a way to specify an ordinal-based string comparer. This results in a much faster Dictionary with string keys.
			/// By using the StringComparer.Ordinal class, you tell the Dictionary to perform the fastest ordinal comparisons on the string 
			/// characters. This improves performance on all lookups. Also, lookups occur when adding elements to a Dictionary.
			/// </summary>
			public Dictionary<string, List<TPOLetter>>	TPOLetters = new Dictionary<string, List<TPOLetter>>(StringComparer.Ordinal); // key=price, value=letter array

			public Dictionary<string, int>	VolumeHistogram = new Dictionary<string, Int32>(StringComparer.Ordinal); // key=price, value=volume

			public string		CurrentIndexString;
			public string		ID;

			public TimeSpan		BeginTime;
			public TimeSpan		EndTime;
			public TimeSpan		IBEndTime;
			public TimeSpan		OpenRangeEndTime;

			public IGMarketProfileEnums.TradingHours TradingHours;

			public MPHelper(Int32 _CurrentIndex, string _CurrentIndexString, IGMarketProfileEnums.TradingHours _TradingHours, DateTime _BeginDate, int _FirstBar, int _StarttBar, double _Open, TimeSpan _BeginTime, TimeSpan _EndTime)
			{
				CurrentIndex		= _CurrentIndex;
				CurrentIndexString	= _CurrentIndexString;
				TradingHours		= _TradingHours;
				BeginDate			= _BeginDate;
				FirstBar			= _FirstBar;
				StartBar			= _StarttBar;
				Open				= _Open;
				BeginTime			= _BeginTime;
				EndTime				= _EndTime;

				ID = string.Format("{0} {1}/{2}-{3}", TradingHours, BeginDate.Month, BeginDate.Day.ToString(),    BeginDate.DayOfWeek.ToString().Substring(0, 3));

				TagsArrows.Add("ArrowOpen" + CurrentIndexString);
				TagsArrows.Add("ArrowClose" + CurrentIndexString);

				foreach (string tag in new string[] { "IB100", "IB150U", "IB150L", "IB200U", "IB200L", "IB300U", "IB300L", "OpenRange", "POCVAH", "POC", "POCVAL", "VPOCVAH", "VPOC", "VPOCVAL" })
					TagsIB.Add(tag + CurrentIndexString);
				
			}
		}

		public class TPOLetter
		{
			public string	Letter;
			public Brush	LetterColor;

			public TPOLetter(Brush _LetterColor, string _Letter)
			{
				LetterColor	= _LetterColor;
				Letter		= _Letter;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Arrow Size", Order = 25, GroupName = "TPO", Description = "Draw open and close arrows.")]
		public int ArrowSize
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Close Arrow Color", Order = 27, GroupName = "TPO")]
		public Brush CloseArrowColor
		{ get; set; }

		[Browsable(false)]
		public string CloseArrowColorSerializable
		{
			get { return Serialize.BrushToString(CloseArrowColor); }
			set { CloseArrowColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Draw Arrows", Order = 24, GroupName = "TPO")]
		public bool DrawArrowsBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw IB", Order = 1, GroupName = "IB")]
		public bool DrawIBBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw POC", Order = 1, GroupName = "POC")]
		public bool DrawPOCBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw Price Histogram", Order = 1, GroupName = "Price Histogram")]
		public bool DrawPriceHistogramBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw TPO", Order = 1, GroupName = "TPO")]
		public bool DrawTPOBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw TPO ID", Order = 28, GroupName = "TPO")]
		public bool DrawTPOIDBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw Virgin POC", Order = 1, GroupName = "Virgin POC")]
		public bool DrawVirginPOCBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Draw Volume Histogram Bool", Order = 1, GroupName = "Volume Histogram")]
		public bool DrawVolumeHistogramBool
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "ETH Begin Time", Order = 4, GroupName = "Session", Description = "ETH Begin Time on your chart (hhmm, 0-2359, military time). Dont't forget to change RTH Begin Time, RTH End Time, ETH Begin Time, and ETH End Time accordingly.")]
		public DateTime ETHBeginTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "ETH End Time", Order = 5, GroupName = "Session", Description = "ETH End Time on your chart (hhmm, 0-2359, military time). Dont't forget to change RTH Begin Time, RTH End Time, ETH Begin Time, and ETH End Time accordingly.")]
		public DateTime ETHEndTime
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "IB 150 Color", Order = 12, GroupName = "IB")]
		public Brush IB150Color
		{ get; set; }

		[Browsable(false)]
		public string IB150ColorSerializable
		{
			get { return Serialize.BrushToString(IB150Color); }
			set { IB150Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "IB 200 Color", Order = 13, GroupName = "IB")]
		public Brush IB200Color
		{ get; set; }

		[Browsable(false)]
		public string IB200ColorSerializable
		{
			get { return Serialize.BrushToString(IB200Color); }
			set { IB200Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "IB 300 Color", Order = 14, GroupName = "IB")]
		public Brush IB300Color
		{ get; set; }

		[Browsable(false)]
		public string IB300ColorSerializable
		{
			get { return Serialize.BrushToString(IB300Color); }
			set { IB300Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "IB Color", Order = 11, GroupName = "IB")]
		public Brush IBColor
		{ get; set; }

		[Browsable(false)]
		public string IBColorSerializable
		{
			get { return Serialize.BrushToString(IBColor); }
			set { IBColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Open Range Color", Order = 16, GroupName = "IB")]
		public Brush IBOpenRangeColor
		{ get; set; }

		[Browsable(false)]
		public string IBOpenRangeColorSerializable
		{
			get { return Serialize.BrushToString(IBOpenRangeColor); }
			set { IBOpenRangeColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Open Range Size", Order = 15, GroupName = "IB", Description = "Open Range Size in minutes (5-60).")]
		public int IBOpenRangeSize
		{ get; set; }	

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Opacity", Order = 17, GroupName = "IB")]
		public int IBOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Size", Order = 10, GroupName = "IB")]
		public int IBSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Width", Order = 18, GroupName = "IB", Description = "IB Plot Width (1-5).")]
		public int IBWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Session Type", Order = 6, GroupName = "Session")]
		public IGMarketProfileEnums.SessionType MPSessionType
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Open Arrow Color", Order = 26, GroupName = "TPO")]
		public Brush OpenArrowColor
		{ get; set; }

		[Browsable(false)]
		public string OpenArrowSerializable
		{
			get { return Serialize.BrushToString(OpenArrowColor); }
			set { OpenArrowColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Plot ETH", Order = 3, GroupName = "Session", Description = "Plot ETH?")]
		public bool PlotETHBool
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "POC Stroke", Order = 2, GroupName = "POC")]
		public Stroke POCStroke
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "VAH Stroke", Order = 3, GroupName = "POC")]
		public Stroke POCVAHStroke
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "VAL Stroke", Order = 4, GroupName = "POC")]
		public Stroke POCVALStroke
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Price Histogram Border Color", Order = 5, GroupName = "Price Histogram")]
		public Brush PriceHistogramBorderColor
		{ get; set; }

		[Browsable(false)]
		public string PriceHistogramBorderColorSerializable
		{
			get { return Serialize.BrushToString(PriceHistogramBorderColor); }
			set { PriceHistogramBorderColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Price Histogram Opacity", Order = 2, GroupName = "Price Histogram")]
		public int PriceHistogramOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Price Histogram VA Color", Order = 3, GroupName = "Price Histogram")]
		public Brush PriceHistogramVAColor
		{ get; set; }

		[Browsable(false)]
		public string PriceHistogramVAColorSerializable
		{
			get { return Serialize.BrushToString(PriceHistogramVAColor); }
			set { PriceHistogramVAColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Price Histogram VAH Color", Order = 4, GroupName = "Price Histogram")]
		public Brush PriceHistogramVAHColor
		{ get; set; }

		[Browsable(false)]
		public string PriceHistogramVAHColorSerializable
		{
			get { return Serialize.BrushToString(PriceHistogramVAHColor); }
			set { PriceHistogramVAHColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Price Histogram VAL Color", Order = 5, GroupName = "Price Histogram")]
		public Brush PriceHistogramVALColor
		{ get; set; }

		[Browsable(false)]
		public string PriceHistogramVALColorSerializable
		{
			get { return Serialize.BrushToString(PriceHistogramVALColor); }
			set { PriceHistogramVALColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "RTH Begin Time", Order = 1, GroupName = "Session", Description = "RTH Begin Time on your chart (hhmm, 0-2359, military time). Dont't forget to change RTH Begin Time, RTH End Time, ETH Begin Time, and ETH End Time accordingly.")]
		public DateTime RTHBeginTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "RTH End Time", Order = 2, GroupName = "Session", Description = "RTH End Time on your chart (hhmm, 0-2359, military time). Dont't forget to change RTH Begin Time, RTH End Time, ETH Begin Time, and ETH End Time accordingly.")]
		public DateTime RTHEndTime
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Show Bars", Order = 8, GroupName = "Session", Description = "IB Size in TPO units (see 01 Session, item 08, 1-5).")]
		public bool ShowBars
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TPO Color", Order = 22, GroupName = "TPO")]
		public Brush TPOColor
		{ get; set; }

		[Browsable(false)]
		public string TPOColorSerializable
		{
			get { return Serialize.BrushToString(TPOColor); }
			set { TPOColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TPO Color Mode", Order = 23, GroupName = "TPO")]
		public IGMarketProfileEnums.ColorMode TPOColorMode
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TPO Font", Order = 20, GroupName = "TPO")]
		public SimpleFont TPOFont
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TPO ID", Order = 30, GroupName = "TPO")]
		public Brush TPOIDColor
		{ get; set; }

		[Browsable(false)]
		public string TPOIDColorSerializable
		{
			get { return Serialize.BrushToString(TPOIDColor); }
			set { TPOIDColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "TPO ID Font", Order = 29, GroupName = "TPO")]
		public SimpleFont TPOIDFont
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "TPO ID YPixel Offset", Order = 31, GroupName = "TPO", Description = "TPO ID YPixelOffset (0-50).")]
		public int TPOIDYPixelOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "TPO Space Between Letters", Order = 21, GroupName = "TPO", Description = "TPO Space Between Letters (1-5).")]
		public int TPOSpaceBetweenLetters
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "TPO Size", Order = 7, GroupName = "Session", Description = "TPO size in minutes (min 5, max 240).")]
		public int TPOSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Value Area Size", Order = 2, GroupName = "POC", Description = "Value Area Size in percentage.")]
		public int ValueAreaSize
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Virgin POC Stroke", Order = 3, GroupName = "Virgin POC")]
		public Stroke VirginPOCStroke
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Virgin POC VAH Stroke", Order = 4, GroupName = "Virgin POC")]
		public Stroke VirginPOCVAHStroke
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Virgin POC VAL Stroke", Order = 5, GroupName = "Virgin POC")]
		public Stroke VirginPOCVALStroke
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Volume Histogram Border Color", Order = 5, GroupName = "Volume Histogram")]
		public Brush VolumeHistogramBorderColor
		{ get; set; }

		[Browsable(false)]
		public string VolumeHistogramBorderColorSerializable
		{
			get { return Serialize.BrushToString(VolumeHistogramBorderColor); }
			set { VolumeHistogramBorderColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Volume Histogram Stroke", Order = 2, GroupName = "Volume Histogram")]
		public Stroke VolumeHistogramStroke
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IGMarketProfile[] cacheIGMarketProfile;
		public IGMarketProfile IGMarketProfile(int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			return IGMarketProfile(Input, arrowSize, closeArrowColor, drawArrowsBool, drawIBBool, drawPOCBool, drawPriceHistogramBool, drawTPOBool, drawTPOIDBool, drawVirginPOCBool, drawVolumeHistogramBool, eTHBeginTime, eTHEndTime, iB150Color, iB200Color, iB300Color, iBColor, iBOpenRangeColor, iBOpenRangeSize, iBOpacity, iBSize, iBWidth, mPSessionType, openArrowColor, plotETHBool, pOCStroke, pOCVAHStroke, pOCVALStroke, priceHistogramBorderColor, priceHistogramOpacity, priceHistogramVAColor, priceHistogramVAHColor, priceHistogramVALColor, rTHBeginTime, rTHEndTime, showBars, tPOColor, tPOColorMode, tPOFont, tPOIDColor, tPOIDFont, tPOIDYPixelOffset, tPOSpaceBetweenLetters, tPOSize, valueAreaSize, virginPOCStroke, virginPOCVAHStroke, virginPOCVALStroke, volumeHistogramBorderColor, volumeHistogramStroke);
		}

		public IGMarketProfile IGMarketProfile(ISeries<double> input, int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			if (cacheIGMarketProfile != null)
				for (int idx = 0; idx < cacheIGMarketProfile.Length; idx++)
					if (cacheIGMarketProfile[idx] != null && cacheIGMarketProfile[idx].ArrowSize == arrowSize && cacheIGMarketProfile[idx].CloseArrowColor == closeArrowColor && cacheIGMarketProfile[idx].DrawArrowsBool == drawArrowsBool && cacheIGMarketProfile[idx].DrawIBBool == drawIBBool && cacheIGMarketProfile[idx].DrawPOCBool == drawPOCBool && cacheIGMarketProfile[idx].DrawPriceHistogramBool == drawPriceHistogramBool && cacheIGMarketProfile[idx].DrawTPOBool == drawTPOBool && cacheIGMarketProfile[idx].DrawTPOIDBool == drawTPOIDBool && cacheIGMarketProfile[idx].DrawVirginPOCBool == drawVirginPOCBool && cacheIGMarketProfile[idx].DrawVolumeHistogramBool == drawVolumeHistogramBool && cacheIGMarketProfile[idx].ETHBeginTime == eTHBeginTime && cacheIGMarketProfile[idx].ETHEndTime == eTHEndTime && cacheIGMarketProfile[idx].IB150Color == iB150Color && cacheIGMarketProfile[idx].IB200Color == iB200Color && cacheIGMarketProfile[idx].IB300Color == iB300Color && cacheIGMarketProfile[idx].IBColor == iBColor && cacheIGMarketProfile[idx].IBOpenRangeColor == iBOpenRangeColor && cacheIGMarketProfile[idx].IBOpenRangeSize == iBOpenRangeSize && cacheIGMarketProfile[idx].IBOpacity == iBOpacity && cacheIGMarketProfile[idx].IBSize == iBSize && cacheIGMarketProfile[idx].IBWidth == iBWidth && cacheIGMarketProfile[idx].MPSessionType == mPSessionType && cacheIGMarketProfile[idx].OpenArrowColor == openArrowColor && cacheIGMarketProfile[idx].PlotETHBool == plotETHBool && cacheIGMarketProfile[idx].POCStroke == pOCStroke && cacheIGMarketProfile[idx].POCVAHStroke == pOCVAHStroke && cacheIGMarketProfile[idx].POCVALStroke == pOCVALStroke && cacheIGMarketProfile[idx].PriceHistogramBorderColor == priceHistogramBorderColor && cacheIGMarketProfile[idx].PriceHistogramOpacity == priceHistogramOpacity && cacheIGMarketProfile[idx].PriceHistogramVAColor == priceHistogramVAColor && cacheIGMarketProfile[idx].PriceHistogramVAHColor == priceHistogramVAHColor && cacheIGMarketProfile[idx].PriceHistogramVALColor == priceHistogramVALColor && cacheIGMarketProfile[idx].RTHBeginTime == rTHBeginTime && cacheIGMarketProfile[idx].RTHEndTime == rTHEndTime && cacheIGMarketProfile[idx].ShowBars == showBars && cacheIGMarketProfile[idx].TPOColor == tPOColor && cacheIGMarketProfile[idx].TPOColorMode == tPOColorMode && cacheIGMarketProfile[idx].TPOFont == tPOFont && cacheIGMarketProfile[idx].TPOIDColor == tPOIDColor && cacheIGMarketProfile[idx].TPOIDFont == tPOIDFont && cacheIGMarketProfile[idx].TPOIDYPixelOffset == tPOIDYPixelOffset && cacheIGMarketProfile[idx].TPOSpaceBetweenLetters == tPOSpaceBetweenLetters && cacheIGMarketProfile[idx].TPOSize == tPOSize && cacheIGMarketProfile[idx].ValueAreaSize == valueAreaSize && cacheIGMarketProfile[idx].VirginPOCStroke == virginPOCStroke && cacheIGMarketProfile[idx].VirginPOCVAHStroke == virginPOCVAHStroke && cacheIGMarketProfile[idx].VirginPOCVALStroke == virginPOCVALStroke && cacheIGMarketProfile[idx].VolumeHistogramBorderColor == volumeHistogramBorderColor && cacheIGMarketProfile[idx].VolumeHistogramStroke == volumeHistogramStroke && cacheIGMarketProfile[idx].EqualsInput(input))
						return cacheIGMarketProfile[idx];
			return CacheIndicator<IGMarketProfile>(new IGMarketProfile(){ ArrowSize = arrowSize, CloseArrowColor = closeArrowColor, DrawArrowsBool = drawArrowsBool, DrawIBBool = drawIBBool, DrawPOCBool = drawPOCBool, DrawPriceHistogramBool = drawPriceHistogramBool, DrawTPOBool = drawTPOBool, DrawTPOIDBool = drawTPOIDBool, DrawVirginPOCBool = drawVirginPOCBool, DrawVolumeHistogramBool = drawVolumeHistogramBool, ETHBeginTime = eTHBeginTime, ETHEndTime = eTHEndTime, IB150Color = iB150Color, IB200Color = iB200Color, IB300Color = iB300Color, IBColor = iBColor, IBOpenRangeColor = iBOpenRangeColor, IBOpenRangeSize = iBOpenRangeSize, IBOpacity = iBOpacity, IBSize = iBSize, IBWidth = iBWidth, MPSessionType = mPSessionType, OpenArrowColor = openArrowColor, PlotETHBool = plotETHBool, POCStroke = pOCStroke, POCVAHStroke = pOCVAHStroke, POCVALStroke = pOCVALStroke, PriceHistogramBorderColor = priceHistogramBorderColor, PriceHistogramOpacity = priceHistogramOpacity, PriceHistogramVAColor = priceHistogramVAColor, PriceHistogramVAHColor = priceHistogramVAHColor, PriceHistogramVALColor = priceHistogramVALColor, RTHBeginTime = rTHBeginTime, RTHEndTime = rTHEndTime, ShowBars = showBars, TPOColor = tPOColor, TPOColorMode = tPOColorMode, TPOFont = tPOFont, TPOIDColor = tPOIDColor, TPOIDFont = tPOIDFont, TPOIDYPixelOffset = tPOIDYPixelOffset, TPOSpaceBetweenLetters = tPOSpaceBetweenLetters, TPOSize = tPOSize, ValueAreaSize = valueAreaSize, VirginPOCStroke = virginPOCStroke, VirginPOCVAHStroke = virginPOCVAHStroke, VirginPOCVALStroke = virginPOCVALStroke, VolumeHistogramBorderColor = volumeHistogramBorderColor, VolumeHistogramStroke = volumeHistogramStroke }, input, ref cacheIGMarketProfile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IGMarketProfile IGMarketProfile(int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			return indicator.IGMarketProfile(Input, arrowSize, closeArrowColor, drawArrowsBool, drawIBBool, drawPOCBool, drawPriceHistogramBool, drawTPOBool, drawTPOIDBool, drawVirginPOCBool, drawVolumeHistogramBool, eTHBeginTime, eTHEndTime, iB150Color, iB200Color, iB300Color, iBColor, iBOpenRangeColor, iBOpenRangeSize, iBOpacity, iBSize, iBWidth, mPSessionType, openArrowColor, plotETHBool, pOCStroke, pOCVAHStroke, pOCVALStroke, priceHistogramBorderColor, priceHistogramOpacity, priceHistogramVAColor, priceHistogramVAHColor, priceHistogramVALColor, rTHBeginTime, rTHEndTime, showBars, tPOColor, tPOColorMode, tPOFont, tPOIDColor, tPOIDFont, tPOIDYPixelOffset, tPOSpaceBetweenLetters, tPOSize, valueAreaSize, virginPOCStroke, virginPOCVAHStroke, virginPOCVALStroke, volumeHistogramBorderColor, volumeHistogramStroke);
		}

		public Indicators.IGMarketProfile IGMarketProfile(ISeries<double> input , int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			return indicator.IGMarketProfile(input, arrowSize, closeArrowColor, drawArrowsBool, drawIBBool, drawPOCBool, drawPriceHistogramBool, drawTPOBool, drawTPOIDBool, drawVirginPOCBool, drawVolumeHistogramBool, eTHBeginTime, eTHEndTime, iB150Color, iB200Color, iB300Color, iBColor, iBOpenRangeColor, iBOpenRangeSize, iBOpacity, iBSize, iBWidth, mPSessionType, openArrowColor, plotETHBool, pOCStroke, pOCVAHStroke, pOCVALStroke, priceHistogramBorderColor, priceHistogramOpacity, priceHistogramVAColor, priceHistogramVAHColor, priceHistogramVALColor, rTHBeginTime, rTHEndTime, showBars, tPOColor, tPOColorMode, tPOFont, tPOIDColor, tPOIDFont, tPOIDYPixelOffset, tPOSpaceBetweenLetters, tPOSize, valueAreaSize, virginPOCStroke, virginPOCVAHStroke, virginPOCVALStroke, volumeHistogramBorderColor, volumeHistogramStroke);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IGMarketProfile IGMarketProfile(int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			return indicator.IGMarketProfile(Input, arrowSize, closeArrowColor, drawArrowsBool, drawIBBool, drawPOCBool, drawPriceHistogramBool, drawTPOBool, drawTPOIDBool, drawVirginPOCBool, drawVolumeHistogramBool, eTHBeginTime, eTHEndTime, iB150Color, iB200Color, iB300Color, iBColor, iBOpenRangeColor, iBOpenRangeSize, iBOpacity, iBSize, iBWidth, mPSessionType, openArrowColor, plotETHBool, pOCStroke, pOCVAHStroke, pOCVALStroke, priceHistogramBorderColor, priceHistogramOpacity, priceHistogramVAColor, priceHistogramVAHColor, priceHistogramVALColor, rTHBeginTime, rTHEndTime, showBars, tPOColor, tPOColorMode, tPOFont, tPOIDColor, tPOIDFont, tPOIDYPixelOffset, tPOSpaceBetweenLetters, tPOSize, valueAreaSize, virginPOCStroke, virginPOCVAHStroke, virginPOCVALStroke, volumeHistogramBorderColor, volumeHistogramStroke);
		}

		public Indicators.IGMarketProfile IGMarketProfile(ISeries<double> input , int arrowSize, Brush closeArrowColor, bool drawArrowsBool, bool drawIBBool, bool drawPOCBool, bool drawPriceHistogramBool, bool drawTPOBool, bool drawTPOIDBool, bool drawVirginPOCBool, bool drawVolumeHistogramBool, DateTime eTHBeginTime, DateTime eTHEndTime, Brush iB150Color, Brush iB200Color, Brush iB300Color, Brush iBColor, Brush iBOpenRangeColor, int iBOpenRangeSize, int iBOpacity, int iBSize, int iBWidth, IGMarketProfileEnums.SessionType mPSessionType, Brush openArrowColor, bool plotETHBool, Stroke pOCStroke, Stroke pOCVAHStroke, Stroke pOCVALStroke, Brush priceHistogramBorderColor, int priceHistogramOpacity, Brush priceHistogramVAColor, Brush priceHistogramVAHColor, Brush priceHistogramVALColor, DateTime rTHBeginTime, DateTime rTHEndTime, bool showBars, Brush tPOColor, IGMarketProfileEnums.ColorMode tPOColorMode, SimpleFont tPOFont, Brush tPOIDColor, SimpleFont tPOIDFont, int tPOIDYPixelOffset, int tPOSpaceBetweenLetters, int tPOSize, int valueAreaSize, Stroke virginPOCStroke, Stroke virginPOCVAHStroke, Stroke virginPOCVALStroke, Brush volumeHistogramBorderColor, Stroke volumeHistogramStroke)
		{
			return indicator.IGMarketProfile(input, arrowSize, closeArrowColor, drawArrowsBool, drawIBBool, drawPOCBool, drawPriceHistogramBool, drawTPOBool, drawTPOIDBool, drawVirginPOCBool, drawVolumeHistogramBool, eTHBeginTime, eTHEndTime, iB150Color, iB200Color, iB300Color, iBColor, iBOpenRangeColor, iBOpenRangeSize, iBOpacity, iBSize, iBWidth, mPSessionType, openArrowColor, plotETHBool, pOCStroke, pOCVAHStroke, pOCVALStroke, priceHistogramBorderColor, priceHistogramOpacity, priceHistogramVAColor, priceHistogramVAHColor, priceHistogramVALColor, rTHBeginTime, rTHEndTime, showBars, tPOColor, tPOColorMode, tPOFont, tPOIDColor, tPOIDFont, tPOIDYPixelOffset, tPOSpaceBetweenLetters, tPOSize, valueAreaSize, virginPOCStroke, virginPOCVAHStroke, virginPOCVALStroke, volumeHistogramBorderColor, volumeHistogramStroke);
		}
	}
}

#endregion
