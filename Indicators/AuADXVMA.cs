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
	public class AuADXVMA : Indicator
	{
        #region Variables
        private int period = 6;
        private double k = 0.0;
        private double hhp = 0.0;
        private double llp = 0.0;
        private double hhv = 0.0;
        private double llv = 0.0;
        private double epsilon = 0.0;
        private bool showPlot = true;
        private bool showPaintBars = true;

        private int opacity = 4;
        private int alpha = 0;
        private int plot0Width = 2;
        private DashStyleHelper dash0Style = DashStyleHelper.Solid;
        private PlotStyle plot0Style = PlotStyle.Line;

        private Brush upColor = Brushes.Lime;
        private Brush neutralColor = Brushes.Tan;
        private Brush downColor = Brushes.Red;

        private Series<double> up;
        private Series<double> down;
        private Series<double> ups;
        private Series<double> downs;
        private Series<double> index;
        private Series<int> trend;
        private ATR volatility;

        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AuADXVMA";
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

                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "ADXVMA");
            }
			else if (State == State.Configure)
			{
                up = new Series<double>(this);
                down = new Series<double>(this);
                ups = new Series<double>(this);
                downs = new Series<double>(this);
                index = new Series<double>(this);
                trend = new Series<int>(this);
            }
		}

		protected override void OnBarUpdate()
		{
			//OnStartup
            if (CurrentBar < 1)
            {
                k = 1.0 / (double)period;
                volatility = ATR(200);
                Plots[0].PlotStyle = plot0Style;
                Plots[0].Width = plot0Width;
                Plots[0].DashStyleHelper = dash0Style;

                if (showPlot)
                    Plots[0].Brush = Brushes.Gray;
                else
                    Plots[0].Brush = Brushes.Transparent;

                alpha = 25 * opacity;

                up[0] = 0.0;
                down[0] = 0.0;
                ups[0] = 0.0;
                downs[0] = 0.0;
                index[0] = 0.0;
                trend[0] = 0;
                ADXVMA[0] = Input[0];
            }
            else
            {
                double currentUp = Math.Max(Input[0] - Input[1], 0);
                double currentDown = Math.Max(Input[1] - Input[0], 0);
                up[0] = (1 - k) * up[1] + k * currentUp;
                down[0] = (1 - k) * down[1] + k * currentDown;

                double sum = up[0] + down[0];
                double fractionUp = 0.0;
                double fractionDown = 0.0;
                if (sum > double.Epsilon)
                {
                    fractionUp = up[0] / sum;
                    fractionDown = down[0] / sum;
                }
                ups[0] = (1 - k) * ups[1] + k * fractionUp;
                downs[0] = (1 - k) * downs[1] + k * fractionDown;

                double normDiff = Math.Abs(ups[0] - downs[0]);
                double normSum = ups[0] + downs[0];
                double normFraction = 0.0;
                if (normSum > double.Epsilon)
                    normFraction = normDiff / normSum;
                index[0] = (1 - k) * index[1] + k * normFraction;

                if (IsFirstTickOfBar)
                {
                    epsilon = 0.1 * volatility[1];
                    hhp = MAX(index, period)[1];
                    llp = MIN(index, period)[1];
                }
                hhv = Math.Max(index[0], hhp);
                llv = Math.Min(index[0], llp);

                double vDiff = hhv - llv;
                double vIndex = 0;
                if (vDiff > double.Epsilon)
                    vIndex = (index[0] - llv) / vDiff;

                ADXVMA[0] = (1 - k * vIndex) * ADXVMA[1] + k * vIndex * Input[0];

                if (trend[1] > -1 && ADXVMA[0] > ADXVMA[1] + epsilon)
                {
                    trend[0] = 1;
                    PlotBrushes[0][0] = upColor;
                }
                else if (trend[1] < 1 && ADXVMA[0] < ADXVMA[1] - epsilon)
                {
                    trend[0] = -1;
                    PlotBrushes[0][0] = downColor;
                }
                else
                {
                    trend[0] = 0;
                    PlotBrushes[0][0] = neutralColor;
                }

                if (showPaintBars)
                {
                    if (trend[0] == 1)
                    {
                        BarBrushes[0] = upColor;
                        CandleOutlineBrushes[0] = upColor;
                    }
                    else if (trend[0] == -1)
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

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)alpha, r, g, b));
                    }
                }
            }
		}


        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ADXVMA
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> Trend
        {
            get { return trend; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "ADXVMA Period", Order = 0, GroupName = "Gen. Parameters")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
        }

        [Display(Name = "Show PaintBars", Description = "Show paint bars on price panel", Order = 0, GroupName = "Options")]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }

        [Display(Name = "Show Plot", Description = "Show plot of the ADXVMA average", Order = 1, GroupName = "Options")]
        public bool ShowPlot
        {
            get { return showPlot; }
            set { showPlot = value; }
        }

        [Display(Name = "Dash Style", Description = "DashStyle for ADXVMA plot", Order = 0, GroupName = "Plot Parameters")]
        public DashStyleHelper Dash0Style
        {
            get { return dash0Style; }
            set { dash0Style = value; }
        }

        [Display(Name = "Line Width", Description = "Width for ADXVMA plot", Order = 1, GroupName = "Plot Parameters")]
        public int Plot0Width
        {
            get { return plot0Width; }
            set { plot0Width = Math.Max(1, value); }
        }

        [Display(Name = "Plot Style", Description = "PlotStyle for ADXVMA plot", Order = 2, GroupName = "Plot Parameters")]
        public PlotStyle Plot0Style
        {
            get { return plot0Style; }
            set { plot0Style = value; }
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

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AuADXVMA[] cacheAuADXVMA;
		public AuADXVMA AuADXVMA(int period)
		{
			return AuADXVMA(Input, period);
		}

		public AuADXVMA AuADXVMA(ISeries<double> input, int period)
		{
			if (cacheAuADXVMA != null)
				for (int idx = 0; idx < cacheAuADXVMA.Length; idx++)
					if (cacheAuADXVMA[idx] != null && cacheAuADXVMA[idx].Period == period && cacheAuADXVMA[idx].EqualsInput(input))
						return cacheAuADXVMA[idx];
			return CacheIndicator<AuADXVMA>(new AuADXVMA(){ Period = period }, input, ref cacheAuADXVMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AuADXVMA AuADXVMA(int period)
		{
			return indicator.AuADXVMA(Input, period);
		}

		public Indicators.AuADXVMA AuADXVMA(ISeries<double> input , int period)
		{
			return indicator.AuADXVMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AuADXVMA AuADXVMA(int period)
		{
			return indicator.AuADXVMA(Input, period);
		}

		public Indicators.AuADXVMA AuADXVMA(ISeries<double> input , int period)
		{
			return indicator.AuADXVMA(input, period);
		}
	}
}

#endregion
