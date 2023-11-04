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
using NinjaTrader.NinjaScript.Indicators.TickHunterTA.THZTAT1;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.TickHunterTA
{
	

	public enum THZSCandleColorPalettes
	{
		CandleColorPaletteBlueAndPurple = 0,
		CandleColorPaletteGreenAndRed = 1,
		CandleColorPaletteGrayScale = 2,
		CandleColorPaletteGRScale = 3
	}

	[Gui.CategoryOrder("0) Indicator Information", 0)]
	[Gui.CategoryOrder("Parameters", 1)]
	[Gui.CategoryOrder("Data Series", 2)]
	[Gui.CategoryOrder("Setup", 3)]
	[Gui.CategoryOrder("Visual", 4)]
	[Gui.CategoryOrder("Candle Palette", 5)]
	public class THZombieSetup : Indicator
	{
		private const string SystemVersion = "v1.008";
		private const string SystemName = "THZombieSetup";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private const string ObjectPrefix = "thzsat_";

		private const int ZombieSetupBuyCode = 1;
		private const int ZombieSetupSellCode = -1;
		private const int ZombieSetupNoCode = 0;

		private Instrument attachedInstrument = null;
		private double attachedInstrumentTickSize = 0;
		private int attachedInstrumentTicksPerPoint = 0;
		private double attachedInstrumentTickValue = 0;

		private Brush signalColorZombieBuy = Brushes.DeepSkyBlue;
		private Brush signalColorZombieSell = Brushes.Violet;

		private Series<double> trailSeries;

		private Brush cp1CandleColorBull = Brushes.RoyalBlue;
		private Brush cp1CandleColorBullBear = Brushes.Turquoise;
		private Brush cp1CandleColorBear = Brushes.Magenta;
		private Brush cp1CandleColorBearBull = Brushes.Thistle;

		private Brush cp1CandleOutlineColorBull = Brushes.RoyalBlue;
		private Brush cp1CandleOutlineColorBullBear = Brushes.Turquoise;
		private Brush cp1CandleOutlineColorBear = Brushes.Magenta;
		private Brush cp1CandleOutlineColorBearBull = Brushes.Thistle;

		private Brush cp2CandleColorBull = Brushes.LightGreen;
		private Brush cp2CandleColorBullBear = Brushes.Green;
		private Brush cp2CandleColorBear = Brushes.Red;
		private Brush cp2CandleColorBearBull = Brushes.LightCoral;

		private Brush cp2CandleOutlineColorBull = Brushes.LightGreen;
		private Brush cp2CandleOutlineColorBullBear = Brushes.Green;
		private Brush cp2CandleOutlineColorBear = Brushes.Red;
		private Brush cp2CandleOutlineColorBearBull = Brushes.LightCoral;

		private Brush cp3CandleColorBull = Brushes.Gainsboro;
		private Brush cp3CandleColorBullBear = Brushes.DimGray;
		private Brush cp3CandleColorBear = Brushes.DimGray;
		private Brush cp3CandleColorBearBull = Brushes.Gainsboro;

		private Brush cp3CandleOutlineColorBull = Brushes.Gainsboro;
		private Brush cp3CandleOutlineColorBullBear = Brushes.DimGray;
		private Brush cp3CandleOutlineColorBear = Brushes.DimGray;
		private Brush cp3CandleOutlineColorBearBull = Brushes.Gainsboro;

		private Brush cp4CandleColorBull = Brushes.Chartreuse;
		private Brush cp4CandleColorBullBear = Brushes.ForestGreen;
		private Brush cp4CandleColorBear = Brushes.Red;
		private Brush cp4CandleColorBearBull = Brushes.Maroon;

		private Brush cp4CandleOutlineColorBull = Brushes.Chartreuse;
		private Brush cp4CandleOutlineColorBullBear = Brushes.ForestGreen;
		private Brush cp4CandleOutlineColorBear = Brushes.Red;
		private Brush cp4CandleOutlineColorBearBull = Brushes.Maroon;

		private ATR atrValue;
		private Series<bool> buyZombieSetupSeries;
		private Series<bool> sellZombieSetupSeries;
		private int lastZombieSetupCode = ZombieSetupNoCode;
		

		private SignalCache signalCache = null;

		private DateTime lastPreCloseAlertTime = DateTime.MinValue;
		private DateTime lastCloseAlertTime = DateTime.MinValue;
		private string fullSoundFilename = "";

		private enum CandleTypes
		{
			Bull = 0,
			BullBear = 1,
			Bear = 2,
			BearBull = 3
		}

		public enum THZSSignalTypes
		{
			None = 0,
			ZombieBuy = 1,
			ZombieSell = 2,
			HalfLifeBuy = 3,
			HalfLifeSell = 4,
			SpikeBuy = 5,
			SpikeSell = 6,
			BumpBuy = 7,
			BumpSell = 8,
			AceBuy = 9,
			AceSell = 10,
			DigBuy = 11,
			DigSell = 12
		};

		public override string DisplayName
		{
			get { return FullSystemName; }
		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = FullSystemName;
				Calculate = Calculate.OnPriceChange;
				IsOverlay = true;
				DisplayInDataBox = true;
				ShowTransparentPlotsInDataBox = true;
				DrawOnPricePanel = true;
				IsAutoScale = true;
				PaintPriceMarkers = false;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive = true;

				ATRPeriod = 15;
				ATRMultiplier = 1;
				this.PaintCandleBar = false;
				this.PaintCandleOutline = false;
				CandleColorPalette = THZSCandleColorPalettes.CandleColorPaletteGreenAndRed;
				ShowZombieEntrySetups = true;
				SignalBarPaddingTicks = 8;
				PlaySoundOnSetupAlert = false;
				LogEntryOnSetupAlert = false;
				this.SoundFile = NinjaTrader.Core.Globals.InstallDir.ToString() + @"sounds\whip.wav";

				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 2), PlotStyle.Line, "TrailingStop");
				AddPlot(Brushes.Transparent, "ZombieSetupStatus");
			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;
				attachedInstrumentTickSize = GetTickSize(attachedInstrument);
				attachedInstrumentTicksPerPoint = GetTicksPerPoint(attachedInstrumentTickSize);
				attachedInstrumentTickValue = GetTickValue(attachedInstrument);
			}
			else if (State == State.DataLoaded)
			{
				Series<double> signalSeries = new Series<double>(this, this.MaximumBarsLookBack);
				signalCache = new SignalCache(signalSeries);

				buyZombieSetupSeries = new Series<bool>(this);
				sellZombieSetupSeries = new Series<bool>(this);

				trailSeries = new Series<double>(this);
				atrValue = ATR(Close, ATRPeriod);

				this.fullSoundFilename = this.SoundFile;

				cp1CandleColorBull.Freeze();
				cp1CandleColorBullBear.Freeze();
				cp1CandleColorBear.Freeze();
				cp1CandleColorBearBull.Freeze();

				cp2CandleColorBull.Freeze();
				cp2CandleColorBullBear.Freeze();
				cp2CandleColorBear.Freeze();
				cp2CandleColorBearBull.Freeze();

				cp3CandleColorBull.Freeze();
				cp3CandleColorBullBear.Freeze();
				cp3CandleColorBear.Freeze();
				cp3CandleColorBearBull.Freeze();

				cp4CandleColorBull.Freeze();
				cp4CandleColorBullBear.Freeze();
				cp4CandleColorBear.Freeze();
				cp4CandleColorBearBull.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < ATRPeriod)
				return;

			buyZombieSetupSeries[0] = false;
			sellZombieSetupSeries[0] = false;
			this.ZombieSetupStatus[0] = lastZombieSetupCode;

			// Trailing stop
			double trailPrice;
			double lossPrice = atrValue[0] * ATRMultiplier; //ATR(Input, ATRPeriod)[0] * ATRMultiplier;
			double currentPrice = Close[0];
			double previousPrice = Close[1];
			bool bullishCandle = Close[0] >= Close[1];

			bool buyZombieSetup = false;
			bool sellZombieSetup = false;

			if (currentPrice > Value[1] && previousPrice > Value[1])
			{
				trailPrice = Math.Max(Value[1], currentPrice - lossPrice);
				SetCandleColors(true, bullishCandle);

				lastZombieSetupCode = ZombieSetupBuyCode;
				this.ZombieSetupStatus[0] = lastZombieSetupCode;
			}
			else if (currentPrice < Value[1] && previousPrice < Value[1])
			{
				trailPrice = Math.Min(Value[1], currentPrice + lossPrice);
				SetCandleColors(false, bullishCandle);

				lastZombieSetupCode = ZombieSetupSellCode;
				this.ZombieSetupStatus[0] = lastZombieSetupCode;
			}
			else if (currentPrice > Value[1])
			{
				trailPrice = currentPrice - lossPrice;
				SetCandleColors(true, bullishCandle);
				buyZombieSetup = true;

				lastZombieSetupCode = ZombieSetupBuyCode;
				this.ZombieSetupStatus[0] = lastZombieSetupCode;
			}
			else
			{
				trailPrice = currentPrice + lossPrice;
				SetCandleColors(false, bullishCandle);
				sellZombieSetup = true;

				lastZombieSetupCode = ZombieSetupSellCode;
				this.ZombieSetupStatus[0] = lastZombieSetupCode;

				//Draw.ArrowDown(this, CurrentBar.ToString(), false, 1, Value[1], Brushes.Orange);
			}

			//Print(" Time=" + Time[0] + " cb=" + CurrentBar + " Bars.Count=");

			Value[0] = trailPrice;


			//SendAlerts(true, Open[0], Time[0]);
			bool previousSetupWasBuy = (buyZombieSetupSeries[1] == true);
			bool previousSetupWasSell = (sellZombieSetupSeries[1] == true);
			bool previousBarHasSetup = (previousSetupWasBuy || previousSetupWasSell);
			bool isBarCurrentBar = (CurrentBar == (Bars.Count - 1));
			bool isPreviousBarClosed = (((CurrentBar - 1) == (Bars.Count - 2)) && previousBarHasSetup);

			if (ShowZombieEntrySetups)
			{
				if (CheckAllowSendAlerts() && isPreviousBarClosed)
				{
					SendAlerts(previousSetupWasBuy, true, Open[1], Time[2]);
				}
			}



			if (buyZombieSetup)
			{
				buyZombieSetupSeries[0] = true;

				if (ShowZombieEntrySetups)
				{
					ClearAllSignalsFromBar(CurrentBar);
					DrawBuyZombieSignal(CurrentBar, Low[0], signalColorZombieBuy);
					if (CheckAllowSendAlerts() && isBarCurrentBar)
					{
						SendAlerts(true, false, Open[0], Time[0]);
					}
				}
			}
			else if (sellZombieSetup)
			{
				sellZombieSetupSeries[0] = true;

				if (ShowZombieEntrySetups)
				{
					ClearAllSignalsFromBar(CurrentBar);
					DrawSellZombieSignal(CurrentBar, High[0], signalColorZombieSell);
					if (CheckAllowSendAlerts() && isBarCurrentBar)
					{
						SendAlerts(false, false, Open[0], Time[0]);
					}
				}
			}
			else
			{
				ClearAllSignalsFromBar(CurrentBar);
			}

		}

		private bool CheckAllowSendAlerts()
		{
			bool returnFlag = false;

			if (LogEntryOnSetupAlert || PlaySoundOnSetupAlert)
			{
				returnFlag = true;
			}

			return (returnFlag);
		}

		private void SendBuyAlerts(bool isCloseAlert, double price, DateTime barTime)
		{
			SendAlerts(true, isCloseAlert, price, barTime);
		}

		private void SendSellAlerts(bool isCloseAlert, double price, DateTime barTime)
		{
			SendAlerts(false, isCloseAlert, price, barTime);
		}

		private void SendAlerts(bool isBuySignal, bool isCloseAlert, double price, DateTime barTime)
		{
			if ((isCloseAlert && this.lastCloseAlertTime != barTime) || (!isCloseAlert && this.lastPreCloseAlertTime != barTime))
			{
				if (isCloseAlert)
					this.lastCloseAlertTime = barTime;
				else
					this.lastPreCloseAlertTime = barTime;

				string signalTypeText = (isBuySignal) ? "Buy" : "Sell";
				string cloesTypeText = (isCloseAlert) ? "Close Bar" : "PreClose Bar";

				string message = GetAlertText(signalTypeText, cloesTypeText, price);

				if (LogEntryOnSetupAlert) Print(DateTime.Now + " " + SystemName + ": " + "*** ALERT: " + message);
				
				if (PlaySoundOnSetupAlert)
				{
					//Print(DateTime.Now + " " + SystemName + ": " + "*** SOUND ALERT: path=" + fullSoundFilename);
					PlaySound(fullSoundFilename);
				}

				
			}

		}

		private string GetInstrumentPeriodText()
		{
			string instrumentName = this.attachedInstrument.FullName;
			string periodText = this.BarsPeriod.Value + " " + this.BarsPeriod.BarsPeriodType.ToString();

			string instrumentPeriodText = "[" + instrumentName + " (" + periodText + ")]";

			return instrumentPeriodText;
		}

		private string GetAlertText(string signalTypeText, string closeTypeText, double openPrice)
		{
			string instrumentPeriodText = GetInstrumentPeriodText();
			string alertText = "";


			alertText = instrumentPeriodText + " - " + signalTypeText + " setup at " + openPrice + " on " + closeTypeText + " - " + FullSystemName;


			return alertText;
		}

		private Brush CandleColorSwitch(CandleTypes candleType)
        {
			Brush candleColorBrush = Brushes.Transparent;

			if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteBlueAndPurple)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp1CandleColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp1CandleColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp1CandleColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp1CandleColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGreenAndRed)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp2CandleColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp2CandleColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp2CandleColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp2CandleColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGrayScale)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp3CandleColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp3CandleColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp3CandleColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp3CandleColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGRScale)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp4CandleColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp4CandleColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp4CandleColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp4CandleColorBearBull;
				}
			}

			return candleColorBrush;
		}

		private Brush CandleColorOutlineSwitch(CandleTypes candleType)
		{
			Brush candleColorBrush = Brushes.Transparent;

			if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteBlueAndPurple)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp1CandleOutlineColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp1CandleOutlineColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp1CandleOutlineColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp1CandleOutlineColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGreenAndRed)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp2CandleOutlineColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp2CandleOutlineColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp2CandleOutlineColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp2CandleOutlineColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGrayScale)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp3CandleOutlineColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp3CandleOutlineColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp3CandleOutlineColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp3CandleOutlineColorBearBull;
				}
			}
			else if (this.CandleColorPalette == THZSCandleColorPalettes.CandleColorPaletteGRScale)
			{
				if (candleType == CandleTypes.Bull)
				{
					candleColorBrush = cp4CandleOutlineColorBull;
				}
				else if (candleType == CandleTypes.BullBear)
				{
					candleColorBrush = cp4CandleOutlineColorBullBear;
				}
				else if (candleType == CandleTypes.Bear)
				{
					candleColorBrush = cp4CandleOutlineColorBear;
				}
				else if (candleType == CandleTypes.BearBull)
				{
					candleColorBrush = cp4CandleOutlineColorBearBull;
				}
			}

			return candleColorBrush;
		}
		private void SetCandleColors(bool isBullishTrail, bool isBullishCandle)
		{
			if (isBullishTrail)
			{
				if (isBullishCandle)
				{
					if (this.PaintCandleBar)
					{
						BarBrush = CandleColorSwitch(CandleTypes.Bull);
					}
					if (this.PaintCandleOutline)
					{
						CandleOutlineBrush = CandleColorOutlineSwitch(CandleTypes.Bull);
					}
				}
				else
				{
					if (this.PaintCandleBar)
					{
						BarBrush = CandleColorSwitch(CandleTypes.BullBear);
					}
					if (this.PaintCandleOutline)
					{
						CandleOutlineBrush = CandleColorOutlineSwitch(CandleTypes.BullBear);
					}
				}
			}
			else
			{
				if (isBullishCandle)
				{
					if (this.PaintCandleBar)
					{
						BarBrush = CandleColorSwitch(CandleTypes.BearBull);
					}
					if (this.PaintCandleOutline)
					{
						CandleOutlineBrush = CandleColorOutlineSwitch(CandleTypes.BearBull);
					}
				}
				else
				{
					if (this.PaintCandleBar)
					{
						BarBrush = CandleColorSwitch(CandleTypes.Bear);
					}
					if (this.PaintCandleOutline)
					{
						CandleOutlineBrush = CandleColorOutlineSwitch(CandleTypes.Bear);
					}
				}
			}
		}

		private void ClearAllSignalsFromBar(int currentBar)
		{
			THZSSignalTypes signalValue = signalCache.CurrentSignal;

			if (signalValue == THZSSignalTypes.ZombieBuy)
				RemoveBuyZombieSignal(currentBar);
			else if (signalValue == THZSSignalTypes.ZombieSell)
				RemoveSellZombieSignal(currentBar);

		}

		void DrawBuyZombieSignal(int currentBar, double price, Brush brush)
		{
			string key = BuildBuyZombieKey(currentBar);
			Square tempSquare = Draw.Square(this, key, true, 0, price - GetSignalBarPadding(), brush);
			tempSquare.OutlineBrush = brush;
			tempSquare.Dispose();

			signalCache.AddSignal(THZSSignalTypes.ZombieBuy);
		}

		void DrawSellZombieSignal(int currentBar, double price, Brush brush)
		{
			string key = BuildSellZombieKey(currentBar);
			Square tempSquare = Draw.Square(this, key, true, 0, price + GetSignalBarPadding(), brush);
			tempSquare.OutlineBrush = brush;
			tempSquare.Dispose();

			signalCache.AddSignal(THZSSignalTypes.ZombieSell);
		}

		private string BuildObjectFullName(string name)
		{
			string fullName = ObjectPrefix + name;
			return fullName;
		}

		string BuildBuyZombieKey(int currentBar)
		{
			string key = BuildObjectFullName("zbuy_" + currentBar);

			return key;
		}

		string BuildSellZombieKey(int currentBar)
		{
			string key = BuildObjectFullName("zsell_" + currentBar);

			return key;
		}

		private void RemoveBuyZombieSignal(int currentBar)
		{
			string key = BuildBuyZombieKey(currentBar);
			RemoveDrawObject(key);

			signalCache.ClearSignal();
		}

		private void RemoveSellZombieSignal(int currentBar)
		{
			string key = BuildSellZombieKey(currentBar);
			RemoveDrawObject(key);

			signalCache.ClearSignal();
		}

		double GetSignalBarPadding()
		{
			return (this.TickSize * (double)this.SignalBarPaddingTicks);
		}


		public static double GetTickSize(Instrument instrument)
		{
			double tickSize = instrument.MasterInstrument.TickSize;

			return (tickSize);
		}

		public double ConvertTicksToDollars(Instrument instrument, int ticks, int contracts)
		{
			double dollarValue = 0;

			if (ticks > 0 && contracts > 0)
			{
				double tickValue = GetTickValue(instrument);
				double tickSize = GetTickSize(instrument);

				dollarValue = tickValue * ticks * contracts;
			}

			return dollarValue;
		}
		public double GetTickValue(Instrument instrument)
		{
			double tickValue = instrument.MasterInstrument.PointValue * instrument.MasterInstrument.TickSize;

			return tickValue;
		}

		public int GetTicksPerPoint(double tickSize)
		{
			int tickPoint = 1;

			if (tickSize < 1)
			{
				tickPoint = (int)(1.0 / tickSize);
			}

			return (tickPoint);
		}

		public class SignalCache
		{
			private Series<double> signalCache = null;

			public THZSSignalTypes CurrentSignal
			{
				get
				{
					return (THZSSignalTypes)this.signalCache[0];
				}
			}
			public SignalCache(Series<double> series)
			{
				this.signalCache = series;
			}

			public void AddSignal(THZSSignalTypes signalType)
			{
				this.signalCache[0] = (double)signalType;
			}

			public void ClearSignal()
			{
				this.signalCache[0] = (double)THZSSignalTypes.None;
			}

		}


		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ATRPeriod", Description = "ATRPeriod", Order = 1, GroupName = "Parameters")]
		public int ATRPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name = "ATRMultiplier", Description = "ATRMultiplier", Order = 2, GroupName = "Parameters")]
		public double ATRMultiplier
		{ get; set; }

		[Display(Name = "PaintCandleBar", Description = "Color Bars", GroupName = "Parameters", Order = 3)]
		public bool PaintCandleBar
		{
			get; set;
		}

		[Display(Name = "PaintCandleOutline", Description = "Color Bars Ouline", GroupName = "Parameters", Order = 4)]
		public bool PaintCandleOutline
		{
			get; set;
		}

		[Display(Name = "CandleColorPalette", Description = "Color Bar Palette", GroupName = "Parameters", Order = 5)]
		public THZSCandleColorPalettes CandleColorPalette
		{
			get; set;
		}

		[NinjaScriptProperty]
		[Display(Name = "ShowZombieEntrySetups", Order = 6, GroupName = "Parameters")]
		public bool ShowZombieEntrySetups
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "SignalBarPaddingTicks", Order = 7, GroupName = "Parameters")]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		public int SignalBarPaddingTicks
		{
			get;
			set;
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "PlaySoundOnSetupAlert", GroupName = "Parameters", Order = 8)]
		public bool PlaySoundOnSetupAlert
		{
			get; set;
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "LogEntryOnSetupAlert", GroupName = "Parameters", Order = 9)]
		public bool LogEntryOnSetupAlert
		{
			get; set;
		}


		[Display(ResourceType = typeof(Custom.Resource), Name = "SoundFile", GroupName = "Parameters", Order = 10)]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter = "Any Files (*.wav)|*.wav")]
		public string SoundFile
		{ get; set; }


		[XmlIgnore]
		[Display(Name = "CP1CandleColorBull", Description = "Color of Bull Candle", GroupName = "Candle Palette", Order = 1)]
		public Brush CP1CandleColorBull
		{
			get { return cp1CandleColorBull; }
			set { cp1CandleColorBull = value; }
		}

		[Browsable(false)]
		public string CP1CandleColorBullSerialize
		{
			get { return Serialize.BrushToString(cp1CandleColorBull); }
			set { cp1CandleColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleColorBullBear", Description = "Color of BullBear Candle", GroupName = "Candle Palette", Order = 2)]
		public Brush CP1CandleColorBullBear
		{
			get { return cp1CandleColorBullBear; }
			set { cp1CandleColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP1CandleColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp1CandleColorBullBear); }
			set { cp1CandleColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleColorBear", Description = "Color of Bear Candle", GroupName = "Candle Palette", Order = 3)]
		public Brush CP1CandleColorBear
		{
			get { return cp1CandleColorBear; }
			set { cp1CandleColorBear = value; }
		}

		[Browsable(false)]
		public string CP1CandleColorBearSerialize
		{
			get { return Serialize.BrushToString(cp1CandleColorBear); }
			set { cp1CandleColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleColorBearBull", Description = "Color of BearBull Candle", GroupName = "Candle Palette", Order = 4)]
		public Brush CP1CandleColorBearBull
		{
			get { return cp1CandleColorBearBull; }
			set { cp1CandleColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP1CandleColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp1CandleColorBearBull); }
			set { cp1CandleColorBearBull = Serialize.StringToBrush(value); }
		}

		//CP1 Candle Outline

		[XmlIgnore]
		[Display(Name = "CP1CandleOutlineColorBull", Description = "Color of Bull Candle Outline", GroupName = "Candle Palette", Order = 5)]
		public Brush CP1CandleOutlineColorBull
		{
			get { return cp1CandleOutlineColorBull; }
			set { cp1CandleOutlineColorBull = value; }
		}

		[Browsable(false)]
		public string CP1CandleOutlineColorBullSerialize
		{
			get { return Serialize.BrushToString(cp1CandleOutlineColorBull); }
			set { cp1CandleOutlineColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleOutlineColorBullBear", Description = "Color of BullBear Candle Outline", GroupName = "Candle Palette", Order = 6)]
		public Brush CP1CandleOutlineColorBullBear
		{
			get { return cp1CandleOutlineColorBullBear; }
			set { cp1CandleOutlineColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP1CandleOutlineColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp1CandleOutlineColorBullBear); }
			set { cp1CandleOutlineColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleOutlineColorBear", Description = "Color of Bear Candle Outline", GroupName = "Candle Palette", Order = 7)]
		public Brush CP1CandleOutlineColorBear
		{
			get { return cp1CandleOutlineColorBear; }
			set { cp1CandleOutlineColorBear = value; }
		}

		[Browsable(false)]
		public string CP1CandleOutlineColorBearSerialize
		{
			get { return Serialize.BrushToString(cp1CandleOutlineColorBear); }
			set { cp1CandleOutlineColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP1CandleOutlineColorBearBull", Description = "Color of BearBull Candle Outline", GroupName = "Candle Palette", Order = 8)]
		public Brush CP1CandleOutlineColorBearBull
		{
			get { return cp1CandleOutlineColorBearBull; }
			set { cp1CandleOutlineColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP1CandleOutlineColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp1CandleOutlineColorBearBull); }
			set { cp1CandleOutlineColorBearBull = Serialize.StringToBrush(value); }
		}


		//CP2 Candles

		[XmlIgnore]
		[Display(Name = "CP2CandleColorBull", Description = "Color of Bull Candle", GroupName = "Candle Palette", Order = 9)]
		public Brush CP2CandleColorBull
		{
			get { return cp2CandleColorBull; }
			set { cp2CandleColorBull = value; }
		}

		[Browsable(false)]
		public string CP2CandleColorBullSerialize
		{
			get { return Serialize.BrushToString(cp2CandleColorBull); }
			set { cp2CandleColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleColorBullBear", Description = "Color of BullBear Candle", GroupName = "Candle Palette", Order = 10)]
		public Brush CP2CandleColorBullBear
		{
			get { return cp2CandleColorBullBear; }
			set { cp2CandleColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP2CandleColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp2CandleColorBullBear); }
			set { cp2CandleColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleColorBear", Description = "Color of Bear Candle", GroupName = "Candle Palette", Order = 11)]
		public Brush CP2CandleColorBear
		{
			get { return cp2CandleColorBear; }
			set { cp2CandleColorBear = value; }
		}

		[Browsable(false)]
		public string CP2CandleColorBearSerialize
		{
			get { return Serialize.BrushToString(cp2CandleColorBear); }
			set { cp2CandleColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleColorBearBull", Description = "Color of BearBull Candle", GroupName = "Candle Palette", Order = 12)]
		public Brush CP2CandleColorBearBull
		{
			get { return cp2CandleColorBearBull; }
			set { cp2CandleColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP2CandleColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp2CandleColorBearBull); }
			set { cp2CandleColorBearBull = Serialize.StringToBrush(value); }
		}

		//CP2 Outline

		[XmlIgnore]
		[Display(Name = "CP2CandleOutlineColorBull", Description = "Color of Bull Candle Outline", GroupName = "Candle Palette", Order = 13)]
		public Brush CP2CandleOutlineColorBull
		{
			get { return cp2CandleOutlineColorBull; }
			set { cp2CandleOutlineColorBull = value; }
		}

		[Browsable(false)]
		public string CP2CandleColorOutlineBullSerialize
		{
			get { return Serialize.BrushToString(cp2CandleOutlineColorBull); }
			set { cp2CandleOutlineColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleOutlineColorBullBear", Description = "Color of BullBear Candle Outline", GroupName = "Candle Palette", Order = 14)]
		public Brush CP2CandleOutlineColorBullBear
		{
			get { return cp2CandleOutlineColorBullBear; }
			set { cp2CandleOutlineColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP2CandleOutlineColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp2CandleOutlineColorBullBear); }
			set { cp2CandleOutlineColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleOutlineColorBear", Description = "Color of Bear Candle Outline", GroupName = "Candle Palette", Order = 15)]
		public Brush CP2CandleOutlineColorBear
		{
			get { return cp2CandleOutlineColorBear; }
			set { cp2CandleOutlineColorBear = value; }
		}

		[Browsable(false)]
		public string CP2CandleOutlineColorBearSerialize
		{
			get { return Serialize.BrushToString(cp2CandleOutlineColorBear); }
			set { cp2CandleOutlineColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP2CandleOutlineColorBearBull", Description = "Color of BearBull Candle Outline", GroupName = "Candle Palette", Order = 16)]
		public Brush CP2CandleOutlineColorBearBull
		{
			get { return cp2CandleOutlineColorBearBull; }
			set { cp2CandleOutlineColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP2CandleOutlineColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp2CandleOutlineColorBearBull); }
			set { cp2CandleOutlineColorBearBull = Serialize.StringToBrush(value); }
		}


		//CP3 Candles

		[XmlIgnore]
		[Display(Name = "CP3CandleColorBull", Description = "Color of Bull Candle", GroupName = "Candle Palette", Order = 17)]
		public Brush CP3CandleColorBull
		{
			get { return cp3CandleColorBull; }
			set { cp3CandleColorBull = value; }
		}

		[Browsable(false)]
		public string CP3CandleColorBullSerialize
		{
			get { return Serialize.BrushToString(cp3CandleColorBull); }
			set { cp3CandleColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleColorBullBear", Description = "Color of BullBear Candle", GroupName = "Candle Palette", Order = 18)]
		public Brush CP3CandleColorBullBear
		{
			get { return cp3CandleColorBullBear; }
			set { cp3CandleColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP3CandleColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp3CandleColorBullBear); }
			set { cp3CandleColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleColorBear", Description = "Color of Bear Candle", GroupName = "Candle Palette", Order = 19)]
		public Brush CP3CandleColorBear
		{
			get { return cp3CandleColorBear; }
			set { cp3CandleColorBear = value; }
		}

		[Browsable(false)]
		public string CP3CandleColorBearSerialize
		{
			get { return Serialize.BrushToString(cp3CandleColorBear); }
			set { cp3CandleColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleColorBearBull", Description = "Color of BearBull Candle", GroupName = "Candle Palette", Order = 20)]
		public Brush CP3CandleColorBearBull
		{
			get { return cp3CandleColorBearBull; }
			set { cp3CandleColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP3CandleColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp3CandleColorBearBull); }
			set { cp3CandleColorBearBull = Serialize.StringToBrush(value); }
		}

		//CP3 Outline

		[XmlIgnore]
		[Display(Name = "CP3CandleOutlineColorBull", Description = "Color of Bull Candle Outline", GroupName = "Candle Palette", Order = 21)]
		public Brush CP3CandleOutlineColorBull
		{
			get { return cp3CandleOutlineColorBull; }
			set { cp3CandleOutlineColorBull = value; }
		}

		[Browsable(false)]
		public string CP3CandleColorOutlineBullSerialize
		{
			get { return Serialize.BrushToString(cp3CandleOutlineColorBull); }
			set { cp3CandleOutlineColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleOutlineColorBullBear", Description = "Color of BullBear Candle Outline", GroupName = "Candle Palette", Order = 22)]
		public Brush CP3CandleOutlineColorBullBear
		{
			get { return cp3CandleOutlineColorBullBear; }
			set { cp3CandleOutlineColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP3CandleOutlineColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp3CandleOutlineColorBullBear); }
			set { cp3CandleOutlineColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleOutlineColorBear", Description = "Color of Bear Candle Outline", GroupName = "Candle Palette", Order = 23)]
		public Brush CP3CandleOutlineColorBear
		{
			get { return cp3CandleOutlineColorBear; }
			set { cp3CandleOutlineColorBear = value; }
		}

		[Browsable(false)]
		public string CP3CandleOutlineColorBearSerialize
		{
			get { return Serialize.BrushToString(cp3CandleOutlineColorBear); }
			set { cp3CandleOutlineColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP3CandleOutlineColorBearBull", Description = "Color of BearBull Candle Outline", GroupName = "Candle Palette", Order = 24)]
		public Brush CP3CandleOutlineColorBearBull
		{
			get { return cp3CandleOutlineColorBearBull; }
			set { cp3CandleOutlineColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP3CandleOutlineColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp3CandleOutlineColorBearBull); }
			set { cp3CandleOutlineColorBearBull = Serialize.StringToBrush(value); }
		}

		//CP4 Candles

		[XmlIgnore]
		[Display(Name = "CP4CandleColorBull", Description = "Color of Bull Candle", GroupName = "Candle Palette", Order = 25)]
		public Brush CP4CandleColorBull
		{
			get { return cp4CandleColorBull; }
			set { cp4CandleColorBull = value; }
		}

		[Browsable(false)]
		public string CP4CandleColorBullSerialize
		{
			get { return Serialize.BrushToString(cp4CandleColorBull); }
			set { cp4CandleColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleColorBullBear", Description = "Color of BullBear Candle", GroupName = "Candle Palette", Order = 26)]
		public Brush CP4CandleColorBullBear
		{
			get { return cp4CandleColorBullBear; }
			set { cp4CandleColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP4CandleColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp4CandleColorBullBear); }
			set { cp4CandleColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleColorBear", Description = "Color of Bear Candle", GroupName = "Candle Palette", Order = 27)]
		public Brush CP4CandleColorBear
		{
			get { return cp4CandleColorBear; }
			set { cp4CandleColorBear = value; }
		}

		[Browsable(false)]
		public string CP4CandleColorBearSerialize
		{
			get { return Serialize.BrushToString(cp4CandleColorBear); }
			set { cp4CandleColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleColorBearBull", Description = "Color of BearBull Candle", GroupName = "Candle Palette", Order = 28)]
		public Brush CP4CandleColorBearBull
		{
			get { return cp4CandleColorBearBull; }
			set { cp4CandleColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP4CandleColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp4CandleColorBearBull); }
			set { cp4CandleColorBearBull = Serialize.StringToBrush(value); }
		}

		//CP4 Outline

		[XmlIgnore]
		[Display(Name = "CP4CandleOutlineColorBull", Description = "Color of Bull Candle Outline", GroupName = "Candle Palette", Order = 29)]
		public Brush CP4CandleOutlineColorBull
		{
			get { return cp4CandleOutlineColorBull; }
			set { cp4CandleOutlineColorBull = value; }
		}

		[Browsable(false)]
		public string CP4CandleColorOutlineBullSerialize
		{
			get { return Serialize.BrushToString(cp4CandleOutlineColorBull); }
			set { cp4CandleOutlineColorBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleOutlineColorBullBear", Description = "Color of BullBear Candle Outline", GroupName = "Candle Palette", Order = 30)]
		public Brush CP4CandleOutlineColorBullBear
		{
			get { return cp4CandleOutlineColorBullBear; }
			set { cp4CandleOutlineColorBullBear = value; }
		}

		[Browsable(false)]
		public string CP4CandleOutlineColorBullBearSerialize
		{
			get { return Serialize.BrushToString(cp4CandleOutlineColorBullBear); }
			set { cp4CandleOutlineColorBullBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleOutlineColorBear", Description = "Color of Bear Candle Outline", GroupName = "Candle Palette", Order = 31)]
		public Brush CP4CandleOutlineColorBear
		{
			get { return cp4CandleOutlineColorBear; }
			set { cp4CandleOutlineColorBear = value; }
		}

		[Browsable(false)]
		public string CP4CandleOutlineColorBearSerialize
		{
			get { return Serialize.BrushToString(cp4CandleOutlineColorBear); }
			set { cp4CandleOutlineColorBear = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "CP4CandleOutlineColorBearBull", Description = "Color of BearBull Candle Outline", GroupName = "Candle Palette", Order = 32)]
		public Brush CP4CandleOutlineColorBearBull
		{
			get { return cp4CandleOutlineColorBearBull; }
			set { cp4CandleOutlineColorBearBull = value; }
		}

		[Browsable(false)]
		public string CP4CandleOutlineColorBearBullSerialize
		{
			get { return Serialize.BrushToString(cp4CandleOutlineColorBearBull); }
			set { cp4CandleOutlineColorBearBull = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Zombie Buy", Description = "Zombie Buy", GroupName = "Candle Palette", Order = 33)]
		public Brush SignalColorZombieBuy
		{
			get { return signalColorZombieBuy; }
			set { signalColorZombieBuy = value; }
		}

		[Browsable(false)]
		public string SignalColorZombieBuySerialize
		{
			get { return Serialize.BrushToString(signalColorZombieBuy); }
			set { signalColorZombieBuy = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Zombie Sell", Description = "Zombie Sell", GroupName = "Candle Palette", Order = 34)]
		public Brush SignalColorZombieSell
		{
			get { return signalColorZombieSell; }
			set { signalColorZombieSell = value; }
		}

		[Browsable(false)]
		public string SignalColorZombieSellSerialize
		{
			get { return Serialize.BrushToString(signalColorZombieSell); }
			set { signalColorZombieSell = Serialize.StringToBrush(value); }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrailingStop
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZombieSetupStatus
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

namespace NinjaTrader.NinjaScript.Indicators.TickHunterTA.THZTAT1
{
	public class RealRunOncePerBar
	{
		private bool isNewBar = false;

		public bool IsFirstRunThisBar
		{
			get
			{
				return isNewBar;
			}
		}

		public void SetRunCompletedThisBar()
		{
			isNewBar = false;
		}

		public void SetNewBar()
		{
			isNewBar = true;
		}
	}

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunterTA.THZombieSetup[] cacheTHZombieSetup;
		public TickHunterTA.THZombieSetup THZombieSetup(string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			return THZombieSetup(Input, indicatorName, aTRPeriod, aTRMultiplier, showZombieEntrySetups, signalBarPaddingTicks);
		}

		public TickHunterTA.THZombieSetup THZombieSetup(ISeries<double> input, string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			if (cacheTHZombieSetup != null)
				for (int idx = 0; idx < cacheTHZombieSetup.Length; idx++)
					if (cacheTHZombieSetup[idx] != null && cacheTHZombieSetup[idx].IndicatorName == indicatorName && cacheTHZombieSetup[idx].ATRPeriod == aTRPeriod && cacheTHZombieSetup[idx].ATRMultiplier == aTRMultiplier && cacheTHZombieSetup[idx].ShowZombieEntrySetups == showZombieEntrySetups && cacheTHZombieSetup[idx].SignalBarPaddingTicks == signalBarPaddingTicks && cacheTHZombieSetup[idx].EqualsInput(input))
						return cacheTHZombieSetup[idx];
			return CacheIndicator<TickHunterTA.THZombieSetup>(new TickHunterTA.THZombieSetup(){ IndicatorName = indicatorName, ATRPeriod = aTRPeriod, ATRMultiplier = aTRMultiplier, ShowZombieEntrySetups = showZombieEntrySetups, SignalBarPaddingTicks = signalBarPaddingTicks }, input, ref cacheTHZombieSetup);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunterTA.THZombieSetup THZombieSetup(string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			return indicator.THZombieSetup(Input, indicatorName, aTRPeriod, aTRMultiplier, showZombieEntrySetups, signalBarPaddingTicks);
		}

		public Indicators.TickHunterTA.THZombieSetup THZombieSetup(ISeries<double> input , string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			return indicator.THZombieSetup(input, indicatorName, aTRPeriod, aTRMultiplier, showZombieEntrySetups, signalBarPaddingTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunterTA.THZombieSetup THZombieSetup(string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			return indicator.THZombieSetup(Input, indicatorName, aTRPeriod, aTRMultiplier, showZombieEntrySetups, signalBarPaddingTicks);
		}

		public Indicators.TickHunterTA.THZombieSetup THZombieSetup(ISeries<double> input , string indicatorName, int aTRPeriod, double aTRMultiplier, bool showZombieEntrySetups, int signalBarPaddingTicks)
		{
			return indicator.THZombieSetup(input, indicatorName, aTRPeriod, aTRMultiplier, showZombieEntrySetups, signalBarPaddingTicks);
		}
	}
}

#endregion
