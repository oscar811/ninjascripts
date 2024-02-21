#region Assembly LuxAlgo Indicator, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null
// C:\Users\sshrestha\Documents\NinjaTrader 8\bin\Custom\LuxAlgo - SuperTrendAIClustering.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.Indicators.LuxAlgo;

namespace NinjaTrader.NinjaScript.Indicators.LuxAlgo2
{
    public class SuperTrendAIClustering2 : Indicator
    {
        private class supertrend
        {
            public double upper;

            public double lower;

            public double output;

            public double perf;

            public double factor;

            public int trend;

            public supertrend(double upper, double lower, double output = 0.0, double perf = 0.0, double factor = 0.0, int trend = 0)
            {
                this.upper = upper;
                this.lower = lower;
                this.output = output;
                this.perf = perf;
                this.factor = factor;
                this.trend = trend;
            }
        }

        private class vector
        {
            public double[] Out;

            public vector()
            {
                Out = new double[0];
            }
        }

        private supertrend[] holder;

        private double[] factors;

        private double[] centroids;

        private vector[] factors_clusters;

        private vector[] perfclusters;

        private double target_factor;

        private double perf_idx;

        private double perf_ama;

        private int from;

        private int RenderCount;

        private Series<double> EMASeries;

        private Series<double> upper;

        private Series<double> lower;

        private Series<int> os;

        private Brush amaBullCss_;

        private Brush amaBearCss_;

        private SimpleFont FontMe;

        private PineTable tb;

        private static PineLib Pine;

