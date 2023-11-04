//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public enum Zombie9PowerBoxCandleColorPalettes
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
	public class THPowerBox : Indicator
	{
		private const string SystemVersion = "v1.015";
		private const string SystemName = "THPowerBox";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private const string ObjectPrefix = "thpb_";
		private MAX max;
		private MIN min;
		private int lastPrintOutputHashCode = 0;

		private Instrument attachedInstrument = null;

		private SignalCache signalCache = null;

		private Brush signalColorHalfLifeBuy = Brushes.Gold;
		private Brush signalColorHalfLifeSell = Brushes.Gold;

		private Series<bool> buyHalfLifeSetupSeries;
		private Series<bool> sellHalfLifeSetupSeries;

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

		private enum CandleTypes
		{
			Bull = 0,
			BullBear = 1,
			Bear = 2,
			BearBull = 3
		}

		public enum SignalTypes
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
			AttackBuy = 11,
			AttackSell = 12
		};

		const int MeanChangePlotIndex = 0;
		const int MeanBullishPlotIndex = 1;
		const int MeanBearishPlotIndex = 2;
		const int UpperPlotIndex = 3;
		const int LowerPlotIndex = 4;

		private Brush meanChangeColor;
		private Brush meanBullishColor;
		private Brush meanBearishColor;

		private const int AUTO_ADJUST_PERIOD_M1_MULTIPLIER = 5;
		private int powerBoxPeriod = 0;

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
				IsSuspendedWhileInactive = true;
				ShowTransparentPlotsInDataBox = true;
				IsAutoScale = true;
				PaintPriceMarkers = false;

				AutoAdjustPeriodsOnM1 = true;
				PowerBoxPeriod = 8;
				this.PaintCandleBar = false;
				this.PaintCandleOutline = false;
				CandleColorPalette = Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteBlueAndPurple;
				ShowHalfLifeEntrySetups = false;
				SignalBarPaddingTicks = 22;

				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.Solid, 1), PlotStyle.Line, "MeanChange");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1), PlotStyle.Line, "MeanBullish");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1), PlotStyle.Line, "MeanBearish");
				AddPlot(new Stroke(Brushes.Gainsboro, DashStyleHelper.Solid, 1), PlotStyle.Line, "Upper");
				AddPlot(new Stroke(Brushes.Gainsboro, DashStyleHelper.Solid, 1), PlotStyle.Line, "Lower");
				AddPlot(Brushes.Transparent, "HalfLifeSetupStatus");
				AddPlot(Brushes.Transparent, "PowerBoxPeriodActual");

			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;
			}
			else if (State == State.DataLoaded)
			{
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

				Series<double> signalSeries = new Series<double>(this, this.MaximumBarsLookBack);
				signalCache = new SignalCache(signalSeries);

				buyHalfLifeSetupSeries = new Series<bool>(this);
				sellHalfLifeSetupSeries = new Series<bool>(this);

				if (AutoAdjustPeriodsOnM1 && IsBarPeriodM1(BarsPeriod))
				{
					powerBoxPeriod = PowerBoxPeriod * AUTO_ADJUST_PERIOD_M1_MULTIPLIER;
				}
				else
				{
					powerBoxPeriod = PowerBoxPeriod;
				}

				max = MAX(High, powerBoxPeriod);
				min = MIN(Low, powerBoxPeriod);

				meanChangeColor = Plots[MeanChangePlotIndex].Brush;
				meanBullishColor = Plots[MeanBullishPlotIndex].Brush;
				meanBearishColor = Plots[MeanBearishPlotIndex].Brush;

				meanChangeColor.Freeze();
				meanBullishColor.Freeze();
				meanBearishColor.Freeze();

				signalColorHalfLifeBuy.Freeze();
				signalColorHalfLifeSell.Freeze();

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

		private bool IsBarPeriodM1(BarsPeriod barPeriod)
		{
			bool returnFlag = false;

			if (barPeriod != null
				&& barPeriod.BaseBarsPeriodType == BarsPeriodType.Minute
				&& barPeriod.Value == 1)
			{
				returnFlag = true;
			}

			return returnFlag;
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < powerBoxPeriod)
				return;

			PowerBoxPeriodActual[0] = powerBoxPeriod;
			this.HalfLifeSetupStatus[0] = 0;

			double max0 = max[0];
			double min0 = min[0];
			double powerBoxMeanPrice = (max0 + min0) / 2;

			MeanChange[0] = powerBoxMeanPrice;

			Upper[0] = max0;
			Lower[0] = min0;

			if (Close[0] >= MeanChange[0])
			{
				PlotBrushes[MeanChangePlotIndex][0] = meanBullishColor;

			}
			else
			{
				PlotBrushes[MeanChangePlotIndex][0] = meanBearishColor;

			}




			bool bullishCandle = Close[0] >= Close[1];
			bool bullishPreviousCandle = (Close[1] >= Close[2]);
			bool bullish2PreviousCandle = (Close[2] >= Close[3]);
			bool aboveMiddleCandle = Close[0] >= MeanChange[0];
			bool aboveMiddlePreviousCandle = Close[1] >= MeanChange[1];

			bool expandUpperCandle = Upper[1] < Upper[0];
			bool expandUpperPreviousCandle = Upper[2] < Upper[1];
			bool expandUpper2PreviousCandle = Upper[3] < Upper[2];

			bool expandLowerCandle = Lower[1] > Lower[0];
			bool expandLowerPreviousCandle = Lower[2] > Lower[1];
			bool expandLower2PreviousCandle = Lower[3] > Lower[2];

			bool buyHalfLifeSetup = (!aboveMiddleCandle || !aboveMiddlePreviousCandle) && ((bullishCandle && expandLowerCandle) ||
			   (bullishCandle && !bullishPreviousCandle && expandLowerPreviousCandle) ||
			   (bullishCandle && !bullishPreviousCandle && !bullish2PreviousCandle && expandLower2PreviousCandle));

			bool sellHalfLifeSetup = (aboveMiddleCandle || aboveMiddlePreviousCandle) && ((!bullishCandle && expandUpperCandle) ||
			   (!bullishCandle && bullishPreviousCandle && expandUpperPreviousCandle) ||
			   (!bullishCandle && bullishPreviousCandle && bullish2PreviousCandle && expandUpper2PreviousCandle));


			buyHalfLifeSetupSeries[0] = false;
			sellHalfLifeSetupSeries[0] = false;


			if (buyHalfLifeSetup && ShowHalfLifeEntrySetups)
			{
				buyHalfLifeSetupSeries[0] = true;
				this.HalfLifeSetupStatus[0] = 1;

				if (ShowHalfLifeEntrySetups)
				{
					ClearAllSignalsFromBar(CurrentBar);
					DrawBuyHalfLifeSignal(CurrentBar, Low[0], signalColorHalfLifeBuy);
				}
			}
			else if (sellHalfLifeSetup && ShowHalfLifeEntrySetups)
			{
				sellHalfLifeSetupSeries[0] = true;
				this.HalfLifeSetupStatus[0] = -1;

				if (ShowHalfLifeEntrySetups)
				{
					ClearAllSignalsFromBar(CurrentBar);
					DrawSellHalfLifeSignal(CurrentBar, High[0], signalColorHalfLifeSell);
				}
			}
			else
			{
				ClearAllSignalsFromBar(CurrentBar);
			}


			bool powerBoxBullishTrend = Close[0] >= powerBoxMeanPrice;

			SetCandleColors(powerBoxBullishTrend, bullishCandle);

		}

		private void SetCandleColors(bool isBullishTrend, bool isBullishCandle)
		{
			if (isBullishTrend)
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

		private Brush CandleColorSwitch(CandleTypes candleType)
		{
			Brush candleColorBrush = Brushes.Transparent;

			if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteBlueAndPurple)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGreenAndRed)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGrayScale)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGRScale)
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

			if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteBlueAndPurple)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGreenAndRed)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGrayScale)
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
			else if (this.CandleColorPalette == Zombie9PowerBoxCandleColorPalettes.CandleColorPaletteGRScale)
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

		private void ClearAllSignalsFromBar(int currentBar)
		{
			SignalTypes signalValue = signalCache.CurrentSignal;

			if (signalValue == SignalTypes.HalfLifeBuy)
				RemoveBuyHalfLifeSignal(currentBar);
			else if (signalValue == SignalTypes.HalfLifeSell)
				RemoveSellHalfLifeSignal(currentBar);

		}

		void DrawBuyHalfLifeSignal(int currentBar, double price, Brush brush)
		{
			string key = BuildBuyHalfLifeKey(currentBar);
			Diamond tempDiamond = Draw.Diamond(this, key, true, 0, price - GetSignalBarPadding(), brush);
			tempDiamond.OutlineBrush = brush;
			tempDiamond.Dispose();

			signalCache.AddSignal(SignalTypes.HalfLifeBuy);
		}

		void DrawSellHalfLifeSignal(int currentBar, double price, Brush brush)
		{
			string key = BuildSellHalfLifeKey(currentBar);
			Diamond tempDiamond = Draw.Diamond(this, key, true, 0, price + GetSignalBarPadding(), brush);
			tempDiamond.OutlineBrush = brush;
			tempDiamond.Dispose();

			signalCache.AddSignal(SignalTypes.HalfLifeSell);
		}

		string BuildBuyHalfLifeKey(int currentBar)
		{
			string key = BuildObjectFullName("thhlbuy_" + currentBar);

			return key;
		}

		string BuildSellHalfLifeKey(int currentBar)
		{
			string key = BuildObjectFullName("thhlsell_" + currentBar);

			return key;
		}

		private string BuildObjectFullName(string name)
		{
			string fullName = ObjectPrefix + name;
			return fullName;
		}


		private void RemoveBuyHalfLifeSignal(int currentBar)
		{
			string key = BuildBuyHalfLifeKey(currentBar);
			RemoveDrawObject(key);

			signalCache.ClearSignal();
		}

		private void RemoveSellHalfLifeSignal(int currentBar)
		{
			string key = BuildSellHalfLifeKey(currentBar);
			RemoveDrawObject(key);

			signalCache.ClearSignal();
		}

		double GetSignalBarPadding()
		{
			return (this.TickSize * (double)this.SignalBarPaddingTicks);
		}

		private void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
		{
			this.PrintTo = outputTab;
			if (blockDuplicateMessages)
			{
				int tempHashCode = output.GetHashCode();
				if (tempHashCode != lastPrintOutputHashCode)
				{
					Print(DateTime.Now + " " + SystemName + ": " + output);
				}
				lastPrintOutputHashCode = tempHashCode;
			}
			else
				Print(DateTime.Now + " " + SystemName + ": " + output);
		}

		public class SignalCache
		{
			private Series<double> signalCache = null;

			public SignalTypes CurrentSignal
			{
				get
				{
					return (SignalTypes)this.signalCache[0];
				}
			}
			public SignalCache(Series<double> series)
			{
				this.signalCache = series;
			}

			public void AddSignal(SignalTypes signalType)
			{
				this.signalCache[0] = (double)signalType;
			}

			public void ClearSignal()
			{
				this.signalCache[0] = (double)SignalTypes.None;
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
		[Display(Name = "ShowHalfLifeEntrySetups", Order = 0, GroupName = "Setups")]
		public bool ShowHalfLifeEntrySetups
		{ get; set; }

		[Display(Name = "AutoAdjustPeriodsOnM1", GroupName = "Parameters", Order = 0)]
		public bool AutoAdjustPeriodsOnM1
		{
			get;
			set;
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PowerBoxPeriod", GroupName = "Parameters", Order = 1)]
		public int PowerBoxPeriod
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "SignalBarPaddingTicks", GroupName = "Parameters", Order = 2)]
		[Range(1, int.MaxValue), NinjaScriptProperty]
		public int SignalBarPaddingTicks
		{
			get;
			set;
		}

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
		public Zombie9PowerBoxCandleColorPalettes CandleColorPalette
		{
			get; set;
		}

		[XmlIgnore]
		[Display(Name = "HalfLife Buy", Description = "HalfLife Buy", GroupName = "Parameters", Order = 6)]
		public Brush SignalColorHalfLifeBuy
		{
			get { return signalColorHalfLifeBuy; }
			set { signalColorHalfLifeBuy = value; }
		}

		[Browsable(false)]
		public string SignalColorHalfLifeBuySerialize
		{
			get { return Serialize.BrushToString(signalColorHalfLifeBuy); }
			set { signalColorHalfLifeBuy = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "HalfLife Sell", Description = "HalfLife Sell", GroupName = "Parameters", Order = 7)]
		public Brush SignalColorHalfLifeSell
		{
			get { return signalColorHalfLifeSell; }
			set { signalColorHalfLifeSell = value; }
		}

		[Browsable(false)]
		public string SignalColorHalfLifeSellSerialize
		{
			get { return Serialize.BrushToString(signalColorHalfLifeSell); }
			set { signalColorHalfLifeSell = Serialize.StringToBrush(value); }
		}

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
		

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanChange
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanBullish
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanBearish
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HalfLifeSetupStatus
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PowerBoxPeriodActual
		{
			get { return Values[6]; }
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private THPowerBox[] cacheTHPowerBox;
		public THPowerBox THPowerBox(string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			return THPowerBox(Input, indicatorName, showHalfLifeEntrySetups, powerBoxPeriod, signalBarPaddingTicks);
		}

		public THPowerBox THPowerBox(ISeries<double> input, string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			if (cacheTHPowerBox != null)
				for (int idx = 0; idx < cacheTHPowerBox.Length; idx++)
					if (cacheTHPowerBox[idx] != null && cacheTHPowerBox[idx].IndicatorName == indicatorName && cacheTHPowerBox[idx].ShowHalfLifeEntrySetups == showHalfLifeEntrySetups && cacheTHPowerBox[idx].PowerBoxPeriod == powerBoxPeriod && cacheTHPowerBox[idx].SignalBarPaddingTicks == signalBarPaddingTicks && cacheTHPowerBox[idx].EqualsInput(input))
						return cacheTHPowerBox[idx];
			return CacheIndicator<THPowerBox>(new THPowerBox(){ IndicatorName = indicatorName, ShowHalfLifeEntrySetups = showHalfLifeEntrySetups, PowerBoxPeriod = powerBoxPeriod, SignalBarPaddingTicks = signalBarPaddingTicks }, input, ref cacheTHPowerBox);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.THPowerBox THPowerBox(string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			return indicator.THPowerBox(Input, indicatorName, showHalfLifeEntrySetups, powerBoxPeriod, signalBarPaddingTicks);
		}

		public Indicators.THPowerBox THPowerBox(ISeries<double> input , string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			return indicator.THPowerBox(input, indicatorName, showHalfLifeEntrySetups, powerBoxPeriod, signalBarPaddingTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.THPowerBox THPowerBox(string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			return indicator.THPowerBox(Input, indicatorName, showHalfLifeEntrySetups, powerBoxPeriod, signalBarPaddingTicks);
		}

		public Indicators.THPowerBox THPowerBox(ISeries<double> input , string indicatorName, bool showHalfLifeEntrySetups, int powerBoxPeriod, int signalBarPaddingTicks)
		{
			return indicator.THPowerBox(input, indicatorName, showHalfLifeEntrySetups, powerBoxPeriod, signalBarPaddingTicks);
		}
	}
}

#endregion
