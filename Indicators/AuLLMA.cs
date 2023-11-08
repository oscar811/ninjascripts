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
	public class AuLLMA : Indicator
	{

        #region Variables
        private int length = 14;
        private double phase = 0;   // value between -100.0 & 100.0

        private double phaseParam, logParam, sqrtParam, sqrtDivider, lengthDivider; // calculated const

        private int lastBar = -1;
        private MidAvg midAvg;
        
        private Series<double> paramA;
        private Series<double> paramB;
        private Series<double> cycleDelta;
        private Series<double> avgDelta;
        private Series<double> fC0;
        private Series<double> fA8;
        private Series<double> fC8;

        private bool showPaintBars = true;
        private Brush upColor = Brushes.Lime;
        private Brush neutralColor = Brushes.Tan;
        private Brush downColor = Brushes.Red;
        private int opacity = 4;
        private int alphaBarClr = 0;
        private bool showPlot = true;

        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"LLMA (Low Lag Moving Average)";
				Name										= "AuLLMA";
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

                BarsRequiredToPlot = 30; // does not show first 30 bars of values

                ShowTransparentPlotsInDataBox = true;

                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "LLMA");
                AddPlot(new Stroke() { Brush = Brushes.Transparent, Width = 1, DashStyleHelper = DashStyleHelper.Solid }, PlotStyle.Dot, "Trend");
            }
			else if (State == State.Configure)
			{
                midAvg = new MidAvg();

                paramA = new Series<double>(this);
                paramB = new Series<double>(this);
                cycleDelta = new Series<double>(this);
                avgDelta = new Series<double>(this);
                fC0 = new Series<double>(this);
                fA8 = new Series<double>(this);
                fC8 = new Series<double>(this);
            }
		}

		protected override void OnBarUpdate()
		{
            bool commit = false;
            double sValue = Input[0];

            //OnStartup
            if (CurrentBar < 1)
            {
                alphaBarClr = 25 * opacity;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;

                double lengthParam = Math.Max(0.0000000001, (Length - 1) / 2.0);

                phaseParam = Phase < -100 ? 0.5 :
                         Phase > 100 ? 2.5 :
                         Phase / 100.0 + 1.5;

                logParam = Math.Max(0, Math.Log(Math.Sqrt(lengthParam)) / Math.Log(2.0) + 2.0);

                sqrtParam = Math.Sqrt(lengthParam) * logParam;
                sqrtDivider = sqrtParam / (sqrtParam + 1.0);
                lengthParam *= 0.9;
                lengthDivider = lengthParam / (lengthParam + 2.0);

                paramB[0] = sValue;
                paramA[0] = sValue;
                cycleDelta[0] = 0;
                avgDelta[0] = 0;
                LLMA[0] = sValue;
            }
            else
            {
                double absValue = Math.Max(Math.Abs(sValue - paramA[1]), Math.Abs(sValue - paramB[1]));
                double delta = absValue + 0.0000000001; // 1.0e-10;
                cycleDelta[0] = delta;

                // calc SMA(10) of cycleDelta
                int cycleLen = Math.Min(10, CurrentBar);  // currBar starts at 1 in this leg
                double deltaSum = avgDelta[1] * Math.Min(10, CurrentBar - 1);
                deltaSum -= cycleDelta[cycleLen];
                deltaSum += delta;
                double avgD = deltaSum / cycleLen;
                avgDelta[0] = avgD;

                double mAvg = midAvg.Add(avgD, CurrentBar > 127 ? avgDelta[127] : 0, commit);

                if (CurrentBar <= 30)
                {
                    // initial bars
                    paramA[0] = (sValue - paramA[1]) > 0 ? sValue : sValue - (sValue - paramA[1]) * sqrtDivider;
                    paramB[0] = (sValue - paramB[1]) < 0 ? sValue : sValue - (sValue - paramB[1]) * sqrtDivider;

                    if (CurrentBar == 30)
                    {
                        // last init bar, init vars for main code
                        fC0[0] = Input[0];
                        int intPart = (Math.Ceiling(sqrtParam) >= 1.0) ? (int)Math.Ceiling(sqrtParam) : 1;
                        int dnShift, leftInt = intPart;
                        intPart = (Math.Floor(sqrtParam) >= 1.0) ? (int)Math.Floor(sqrtParam) : 1;
                        int upShift, rightPart = intPart;
                        double dValue = (leftInt == rightPart) ? 1.0 : (sqrtParam - rightPart) / (leftInt - rightPart);
                        upShift = (rightPart <= 29) ? rightPart : 29;
                        dnShift = (leftInt <= 29) ? leftInt : 29;
                        fA8[0] = (Input[0] - Input[upShift]) * (1 - dValue) / rightPart + (Input[0] - Input[dnShift]) * dValue / leftInt;
                    }
                    LLMA[0] = sValue;
                }
                else
                {
                    double powerValue = (0.5 <= logParam - 2.0) ? logParam - 2.0 : 0.5;
                    double dValue = logParam >= Math.Pow(absValue / mAvg, powerValue) ? Math.Pow(absValue / mAvg, powerValue) : logParam;
                    if (dValue < 1) dValue = 1;
                    powerValue = Math.Pow(sqrtDivider, Math.Sqrt(dValue));

                    paramA[0] = (sValue - paramA[1]) > 0 ? sValue : sValue - (sValue - paramA[1]) * powerValue;
                    paramB[0] = (sValue - paramB[1]) < 0 ? sValue : sValue - (sValue - paramB[1]) * powerValue;

                    powerValue = Math.Pow(lengthDivider, dValue);
                    double squareValue = powerValue * powerValue;
                    fC0[0] = (1 - powerValue) * sValue + powerValue * fC0[1];
                    fC8[0] = (sValue - fC0[0]) * (1.0 - lengthDivider) + lengthDivider * fC8[1];
                    fA8[0] = (phaseParam * fC8[0] + fC0[0] - LLMA[1]) *
                             (powerValue * (-2.0) + squareValue + 1) + squareValue * fA8[1];
                    LLMA[0] = LLMA[1] + fA8[0];
                }
            }

            if (CurrentBar > 1)
            {
                Trend[0] = 0;
                if (LLMA[0] > LLMA[1])
                    Trend[0] = 1;
                else if (LLMA[0] < LLMA[1])
                    Trend[0] = -1;

                if (showPlot)
                {
                    if (Trend[0] == 1)
                        PlotBrushes[0][0] = upColor;
                    else if (Trend[0] == -1)
                        PlotBrushes[0][0] = downColor;
                    else if (Trend[0] == 0)
                        PlotBrushes[0][0] = neutralColor;
                }

                if (showPaintBars)
                {
                    if (Trend[0] == 1)
                    {
                        BarBrushes[0] = upColor;
                        CandleOutlineBrushes[0] = upColor;
                    }
                    else if (Trend[0] == -1)
                    {
                        BarBrushes[0] = downColor;
                        CandleOutlineBrushes[0] = downColor;
                    }
                    else
                    {
                        BarBrushes[0] = neutralColor;
                        CandleOutlineBrushes[0] = neutralColor;
                    }

                    if (Close[0] > Open[0])
                    {
                        byte g = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).B;

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)alphaBarClr, r, g, b));
                    }
                }
            }
        }

        #region MidAvg Helper Class
        private class MidAvg
        {
            // this class tracks a list of sorted values and maintains an average
            // of the middle of the sorted range (throws the tails out)
            private const int size = 128;            // must be multiple of 4
            private const int sizeM1 = size - 1;
            private const int uMid = size / 2;       // i.e. size==128 -> uMid=64
            private const int lMid = uMid - 1;       // i.e. size==128 -> lMid=63
            private const int uQuad = size / 4 * 3;  // i.e. size==128 -> uQuad=96
            private const int lQuad = size / 4;      // i.e. size==128 -> lQuad=32
                                                     // Note: in the above, uQuad should be decremented by -1 if the intent is
                                                     // to have half of the values in the avg, as the algo includes start..end inclusive,
                                                     // however, the algo was left as is to match the original code

            private int num;                   // number of items in list
            private double[] list;                  // storage for values
            private int left, right;           // left and right edges
            private int winStart, winEnd;      // begin and end of window on list

            private double sum;
            private double Avg { get { return _num == 0 ? 0 : _sum / (_winEnd - _winStart + 1); } }

            // the following are temp working vars between commits
            private int _num, _left, _right, _winStart, _winEnd, _insIndex, _rmIndex;
            private double _sum, _newValue;

            public MidAvg()
            {
                sum = 0; _sum = sum;
                num = 0; _num = num;
                left = uMid; _left = left;
                right = lMid; _right = right;
                list = new double[size];
                for (int i = 0; i < size; i++) list[i] = (i <= lMid) ? -1000000 : 1000000;
            }

            private void Commit()
            {
                if (_num != 0)
                {
                    // perform the insertion
                    if (_rmIndex == _insIndex)
                        list[_insIndex] = _newValue;
                    else if (_rmIndex < _insIndex)
                    {
                        for (int j = _rmIndex + 1; j <= (_insIndex - 1); j++) list[j - 1] = list[j];
                        list[_insIndex - 1] = _newValue;
                    }
                    else
                    {
                        for (int j = _rmIndex - 1; j >= _insIndex; j--) list[j + 1] = list[j];
                        list[_insIndex] = _newValue;
                    }

                    // update state
                    num = _num;
                    sum = _sum;
                    left = _left;
                    right = _right;
                    winStart = _winStart;
                    winEnd = _winEnd;
                }
            }

            public double Add(double newValue, double removeValue, bool commit)
            {
                int sIndex;    // search index

                if (commit) Commit();

                _newValue = newValue;

                if (num >= sizeM1)
                {
                    // buffer is full, so look for entry to be removed to make room
                    sIndex = uMid; _rmIndex = sIndex;
                    while (sIndex > 1)
                    {
                        if (list[_rmIndex] < removeValue) { sIndex /= 2; _rmIndex += sIndex; }
                        else if (list[_rmIndex] <= removeValue) { sIndex = 1; }
                        else { sIndex /= 2; _rmIndex -= sIndex; }
                    }
                }
                else
                {
                    // not full yet, insert entry, list grows out from middle, _rmIndex is next entry to use
                    if ((right + left) > sizeM1)
                    {
                        _left = left - 1; _rmIndex = _left;
                    }
                    else
                    {
                        _right = right + 1; _rmIndex = _right;
                    }
                    // keep track of window that we sum over
                    _winEnd = (_right > uQuad) ? uQuad : _right;
                    _winStart = (_left < lQuad) ? lQuad : _left;
                }

                // find insertion point in sorted list
                sIndex = uMid; _insIndex = sIndex;
                while (sIndex > 1)
                {
                    if (list[_insIndex] >= _newValue)
                    {
                        if (list[_insIndex - 1] <= _newValue) { sIndex = 1; }
                        else { sIndex /= 2; _insIndex -= sIndex; }
                    }
                    else { sIndex /= 2; _insIndex += sIndex; }
                    if ((_insIndex == sizeM1) && (_newValue > list[sizeM1])) _insIndex = size;
                }

                // adjust sum based on insertion/deletion points
                _sum = sum;

                // list additions
                if (_insIndex <= _rmIndex)
                {
                    if (_winStart <= _insIndex && _insIndex <= _winEnd)
                        _sum += _newValue;           // new value in the window
                    else if (_insIndex < _winStart && _winStart <= _rmIndex)
                        _sum += list[_winStart - 1]; // push new entry into list 
                }
                else if (_insIndex <= _winStart)
                {
                    if (_rmIndex <= _winEnd && (_winEnd + 1) < _insIndex)
                        _sum += list[_winEnd + 1];   // push entry down
                }
                else if (_insIndex <= (_winEnd + 1))
                    _sum += _newValue;               // insert into list or end slot when moved
                else if (_rmIndex <= _winEnd && (_winEnd + 1) < _insIndex)
                    _sum += list[_winEnd + 1];       // push entry down

                // list removals
                if (num > sizeM1 && _winStart <= _rmIndex && _rmIndex <= _winEnd)
                    _sum -= list[_rmIndex];          // remove entry from list
                else if (_insIndex <= _winEnd && _winEnd < _rmIndex)
                    _sum -= list[_winEnd];           // push entry off end of list
                else if (_rmIndex < _winStart && _winStart < _insIndex)
                    _sum -= list[_winStart];         // push entry off start of list

                // do not perform the insertion, done in commit

                if (num <= sizeM1)
                {
                    _num = num + 1;
                }

                return Avg;
            }
        }
        #endregion

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LLMA
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Trend
        {
            get { return Values[1]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Length", Description = "Lookback interval", Order = 0, GroupName = "Gen. Parameters")]
        public int Length
        {
            get { return length; }
            set { length = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Phase", Description = "Phase (similar to offset), -100 to 100", Order = 1, GroupName = "Gen. Parameters")]
        public double Phase
        {
            get { return phase; }
            set { phase = Math.Min(Math.Max(-100, value), 100); }
        }

        [Display(Name = "Show PaintBars", Description = "Show paint bars on price panel", Order = 2, GroupName = "Gen. Parameters")]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }

        [XmlIgnore]
        [Display(Name = "Average Chop Mode", Description = "Select color for neutral average", Order = 0, GroupName = "Plot Colors")]
        public Brush NeutralColor
        {
            get { return neutralColor; }
            set { neutralColor = value; }
        }

        [Browsable(false)]
        public string NeutralColorSerialize
        {
            get { return Serialize.BrushToString(neutralColor); }
            set { neutralColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Average Falling", Description = "Select color for falling average", Order = 1, GroupName = "Plot Colors")]
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

        [XmlIgnore]
        [Display(Name = "Average Rising", Description = "Select color for rising average", Order = 2, GroupName = "Plot Colors")]
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

        [Display(Name = "Upclose Opacity", Description = "When paint bars are activated, this parameter sets the opacity of the upclose bars", Order = 3, GroupName = "Plot Colors")]
        public int Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        [Display(Name = "Show Plot", Description = "Show plot of the Zero-Lagging Heiken-Ashi TEMA", Order = 4, GroupName = "Plot Colors")]
        public bool ShowPlot
        {
            get { return showPlot; }
            set { showPlot = value; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AuLLMA[] cacheAuLLMA;
		public AuLLMA AuLLMA(int length, double phase)
		{
			return AuLLMA(Input, length, phase);
		}

		public AuLLMA AuLLMA(ISeries<double> input, int length, double phase)
		{
			if (cacheAuLLMA != null)
				for (int idx = 0; idx < cacheAuLLMA.Length; idx++)
					if (cacheAuLLMA[idx] != null && cacheAuLLMA[idx].Length == length && cacheAuLLMA[idx].Phase == phase && cacheAuLLMA[idx].EqualsInput(input))
						return cacheAuLLMA[idx];
			return CacheIndicator<AuLLMA>(new AuLLMA(){ Length = length, Phase = phase }, input, ref cacheAuLLMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuLLMA AuLLMA(int length, double phase)
		{
			return indicator.AuLLMA(Input, length, phase);
		}

		public Indicators.AuLLMA AuLLMA(ISeries<double> input , int length, double phase)
		{
			return indicator.AuLLMA(input, length, phase);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuLLMA AuLLMA(int length, double phase)
		{
			return indicator.AuLLMA(Input, length, phase);
		}

		public Indicators.AuLLMA AuLLMA(ISeries<double> input , int length, double phase)
		{
			return indicator.AuLLMA(input, length, phase);
		}
	}
}

#endregion