        public override string DisplayName => "LuxAlgo - SuperTrend AI (Clustering)";

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Length", Order = 1, GroupName = "1. Parameters")]
        public int length { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Factor Range Min", Order = 2, GroupName = "1. Parameters")]
        public int minMult { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Factor Range Max", Order = 3, GroupName = "1. Parameters")]
        public int maxMult { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Step", Order = 4, GroupName = "1. Parameters")]
        public double step { get; set; }

        [NinjaScriptProperty]
        [Range(2.0, double.MaxValue)]
        [Display(Name = "Performance Memory", Order = 5, GroupName = "1. Parameters")]
        public double perfAlpha { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "From Cluster", Order = 6, GroupName = "1. Parameters")]
        public LuxSTAIFromCluster fromCluster { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Maximum Iteration Steps", Order = 7, GroupName = "2. Optimization")]
        public int maxIter { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Historical Bars Calculation", Order = 8, GroupName = "2. Optimization")]
        public int maxData { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trailing Stop Bear", Order = 9, GroupName = "3. Style")]
        public Brush bearCss { get; set; }

        [Browsable(false)]
        public string bearCssSerializable
        {
            get
            {
                return Serialize.BrushToString(bearCss);
            }
            set
            {
                bearCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Trailing Stop Bull", Order = 10, GroupName = "3. Style")]
        public Brush bullCss { get; set; }

        [Browsable(false)]
        public string bullCssSerializable
        {
            get
            {
                return Serialize.BrushToString(bullCss);
            }
            set
            {
                bullCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "AMA Bear", Order = 11, GroupName = "3. Style")]
        public Brush amaBearCss { get; set; }

        [Browsable(false)]
        public string amaBearCssSerializable
        {
            get
            {
                return Serialize.BrushToString(amaBearCss);
            }
            set
            {
                amaBearCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "AMA Bull", Order = 12, GroupName = "3. Style")]
        public Brush amaBullCss { get; set; }

        [Browsable(false)]
        public string amaBullCssSerializable
        {
            get
            {
                return Serialize.BrushToString(amaBullCss);
            }
            set
            {
                amaBullCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Signals", Order = 14, GroupName = "3. Style")]
        public bool showSignals { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Dashboard", Order = 15, GroupName = "4. Dashboard")]
        public bool showDash { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Location", Order = 16, GroupName = "4. Dashboard")]
        public LuxTablePosition dashLoc { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Size", Order = 17, GroupName = "4. Dashboard")]
        public int textSize { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public double hl2 => (base.High[0] + base.Low[0]) / 2.0;

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ts => base.Values[0];

        protected override void OnStateChange()
        {
            if (base.State == State.SetDefaults)
            {
                base.Description = "Enter the description for your new custom Indicator here.";
                base.Name = "SuperTrend AI Clustering";
                base.Calculate = Calculate.OnBarClose;
                base.IsOverlay = true;
                base.DisplayInDataBox = true;
                base.DrawOnPricePanel = true;
                base.DrawHorizontalGridLines = true;
                base.DrawVerticalGridLines = true;
                base.PaintPriceMarkers = true;
                base.ScaleJustification = ScaleJustification.Right;
                base.IsSuspendedWhileInactive = true;
                length = 10;
                minMult = 1;
                maxMult = 5;
                step = 0.5;
                perfAlpha = 10.0;
                fromCluster = LuxSTAIFromCluster.Best;
                maxIter = 1000;
                maxData = 10000;
                bearCss = Brushes.Crimson;
                bullCss = Brushes.Teal;
                amaBearCss = Brushes.Crimson;
                amaBullCss = Brushes.Teal;
                showSignals = true;
                showDash = true;
                dashLoc = LuxTablePosition.TopRight;
                textSize = 12;
                AddPlot(Brushes.DodgerBlue, "Trailing Stop");
                AddPlot(Brushes.DodgerBlue, "Trailing Stop AMA");
            }
            else if (base.State != State.Configure && base.State == State.DataLoaded)
            {
                Pine = new PineLib(this, this, base.DrawObjects);
                holder = new supertrend[0];
                factors = new double[0];
                factors_clusters = new vector[0];
                perfclusters = new vector[0];
                target_factor = 0.0;
                perf_idx = 0.0;
                perf_ama = 0.0;
                EMASeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                upper = new Series<double>(this, MaximumBarsLookBack.Infinite);
                lower = new Series<double>(this, MaximumBarsLookBack.Infinite);
                os = new Series<int>(this, MaximumBarsLookBack.Infinite);
                amaBearCss_ = amaBearCss.CloneCurrentValue();
                amaBearCss_.Opacity = 0.5;
                amaBearCss_.Freeze();
                amaBullCss_ = amaBullCss.CloneCurrentValue();
                amaBullCss_.Opacity = 0.5;
                amaBullCss_.Freeze();
                for (int i = 0; i <= (int)((double)(maxMult - minMult) / step); i++)
                {
                    Pine.Array.PushElement(ref factors, (double)minMult + (double)i * step);
                    Pine.Array.PushElement(ref holder, new supertrend(hl2, hl2));
                }

                switch (fromCluster)
                {
                    case LuxSTAIFromCluster.Best:
                        from = 2;
                        break;
                    case LuxSTAIFromCluster.Average:
                        from = 1;
                        break;
                    case LuxSTAIFromCluster.Worst:
                        from = 0;
                        break;
                }

                if (showDash)
                {
                    tb = new PineTable(this);
                    PineTable pineTable = tb;
                    LuxTablePosition tablePosition = dashLoc;
                    SharpDX.Color? borderColor = SharpDX.Color.Gray;
                    pineTable.New(tablePosition, 4, 4, null, null, 0, borderColor, 1);
                    tb.SetCell(0, 0, "Cluster", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(0, 1, "Best", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(0, 2, "Average", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(0, 3, "Worst", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(1, 0, "Size", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(2, 0, "CD", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    tb.SetCell(3, 0, "Factors", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                    RenderCount = 0;
                }

                FontMe = new SimpleFont("Arial", textSize);
            }
        }

        protected override void OnBarUpdate()
        {
            if (base.CurrentBar < 1)
            {
                upper[0] = hl2;
                lower[0] = hl2;
                os[0] = 0;
                return;
            }

            double num = ATR(length)[0];
            int num2 = 0;
            double[] array = factors;
            foreach (double num3 in array)
            {
                double num4 = hl2 + num * num3;
                double num5 = hl2 - num * num3;
                holder[num2].trend = ((base.Close[0] > holder[num2].upper) ? 1 : ((!(base.Close[0] < holder[num2].lower)) ? holder[num2].trend : 0));
                holder[num2].upper = ((base.Close[1] < holder[num2].upper) ? Math.Min(num4, holder[num2].upper) : num4);
                holder[num2].lower = ((base.Close[1] > holder[num2].lower) ? Math.Max(num5, holder[num2].lower) : num5);
                double num6 = Pine.Nz(Math.Sign(base.Close[1] - holder[num2].output));
                holder[num2].perf += 2.0 / (perfAlpha + 1.0) * (Pine.Nz(base.Close[0] - base.Close[1]) * num6 - holder[num2].perf);
                holder[num2].output = ((holder[num2].trend == 1) ? holder[num2].lower : holder[num2].upper);
                holder[num2].factor = num3;
                num2++;
            }

            double[] array2 = new double[0];
            double[] array3 = new double[0];
            if (base.Count - base.CurrentBar <= maxData)
            {
                for (int j = 0; j < holder.Length; j++)
                {
                    Pine.Array.PushElement(ref array3, holder[j].perf);
                    Pine.Array.PushElement(ref array2, holder[j].factor);
                }
            }

            centroids = new double[0];
            Pine.Array.PushElement(ref centroids, Pine.Array.PercentileLinearInterpolation(array3, 25.0));
            Pine.Array.PushElement(ref centroids, Pine.Array.PercentileLinearInterpolation(array3, 50.0));
            Pine.Array.PushElement(ref centroids, Pine.Array.PercentileLinearInterpolation(array3, 75.0));
            if (base.Count - base.CurrentBar <= maxData)
            {
                for (int k = 0; k < maxIter; k++)
                {
                    factors_clusters = new vector[3]
                    {
                    new vector(),
                    new vector(),
                    new vector()
                    };
                    perfclusters = new vector[3]
                    {
                    new vector(),
                    new vector(),
                    new vector()
                    };
                    int num7 = 0;
                    array = array3;
                    foreach (double num8 in array)
                    {
                        double[] array4 = new double[0];
                        double[] array5 = centroids;
                        foreach (double num9 in array5)
                        {
                            Pine.Array.PushElement(ref array4, Math.Abs(num8 - num9));
                        }

                        int num10 = Pine.Array.IndexOfElement(ref array4, Pine.Array.MinArrayValue(ref array4));
                        Pine.Array.PushElement(ref perfclusters[num10].Out, num8);
                        Pine.Array.PushElement(ref factors_clusters[num10].Out, array2[num7]);
                        num7++;
                    }

                    double[] array6 = new double[0];
                    for (int m = 0; m < perfclusters.Length; m++)
                    {
                        Pine.Array.PushElement(ref array6, Pine.Array.AverageArrayElements(perfclusters[m].Out));
                    }

                    if (array6[0] == centroids[0] && array6[1] == centroids[1] && array6[2] == centroids[2])
                    {
                        break;
                    }

                    centroids = array6.ToArray();
                }
            }

            EMASeries[0] = Math.Abs(base.Close[0] - base.Close[1]);
            double num11 = EMA(EMASeries, (int)perfAlpha)[0];
            if (perfclusters.Length != 0)
            {
                target_factor = Pine.Nz(Pine.Array.AverageArrayElements(factors_clusters[from].Out), target_factor);
                perf_idx = Math.Max(Pine.Nz(Pine.Array.AverageArrayElements(perfclusters[from].Out)), 0.0) / num11;
            }

            double num12 = hl2 + num * target_factor;
            double num13 = hl2 - num * target_factor;
            upper[0] = ((base.Close[1] < upper[1]) ? Math.Min(num12, upper[1]) : num12);
            lower[0] = ((base.Close[1] > lower[1]) ? Math.Max(num13, lower[1]) : num13);
            os[0] = ((base.Close[0] > upper[0]) ? 1 : ((!(base.Close[0] < lower[0])) ? os[1] : 0));
            ts[0] = ((os[0] == 1) ? lower[0] : upper[0]);
            if (!ts.IsValidDataPoint(1))
            {
                perf_ama = ts[0];
            }
            else
            {
                perf_ama += perf_idx * (ts[0] - perf_ama);
            }

            base.Values[1][0] = perf_ama;
            base.PlotBrushes[0][0] = ((os[0] != os[1]) ? Brushes.Transparent : ((os[0] == 1) ? bullCss : bearCss));
            base.PlotBrushes[1][0] = (Pine.TA.Cross(base.Close, base.Values[1]) ? Brushes.Transparent : ((base.Close[0] > perf_ama) ? amaBullCss_ : amaBearCss_));
            if (showSignals && os[0] != os[1])
            {
                if (os[0] == 1)
                {
                    Pine.Label.New(base.CurrentBar, ts[0], ((int)(perf_idx * 10.0)).ToString(), bullCss, null, 100, -1, Brushes.White, FontMe, TextAlignment.Center, textSize);
                }
                else
                {
                    Pine.Label.New(base.CurrentBar, ts[0], ((int)(perf_idx * 10.0)).ToString(), bearCss, null, 100, 1, Brushes.White, FontMe, TextAlignment.Center, textSize);
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);
            if (base.Bars == null || base.ChartControl == null || !showDash)
            {
                return;
            }

            if (base.Count != RenderCount && Pine.BarState.IsLast)
            {
                double num = ((perfclusters.Length != 0) ? perfclusters[2].Out.Length : 0);
                double num2 = ((perfclusters.Length != 0) ? perfclusters[1].Out.Length : 0);
                double num3 = ((perfclusters.Length != 0) ? perfclusters[0].Out.Length : 0);
                tb.SetCell(1, 1, num.ToString(), 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                tb.SetCell(1, 2, num2.ToString(), 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                tb.SetCell(1, 3, num3.ToString(), 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                tb.SetCell(3, 1, (factors_clusters.Length != 0) ? (" [" + string.Join(", ", factors_clusters[2].Out.Select((double x) => x.ToString(CultureInfo.InvariantCulture))) + "]") : "[]", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignLeft, PineTable.TextVertical.AlignCenter, textSize);
                tb.SetCell(3, 2, (factors_clusters.Length != 0) ? (" [" + string.Join(", ", factors_clusters[1].Out.Select((double x) => x.ToString(CultureInfo.InvariantCulture))) + "]") : "[]", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignLeft, PineTable.TextVertical.AlignCenter, textSize);
                tb.SetCell(3, 3, (factors_clusters.Length != 0) ? (" [" + string.Join(", ", factors_clusters[0].Out.Select((double x) => x.ToString(CultureInfo.InvariantCulture))) + "]") : "[]", 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignLeft, PineTable.TextVertical.AlignCenter, textSize);
                for (int i = 0; i < perfclusters.Length; i++)
                {
                    double num4 = 0.0;
                    if (perfclusters[i].Out.Length > 1)
                    {
                        double[] @out = perfclusters[i].Out;
                        foreach (double num5 in @out)
                        {
                            num4 += Math.Abs(num5 - centroids[i]);
                        }
                    }

                    switch (i)
                    {
                        case 0:
                            num4 /= num3;
                            break;
                        case 1:
                            num4 /= num2;
                            break;
                        case 2:
                            num4 /= num;
                            break;
                    }

                    tb.SetCell(2, 3 - i, num4.ToString("N4"), 0, 0, SharpDX.Color.White, PineTable.TextHorizontal.AlignCenter, PineTable.TextVertical.AlignCenter, textSize);
                }

                tb.Calculate();
            }

            RenderCount = base.Count;
            tb.Draw(base.RenderTarget, chartControl, chartScale);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LuxAlgo2.SuperTrendAIClustering2[] cacheSuperTrendAIClustering2;
		public LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			return SuperTrendAIClustering2(Input, length, minMult, maxMult, step, perfAlpha, fromCluster, maxIter, maxData, bearCss, bullCss, amaBearCss, amaBullCss, showSignals, showDash, dashLoc, textSize);
		}

		public LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(ISeries<double> input, int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			if (cacheSuperTrendAIClustering2 != null)
				for (int idx = 0; idx < cacheSuperTrendAIClustering2.Length; idx++)
					if (cacheSuperTrendAIClustering2[idx] != null && cacheSuperTrendAIClustering2[idx].length == length && cacheSuperTrendAIClustering2[idx].minMult == minMult && cacheSuperTrendAIClustering2[idx].maxMult == maxMult && cacheSuperTrendAIClustering2[idx].step == step && cacheSuperTrendAIClustering2[idx].perfAlpha == perfAlpha && cacheSuperTrendAIClustering2[idx].fromCluster == fromCluster && cacheSuperTrendAIClustering2[idx].maxIter == maxIter && cacheSuperTrendAIClustering2[idx].maxData == maxData && cacheSuperTrendAIClustering2[idx].bearCss == bearCss && cacheSuperTrendAIClustering2[idx].bullCss == bullCss && cacheSuperTrendAIClustering2[idx].amaBearCss == amaBearCss && cacheSuperTrendAIClustering2[idx].amaBullCss == amaBullCss && cacheSuperTrendAIClustering2[idx].showSignals == showSignals && cacheSuperTrendAIClustering2[idx].showDash == showDash && cacheSuperTrendAIClustering2[idx].dashLoc == dashLoc && cacheSuperTrendAIClustering2[idx].textSize == textSize && cacheSuperTrendAIClustering2[idx].EqualsInput(input))
						return cacheSuperTrendAIClustering2[idx];
			return CacheIndicator<LuxAlgo2.SuperTrendAIClustering2>(new LuxAlgo2.SuperTrendAIClustering2(){ length = length, minMult = minMult, maxMult = maxMult, step = step, perfAlpha = perfAlpha, fromCluster = fromCluster, maxIter = maxIter, maxData = maxData, bearCss = bearCss, bullCss = bullCss, amaBearCss = amaBearCss, amaBullCss = amaBullCss, showSignals = showSignals, showDash = showDash, dashLoc = dashLoc, textSize = textSize }, input, ref cacheSuperTrendAIClustering2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			return indicator.SuperTrendAIClustering2(Input, length, minMult, maxMult, step, perfAlpha, fromCluster, maxIter, maxData, bearCss, bullCss, amaBearCss, amaBullCss, showSignals, showDash, dashLoc, textSize);
		}

		public Indicators.LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(ISeries<double> input , int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			return indicator.SuperTrendAIClustering2(input, length, minMult, maxMult, step, perfAlpha, fromCluster, maxIter, maxData, bearCss, bullCss, amaBearCss, amaBullCss, showSignals, showDash, dashLoc, textSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			return indicator.SuperTrendAIClustering2(Input, length, minMult, maxMult, step, perfAlpha, fromCluster, maxIter, maxData, bearCss, bullCss, amaBearCss, amaBullCss, showSignals, showDash, dashLoc, textSize);
		}

		public Indicators.LuxAlgo2.SuperTrendAIClustering2 SuperTrendAIClustering2(ISeries<double> input , int length, int minMult, int maxMult, double step, double perfAlpha, LuxSTAIFromCluster fromCluster, int maxIter, int maxData, Brush bearCss, Brush bullCss, Brush amaBearCss, Brush amaBullCss, bool showSignals, bool showDash, LuxTablePosition dashLoc, int textSize)
		{
			return indicator.SuperTrendAIClustering2(input, length, minMult, maxMult, step, perfAlpha, fromCluster, maxIter, maxData, bearCss, bullCss, amaBearCss, amaBullCss, showSignals, showDash, dashLoc, textSize);
		}
	}
}

#endregion
