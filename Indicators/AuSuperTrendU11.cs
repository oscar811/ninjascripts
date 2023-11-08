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

#region Global Enums

public enum AuSuperTrendU11BaseType
{
    Median, ADXVMA, Butterworth_2, Butterworth_3, DEMA, DSMA, DTMA, DWMA, Ehlers, EMA, Gauss_2, Gauss_3, Gauss_4,
    HMA, HoltEMA, LinReg, LLMA, SMA, SuperSmoother_2, SuperSmoother_3, TEMA, TMA, TSMA, TWMA, VWMA, WMA, ZeroLagHATEMA, ZeroLagTEMA, ZLEMA
}
public enum AuSuperTrendU11OffsetType
{
    Default, Median, ADXVMA, Butterworth_2, Butterworth_3, DEMA, DSMA, DTMA, DWMA, Ehlers, EMA, Gauss_2, Gauss_3, Gauss_4,
    HMA, HoltEMA, LinReg, LLMA, SMA, SuperSmoother_2, SuperSmoother_3, TEMA, TMA, TSMA, TWMA, VWMA, WMA, ZeroLagHATEMA, ZeroLagTEMA, ZLEMA
}
public enum AuSuperTrendU11VolaType { Simple_Range, True_Range, Standard_Deviation }

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AuSuperTrendU11 : Indicator
	{
        #region Variables
        private int basePeriod = 3; // Default setting for Median Period
        private int rangePeriod = 15; // Default setting for Range Period
        private double multiplier = 2.5; // Default setting for Multiplier
        private AuSuperTrendU11BaseType thisBaseType = AuSuperTrendU11BaseType.HMA;
        private AuSuperTrendU11OffsetType thisOffsetType = AuSuperTrendU11OffsetType.Default;
        private AuSuperTrendU11VolaType thisVolaType = AuSuperTrendU11VolaType.True_Range;
        //private bool candles = false;
        private bool gap = false;
        private bool reverseIntraBar = false;
        private bool showArrows = true;
        private bool showPaintBars = true;
        private bool showStopLine = true;
        private bool soundAlert = false;
        private bool currentUpTrend = true;
        private bool priorUpTrend = true;
        private bool stoppedOut = false;
        private double movingBase = 0.0;
        private double offset = 0.0;
        private double trailingAmount = 0.0;
        private double currentStopLong = 0.0;
        private double currentStopShort = 0.0;
        private double margin = 0.0;
        private int displacement = 0;
        private int opacity = 3;
        private int alpha = 0;
        private int plot0Width = 1;

        private PlotStyle plot0Style = PlotStyle.Dot;
        private DashStyleHelper dash0Style = DashStyleHelper.Dot;
        private int plot1Width = 1;
        private PlotStyle plot1Style = PlotStyle.Line;
        private DashStyleHelper dash1Style = DashStyleHelper.Solid;

        private Brush upColor = Brushes.Lime;
        private Brush trendColor = Brushes.Transparent;
        private Brush downColor = Brushes.Red;

        private int rearmTime = 30;
        private string confirmedUpTrend = @"\sounds\newuptrend.wav";
        private string confirmedDownTrend = @"\sounds\newdowntrend.wav";
        private string potentialUpTrend = @"\sounds\potentialuptrend.wav";
        private string potentialDownTrend = @"\sounds\potentialdowntrend.wav";

        private Series<double> reverseDot;
        private Series<bool> upTrend;

        private Series<double> baseline;
        private Series<double> rangeSeries;
        private Series<double> offsetSeries;

        private ATR volatility;

        private bool bolHistoricalProc = true;
        #endregion

        #region OnStateChange

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Universal SuperTrend V11";
				Name										= "AuSuperTrendU11";
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

                ArePlotsConfigurable = false; // Plots are not configurable in the indicator dialog

                ShowTransparentPlotsInDataBox = true;

                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "StopDot");
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "StopLine");
                AddPlot(new Stroke() { Brush = Brushes.Transparent, Width = 1, DashStyleHelper = DashStyleHelper.Solid }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
                reverseDot = new Series<double>(this);
                upTrend = new Series<bool>(this);
                baseline = new Series<double>(this);
                rangeSeries = new Series<double>(this);
                offsetSeries = new Series<double>(this);
            }
            else if (State == State.Transition)
            {
                bolHistoricalProc = false;
            }

        }

        #endregion

        #region OnBarUpdate

        protected override void OnBarUpdate()
		{
            // on startup
            if (CurrentBar < 1)
            {
                vInitInternalSettings();

                displacement = Math.Max(Displacement, -CurrentBar);
                priorUpTrend = true;
                currentUpTrend = true;
                upTrend[0] = true;
                StopDot[0] = Close[0];
                StopLine[0] = Close[0];
                PlotBrushes[0][-displacement] = Brushes.Transparent;
                PlotBrushes[1][-displacement] = Brushes.Transparent;

                volatility = ATR(Close, 256);

                Plots[0].Width = plot0Width;
                Plots[0].PlotStyle = plot0Style;
                Plots[0].DashStyleHelper = dash0Style;

                Plots[1].Width = plot1Width;
                Plots[1].PlotStyle = plot1Style;
                Plots[1].DashStyleHelper = dash1Style;

                alpha = 25 * opacity;

                gap = (plot1Style == PlotStyle.Line) || (plot1Style == PlotStyle.Square);

            }
            else
            {
                if (IsFirstTickOfBar)
                    vOnFirstTickOfBar();

                if (reverseIntraBar) // only one trend change per bar is permitted
                {
                    if (!stoppedOut)
                    {
                        if (priorUpTrend && Low[0] < currentStopLong)
                        {
                            currentUpTrend = false;
                            stoppedOut = true;
                        }
                        else if (!priorUpTrend && High[0] > currentStopShort)
                        {
                            currentUpTrend = true;
                            stoppedOut = true;
                        }
                        if (stoppedOut)
                        {
                            if (showPaintBars)
                            {
                                if (currentUpTrend)
                                    trendColor = upColor;
                                else
                                    trendColor = downColor;

                                CandleOutlineBrushes[-displacement] = trendColor;
                                BarBrushes[-displacement] = trendColor;
                            }
                            if (showArrows)
                            {
                                if (currentUpTrend && !priorUpTrend)
                                    Draw.ArrowUp(this, "arrow" + CurrentBar, true, -displacement, movingBase - trailingAmount - 0.5 * margin, upColor);
                                else if (!currentUpTrend && priorUpTrend)
                                    Draw.ArrowDown(this, "arrow" + CurrentBar, true, -displacement, movingBase + trailingAmount + 0.5 * margin, downColor);
                            }
                        }
                    }
                }
                else
                {
                    if (priorUpTrend && Close[0] < currentStopLong)
                        currentUpTrend = false;
                    else if (!priorUpTrend && Close[0] > currentStopShort)
                        currentUpTrend = true;
                    else
                        currentUpTrend = priorUpTrend;

                    if (showArrows && Calculate == Calculate.OnBarClose)
                    {
                        if (currentUpTrend && !priorUpTrend)
                            Draw.ArrowUp(this, "arrow" + (CurrentBar + 1), true, -displacement - 1, movingBase - trailingAmount - 0.5 * margin, upColor);
                        else if (!currentUpTrend && priorUpTrend)
                            Draw.ArrowDown(this, "arrow" + (CurrentBar + 1), true, -displacement - 1, movingBase + trailingAmount + 0.5 * margin, downColor);
                    }
                }

                // this information can be accessed by a strategy
                if (Calculate == Calculate.OnBarClose)
                    upTrend[0] = currentUpTrend;
                else if (IsFirstTickOfBar && !reverseIntraBar)
                    upTrend[0] = priorUpTrend;
                else if (reverseIntraBar)
                    upTrend[0] = currentUpTrend;

                if (upTrend[0])
                    Trend[0] = 1;
                else
                    Trend[0] = -1; 

                if (showPaintBars)
                {
                    if (Open[0] < Close[0])
                    {
                        byte g = ((Color)trendColor.GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)trendColor.GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)trendColor.GetValue(SolidColorBrush.ColorProperty)).B;

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)alpha, r, g, b));
                    }
                }

                if (soundAlert && !bolHistoricalProc && (Calculate == Calculate.OnBarClose || reverseIntraBar))
                {
                    if (currentUpTrend && !priorUpTrend)
                    {
                        try
                        {
                            Alert("NewUpTrend", Priority.Medium, "NewUpTrend", confirmedUpTrend, rearmTime, Brushes.Black, upColor);
                        }
                        catch { }
                    }
                    else if (!currentUpTrend && priorUpTrend)
                    {
                        try
                        {
                            Alert("NewDownTrend", Priority.Medium, "NewDownTrend", confirmedDownTrend, rearmTime, Brushes.Black, downColor);
                        }
                        catch { }
                    }
                }
                if (soundAlert && !bolHistoricalProc && Calculate != Calculate.OnBarClose && !reverseIntraBar)
                {
                    if (currentUpTrend && !priorUpTrend)
                    {
                        try
                        {
                            Alert("PotentialUpTrend", Priority.Medium, "PotentialUpTrend", potentialUpTrend, rearmTime, Brushes.Black, upColor);
                        }
                        catch { }
                    }
                    else if (!currentUpTrend && priorUpTrend)
                    {
                        try
                        {
                            Alert("PotentialDownTrend", Priority.Medium, "PotentialDownTrend", potentialDownTrend, rearmTime, Brushes.Black, downColor);
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion

        #region Initialization of Internal Indicators

        private void vInitInternalSettings()
        {
            switch (thisVolaType)
            {
                case AuSuperTrendU11VolaType.Simple_Range:
                    rangeSeries = Range().Value;
                    break;
                case AuSuperTrendU11VolaType.True_Range:
                    rangeSeries = ATR(Close, 1).Value;
                    break;
                case AuSuperTrendU11VolaType.Standard_Deviation:
                    thisOffsetType = AuSuperTrendU11OffsetType.Default;
                    rangeSeries = StdDev(Close, rangePeriod).Value;
                    offsetSeries = StdDev(Close, rangePeriod).Value;
                    break;
            }

            switch (thisBaseType)
            {
                case AuSuperTrendU11BaseType.Median:
                    baseline = AuMovingMedian(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.ADXVMA:
                    baseline = AuADXVMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Butterworth_2:
                    baseline = AuButterworthFilter(Input, 2, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Butterworth_3:
                    baseline = AuButterworthFilter(Input, 3, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.DEMA:
                    baseline = AuDEMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.DSMA:
                    baseline = AuDSMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.DTMA:
                    baseline = AuDTMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.DWMA:
                    baseline = AuDWMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Ehlers:
                    baseline = AuEhlersFilter(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.EMA:
                    baseline = AuEMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Gauss_2:
                    baseline = AuGaussianFilter(Input, 2, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Gauss_3:
                    baseline = AuGaussianFilter(Input, 3, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.Gauss_4:
                    baseline = AuGaussianFilter(Input, 4, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.HMA:
                    baseline = AuHMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.HoltEMA:
                    baseline = AuHoltEMA(Input, basePeriod, 2 * basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.LinReg:
                    baseline = AuLinReg(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.LLMA:
                    baseline = AuLLMA(Input, basePeriod, 0.0).Value;
                    break;
                case AuSuperTrendU11BaseType.SMA:
                    baseline = AuSMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.SuperSmoother_2:
                    baseline = AuSuperSmootherFilter(Input, 2, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.SuperSmoother_3:
                    baseline = AuSuperSmootherFilter(Input, 3, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.TEMA:
                    baseline = AuTEMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.TMA:
                    baseline = AuTMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.TSMA:
                    baseline = AuTSMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.TWMA:
                    baseline = AuTWMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.VWMA:
                    baseline = AuVWMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.WMA:
                    baseline = AuWMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.ZeroLagHATEMA:
                    baseline = AuZeroLagHATEMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.ZeroLagTEMA:
                    baseline = AuZeroLagTEMA(Input, basePeriod).Value;
                    break;
                case AuSuperTrendU11BaseType.ZLEMA:
                    baseline = AuZLEMA(Input, basePeriod).Value;
                    break;
            }

            if (thisVolaType != AuSuperTrendU11VolaType.Standard_Deviation)
            {
                switch (thisOffsetType)
                {
                    case AuSuperTrendU11OffsetType.Default:
                        offsetSeries = AuEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Median:
                        offsetSeries = AuMovingMedian(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.ADXVMA:
                        offsetSeries = AuADXVMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Butterworth_2:
                        offsetSeries = AuButterworthFilter(rangeSeries, 2, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Butterworth_3:
                        offsetSeries = AuButterworthFilter(rangeSeries, 3, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.DEMA:
                        offsetSeries = AuDEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.DSMA:
                        offsetSeries = AuDSMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.DTMA:
                        offsetSeries = AuDTMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.DWMA:
                        offsetSeries = AuDWMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Ehlers:
                        offsetSeries = AuEhlersFilter(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.EMA:
                        offsetSeries = AuEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Gauss_2:
                        offsetSeries = AuGaussianFilter(rangeSeries, 2, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Gauss_3:
                        offsetSeries = AuGaussianFilter(rangeSeries, 3, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.Gauss_4:
                        offsetSeries = AuGaussianFilter(rangeSeries, 4, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.HMA:
                        offsetSeries = AuHMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.HoltEMA:
                        offsetSeries = AuHoltEMA(rangeSeries, rangePeriod, 2 * rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.LinReg:
                        offsetSeries = AuLinReg(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.LLMA:
                        offsetSeries = AuLLMA(rangeSeries, rangePeriod, 0.0).Value;
                        break;
                    case AuSuperTrendU11OffsetType.SMA:
                        offsetSeries = AuSMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.SuperSmoother_2:
                        offsetSeries = AuSuperSmootherFilter(rangeSeries, 2, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.SuperSmoother_3:
                        offsetSeries = AuSuperSmootherFilter(rangeSeries, 3, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.TEMA:
                        offsetSeries = AuTEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.TMA:
                        offsetSeries = AuTMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.TSMA:
                        offsetSeries = AuTSMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.TWMA:
                        offsetSeries = AuTWMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.VWMA:
                        offsetSeries = AuVWMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.WMA:
                        offsetSeries = AuWMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.ZeroLagHATEMA:
                        offsetSeries = AuZeroLagHATEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.ZeroLagTEMA:
                        offsetSeries = AuZeroLagTEMA(rangeSeries, rangePeriod).Value;
                        break;
                    case AuSuperTrendU11OffsetType.ZLEMA:
                        offsetSeries = AuZLEMA(rangeSeries, rangePeriod).Value;
                        break;
                }
            }
        }

        #endregion

        #region OnFirstTickOfBar

        private void vOnFirstTickOfBar()
        {
            displacement = Math.Max(Displacement, -CurrentBar);
            movingBase = baseline[1];
            offset = Math.Max(TickSize, offsetSeries[1]);
            trailingAmount = multiplier * offset;
            margin = volatility[1];

            if (currentUpTrend)
            {
                currentStopShort = movingBase + trailingAmount;

                if (priorUpTrend)
                    currentStopLong = Math.Max(currentStopLong, movingBase - trailingAmount);
                else
                    currentStopLong = movingBase - trailingAmount;

                StopDot[0] = currentStopLong;
                ReverseDot[0] = currentStopShort;
                PlotBrushes[0][-displacement] = upColor;

                if (showStopLine)
                {
                    StopLine[0] = currentStopLong;

                    if (gap && !priorUpTrend)
                        PlotBrushes[1][-displacement] = Brushes.Transparent;
                    else
                        PlotBrushes[1][-displacement] = upColor;
                }
                else
                    StopLine.Reset();
            }
            else
            {
                currentStopLong = movingBase - trailingAmount;

                if (!priorUpTrend)
                    currentStopShort = Math.Min(currentStopShort, movingBase + trailingAmount);
                else
                    currentStopShort = movingBase + trailingAmount;

                StopDot[0] = currentStopShort;
                ReverseDot[0] = currentStopLong;
                PlotBrushes[0][-displacement] = downColor;

                if (showStopLine)
                {
                    StopLine[0] = currentStopShort;

                    if (gap && priorUpTrend)
                        PlotBrushes[1][-displacement] = Brushes.Transparent;
                    else
                        PlotBrushes[1][-displacement] = downColor;
                }
                else
                    StopLine.Reset();
            }

            if (showPaintBars)
            {
                if (currentUpTrend)
                    trendColor = upColor;
                else
                    trendColor = downColor;

                CandleOutlineBrushes[-displacement] = trendColor;
                BarBrushes[-displacement] = trendColor;
            }

            if (showArrows && !reverseIntraBar)
            {
                if (currentUpTrend && !priorUpTrend)
                    Draw.ArrowUp(this, "arrow" + CurrentBar, true, -displacement, currentStopLong - 0.5 * margin, upColor);
                else if (!currentUpTrend && priorUpTrend)
                    Draw.ArrowDown(this,"arrow" + CurrentBar, true, -displacement, currentStopShort + 0.5 * margin, downColor);
            }

            priorUpTrend = currentUpTrend;
            stoppedOut = false;
            
        }

        #endregion

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StopDot
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StopLine
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Trend
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ReverseDot
        {
            get { return reverseDot; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> UpTrend
        {
            get { return upTrend; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Baseline smoothing", Description = "Moving average type for baseline", Order = 0, GroupName = "Gen. Options")]
        public AuSuperTrendU11BaseType ThisBaseType
        {
            get { return thisBaseType; }
            set { thisBaseType = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Offset smoothing", Description = "Moving average type for volatility estimator", Order = 1, GroupName = "Gen. Options")]
        public AuSuperTrendU11OffsetType ThisOffsetType
        {
            get { return thisOffsetType; }
            set { thisOffsetType = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Offset type", Description = "Simple or True Range", Order = 2, GroupName = "Gen. Options")]
        public AuSuperTrendU11VolaType ThisRangeType
        {
            get { return thisVolaType; }
            set { thisVolaType = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Reverse intra-bar", Description = "Reverse intra-bar", Order = 3, GroupName = "Gen. Options")]
        public bool ReverseIntraBar
        {
            get { return reverseIntraBar; }
            set { reverseIntraBar = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Baseline period", Description = "Median period", Order = 0, GroupName = "Gen. Parameters")]
        public int BasePeriod
        {
            get { return basePeriod; }
            set { basePeriod = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Offset multiplier", Description = "ATR multiplier", Order = 1, GroupName = "Gen. Parameters")]
        public double Multiplier
        {
            get { return multiplier; }
            set { multiplier = Math.Max(0.0, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Offset period", Description = "ATR period", Order = 2, GroupName = "Gen. Parameters")]
        public int RangePeriod
        {
            get { return rangePeriod; }
            set { rangePeriod = Math.Max(1, value); }
        }

        [Display(Name = "Show arrows", Description = "Show arrows when trendline is violated?", Order = 0, GroupName = "Gen. Plot & Sound")]
        public bool ShowArrows
        {
            get { return showArrows; }
            set { showArrows = value; }
        }

        [Display(Name = "Show paintbars", Description = "Color the bars in the direction of the trend?", Order = 1, GroupName = "Gen. Plot & Sound")]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }

        [Display(Name = "Show stop line", Description = "Show stop line", Order = 2, GroupName = "Gen. Plot & Sound")]
        public bool ShowStopLine
        {
            get { return showStopLine; }
            set { showStopLine = value; }
        }

        [Display(Name = "Sound alert active", Description = "Sound alerts activated", Order = 3, GroupName = "Gen. Plot & Sound")]
        public bool SoundAlert
        {
            get { return soundAlert; }
            set { soundAlert = value; }
        }

        [XmlIgnore]
        [Display(Name = "Uptrend", Description = "Select color for uptrend", Order = 0, GroupName = "Gen. Plot Colors")]
        public Brush UpColor
        {
            get { return upColor; }
            set { upColor = value; }
        }

        [Browsable(false)]
        public string UpColorSerialize
        {
            get { return Serialize.BrushToString(upColor); }
            set { upColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Downtrend", Description = "Select color for downtrend", Order = 1, GroupName = "Gen. Plot Colors")]
        public Brush DownColor
        {
            get { return downColor; }
            set { downColor = value; }
        }

        [Browsable(false)]
        public string DownColorSerialize
        {
            get { return Serialize.BrushToString(downColor); }
            set { downColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "DashStyle stop dots", Description = "DashStyle for stop dots", Order = 0, GroupName = "Gen. Plot Params")]
        public DashStyleHelper Dash0Style
        {
            get { return dash0Style; }
            set { dash0Style = value; }
        }

        [Display(Name = "DashStyle stop line", Description = "DashStyle for stop line", Order = 1, GroupName = "Gen. Plot Params")]
        public DashStyleHelper Dash1Style
        {
            get { return dash1Style; }
            set { dash1Style = value; }
        }

        [Display(Name = "PlotStyle stop dots", Description = "PlotStyle for stop dots", Order = 2, GroupName = "Gen. Plot Params")]
        public PlotStyle Plot0Style
        {
            get { return plot0Style; }
            set { plot0Style = value; }
        }

        [Display(Name = "PlotStyle stop line", Description = "PlotStyle for stop line", Order = 3, GroupName = "Gen. Plot Params")]
        public PlotStyle Plot1Style
        {
            get { return plot1Style; }
            set { plot1Style = value; }
        }

        [Display(Name = "Upclose opacity", Description = "When paint bars are activated, this parameter sets the opacity of the upclose bars", Order = 4, GroupName = "Gen. Plot Params")]
        public int Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        [Display(Name = "Width stop dots", Description = "Width for stop dots", Order = 5, GroupName = "Gen. Plot Params")]
        public int Plot0Width
        {
            get { return plot0Width; }
            set { plot0Width = Math.Max(1, value); }
        }

        [Display(Name = "Width stop line", Description = "Width for stop line", Order = 6, GroupName = "Gen. Plot Params")]
        public int Plot1Width
        {
            get { return plot1Width; }
            set { plot1Width = Math.Max(1, value); }
        }

        [Display(Name = "New downtrend", Description = "Sound file for confirmed new downtrend", Order = 0, GroupName = "Gen. Sound Alerts")]
        public string ConfirmedDownTrend
        {
            get { return confirmedDownTrend; }
            set { confirmedDownTrend = value; }
        }

        [Display(Name = "New uptrend", Description = "Sound file for confirmed new uptrend", Order = 1, GroupName = "Gen. Sound Alerts")]
        public string ConfirmedUpTrend
        {
            get { return confirmedUpTrend; }
            set { confirmedUpTrend = value; }
        }

        [Display(Name = "Potential downtrend", Description = "Sound file for potential new downtrend", Order = 2, GroupName = "Gen. Sound Alerts")]
        public string PotentialDownTrend
        {
            get { return potentialDownTrend; }
            set { potentialDownTrend = value; }
        }

        [Display(Name = "Potential uptrend", Description = "Sound file for potential new uptrend", Order = 3, GroupName = "Gen. Sound Alerts")]
        public string PotentialUpTrend
        {
            get { return potentialUpTrend; }
            set { potentialUpTrend = value; }
        }

        [Display(Name = "Rearm time (sec)", Description = "Rearm time for alert in seconds", Order = 4, GroupName = "Gen. Sound Alerts")]
        public int RearmTime
        {
            get { return rearmTime; }
            set { rearmTime = value; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AuSuperTrendU11[] cacheAuSuperTrendU11;
		public AuSuperTrendU11 AuSuperTrendU11(AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			return AuSuperTrendU11(Input, thisBaseType, thisOffsetType, thisRangeType, reverseIntraBar, basePeriod, multiplier, rangePeriod);
		}

		public AuSuperTrendU11 AuSuperTrendU11(ISeries<double> input, AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			if (cacheAuSuperTrendU11 != null)
				for (int idx = 0; idx < cacheAuSuperTrendU11.Length; idx++)
					if (cacheAuSuperTrendU11[idx] != null && cacheAuSuperTrendU11[idx].ThisBaseType == thisBaseType && cacheAuSuperTrendU11[idx].ThisOffsetType == thisOffsetType && cacheAuSuperTrendU11[idx].ThisRangeType == thisRangeType && cacheAuSuperTrendU11[idx].ReverseIntraBar == reverseIntraBar && cacheAuSuperTrendU11[idx].BasePeriod == basePeriod && cacheAuSuperTrendU11[idx].Multiplier == multiplier && cacheAuSuperTrendU11[idx].RangePeriod == rangePeriod && cacheAuSuperTrendU11[idx].EqualsInput(input))
						return cacheAuSuperTrendU11[idx];
			return CacheIndicator<AuSuperTrendU11>(new AuSuperTrendU11(){ ThisBaseType = thisBaseType, ThisOffsetType = thisOffsetType, ThisRangeType = thisRangeType, ReverseIntraBar = reverseIntraBar, BasePeriod = basePeriod, Multiplier = multiplier, RangePeriod = rangePeriod }, input, ref cacheAuSuperTrendU11);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuSuperTrendU11 AuSuperTrendU11(AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			return indicator.AuSuperTrendU11(Input, thisBaseType, thisOffsetType, thisRangeType, reverseIntraBar, basePeriod, multiplier, rangePeriod);
		}

		public Indicators.AuSuperTrendU11 AuSuperTrendU11(ISeries<double> input , AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			return indicator.AuSuperTrendU11(input, thisBaseType, thisOffsetType, thisRangeType, reverseIntraBar, basePeriod, multiplier, rangePeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuSuperTrendU11 AuSuperTrendU11(AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			return indicator.AuSuperTrendU11(Input, thisBaseType, thisOffsetType, thisRangeType, reverseIntraBar, basePeriod, multiplier, rangePeriod);
		}

		public Indicators.AuSuperTrendU11 AuSuperTrendU11(ISeries<double> input , AuSuperTrendU11BaseType thisBaseType, AuSuperTrendU11OffsetType thisOffsetType, AuSuperTrendU11VolaType thisRangeType, bool reverseIntraBar, int basePeriod, double multiplier, int rangePeriod)
		{
			return indicator.AuSuperTrendU11(input, thisBaseType, thisOffsetType, thisRangeType, reverseIntraBar, basePeriod, multiplier, rangePeriod);
		}
	}
}

#endregion
