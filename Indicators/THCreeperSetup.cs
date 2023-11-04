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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.TickHunterTA
{
	

	[Gui.CategoryOrder("0) Indicator Information", 0)]
	[Gui.CategoryOrder("Parameters", 1)]
	[Gui.CategoryOrder("Data Series", 2)]
	[Gui.CategoryOrder("Setup", 3)]
	[Gui.CategoryOrder("Visual", 4)]
	[Gui.CategoryOrder("Candle Palette", 5)]
	public class THCreeperSetup : Indicator
	{
		private const string SystemVersion = "v1.016";
		private const string SystemName = "THCreeperSetup";
		private const string FullSystemName = SystemName + " - " + SystemVersion;

		private Dictionary<string, DXMediaMap> dxmBrushes;

		private Series<double> macdEMAFast;
		private Series<double> macdEMASlow;

		private THStdDev stdDev;

		private double constant1;
		private double constant2;
		private double constant3;
		private double constant4;
		private double constant5;

		private double constant6;
		private EMA ma1Value;
		private EMA ma2Value;
		private MACD macdValue;

		const int LineChangePlotIndex = 2;
		const int LineBullishPlotIndex = 3;
		const int LineBearishPlotIndex = 4;

		const int AvgChangePlotIndex = 5;
		const int AvgBullishPlotIndex = 6;
		const int AvgBearishPlotIndex = 7;

		const int DotChangePlotIndex = 8;
		const int DotBullishPlotIndex = 9;
		const int DotBearishPlotIndex = 10;

		private Brush lineChangeColor;
		private Brush lineBullishColor;
		private Brush lineBearishColor;

		private Brush avgChangeColor;
		private Brush avgBullishColor;
		private Brush avgBearishColor;

		private Brush dotChangeColor;
		private Brush dotBullishColor;
		private Brush dotBearishColor;

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

				IsSuspendedWhileInactive = true;
				IsAutoScale = false;
				PaintPriceMarkers = false;
				DrawHorizontalGridLines = false;
				DrawVerticalGridLines = false;

				// Create default brushes
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "CloudAreaColor" })
					dxmBrushes.Add(brushName, new DXMediaMap());

				MACDFastPeriod = 12;
				MACDSlowPeriod = 26;
				MACDSmoothPeriod = 9;
				BBPeriod = 12;

				StdDevMultiplier = 1.0;

				UseLineCrossDotColorFilter = true;

				CloudColorOpacity = 26;
				CloudAreaColor = Brushes.Gray;

				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "BollingerUpperBand");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "BollingerLowerBand");

				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "LineChange");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "LineBullish");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "LineBearish");

				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "AvgChange");
				AddPlot(new Stroke(Brushes.LimeGreen, 1), PlotStyle.Line, "AvgBullish");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "AvgBearish");

				AddPlot(new Stroke(Brushes.DimGray, 3), PlotStyle.Dot, "DotChange");
				AddPlot(new Stroke(Brushes.RoyalBlue, 3), PlotStyle.Dot, "DotBullish");
				AddPlot(new Stroke(Brushes.Magenta, 3), PlotStyle.Dot, "DotBearish");

				AddLine(new Stroke(Brushes.DimGray, 1), 0, "Zeroline");

			}
			else if (State == State.Configure)
			{
				constant1 = 2.0 / (1 + MACDFastPeriod);
				constant2 = (1 - (2.0 / (1 + MACDFastPeriod)));
				constant3 = 2.0 / (1 + MACDSlowPeriod);
				constant4 = (1 - (2.0 / (1 + MACDSlowPeriod)));
				constant5 = 2.0 / (1 + MACDSmoothPeriod);
				constant6 = (1 - (2.0 / (1 + MACDSmoothPeriod)));
			}
			else if (State == State.DataLoaded)
			{
				macdEMAFast = new Series<double>(this);
				macdEMASlow = new Series<double>(this);

				macdValue = MACD(Input, MACDFastPeriod, MACDSlowPeriod, BBPeriod);
				stdDev = THStdDev(macdValue, "", BBPeriod);

				lineChangeColor = Plots[LineChangePlotIndex].Brush;
				lineBullishColor = Plots[LineBullishPlotIndex].Brush;
				lineBearishColor = Plots[LineBearishPlotIndex].Brush;

				lineChangeColor.Freeze();
				lineBullishColor.Freeze();
				lineBearishColor.Freeze();

				avgChangeColor = Plots[AvgChangePlotIndex].Brush;
				avgBullishColor = Plots[AvgBullishPlotIndex].Brush;
				avgBearishColor = Plots[AvgBearishPlotIndex].Brush;

				avgChangeColor.Freeze();
				avgBullishColor.Freeze();
				avgBearishColor.Freeze();

				dotChangeColor = Plots[DotChangePlotIndex].Brush;
				dotBullishColor = Plots[DotBullishPlotIndex].Brush;
				dotBearishColor = Plots[DotBearishPlotIndex].Brush;

				dotChangeColor.Freeze();
				dotBullishColor.Freeze();
				dotBearishColor.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < MACDSlowPeriod)
				return;

			double input0 = Input[0];
			double input1 = Input[1];

			if (CurrentBar == 0)
			{
				macdEMAFast[0] = input0;
				macdEMASlow[0] = input0;
				AvgChange[0] = 0;
				AvgBullish[0] = 0;
				AvgBearish[0] = 0;
			}
			else
			{
				double fastEma0 = constant1 * input0 + constant2 * macdEMAFast[1];
				double slowEma0 = constant3 * input0 + constant4 * macdEMASlow[1];
				double fastEma1 = constant1 * input1 + constant2 * macdEMAFast[2];
				double slowEma1 = constant3 * input1 + constant4 * macdEMASlow[2];
				double macd = fastEma0 - slowEma0;
				double previousMACD = fastEma1 - slowEma1;
				double macdAvg = constant5 * macd + constant6 * AvgChange[1];
				double previousMACDAvg = constant5 * previousMACD + constant6 * AvgChange[2];

				macdEMAFast[0] = fastEma0;
				macdEMASlow[0] = slowEma0;

				LineChange[0] = macd;
				AvgChange[0] = macdAvg;
				DotChange[0] = macd;

				double stdDev0 = stdDev[0];

				BollingerUpperBand[0] = macdAvg + (StdDevMultiplier * stdDev0);
				BollingerLowerBand[0] = macdAvg - (StdDevMultiplier * stdDev0);

				bool bullishMACDSlope = (macd >= macdAvg);

				if (bullishMACDSlope)
				{
					PlotBrushes[LineChangePlotIndex][0] = lineBullishColor;
				}
				else
				{
					PlotBrushes[LineChangePlotIndex][0] = lineBearishColor;
				}

				bool bullishAvgSlope = (macdAvg >= previousMACDAvg);

				if (bullishAvgSlope)
				{
					PlotBrushes[AvgChangePlotIndex][0] = avgBullishColor;
				}
				else
				{
					PlotBrushes[AvgChangePlotIndex][0] = avgBearishColor;
				}

				bool bullishDotSlope = (UseLineCrossDotColorFilter) ? (macd >= macdAvg) : (macd >= previousMACD);

				if (bullishDotSlope)
				{
					PlotBrushes[DotChangePlotIndex][0] = dotBullishColor;
				}
				else
				{
					PlotBrushes[DotChangePlotIndex][0] = dotBearishColor;
				}
			}
		}

		public override void OnRenderTargetChanged()
		{

			// Dispose and recreate our DX Brushes
			try
			{
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
				{
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();

					if (RenderTarget != null)
						item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);
				}
			}
			catch (Exception exception)
			{
				Log(exception.ToString(), LogLevel.Error);
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			//tmpMargin = ChartControl.Properties.BarMarginRight;
			// Call base OnRender() method to paint defined Plots.
			base.OnRender(chartControl, chartScale);

			// Store previous AA mode
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;

			// Draw Region between SenkouSpanA and SenkouSpanB
			DrawRegionBetweenSeries(chartScale, BollingerUpperBand, BollingerLowerBand, "CloudAreaColor", "CloudAreaColor", 0);

			// Reset AA mode.
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}
		

		#region SharpDX Helper Classes & Methods
		private SharpDX.Vector2 FindIntersection(SharpDX.Vector2 p1, SharpDX.Vector2 p2, SharpDX.Vector2 p3, SharpDX.Vector2 p4)
		{
			SharpDX.Vector2 intersection = new SharpDX.Vector2();

			bool segments_intersect;
			// Get the segments' parameters.
			float dx12 = p2.X - p1.X;
			float dy12 = p2.Y - p1.Y;
			float dx34 = p4.X - p3.X;
			float dy34 = p4.Y - p3.Y;

			// Solve for t1 and t2
			float denominator = (dy12 * dx34 - dx12 * dy34);

			float t1 =
				((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
					/ denominator;
			if (float.IsInfinity(t1))
				intersection = new SharpDX.Vector2(float.NaN, float.NaN);

			// Find the point of intersection.
			intersection = new SharpDX.Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);
			return intersection;
		}

		private void SetOpacity(string brushName)
		{
			if (dxmBrushes[brushName].MediaBrush == null)
				return;

			if (dxmBrushes[brushName].MediaBrush.IsFrozen)
				dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

			dxmBrushes[brushName].MediaBrush.Opacity = CloudColorOpacity / 100.0;
			dxmBrushes[brushName].MediaBrush.Freeze();
		}

		private class DXMediaMap
		{
			public SharpDX.Direct2D1.Brush DxBrush;
			public System.Windows.Media.Brush MediaBrush;
		}


		private class SharpDXFigure
		{
			public SharpDX.Vector2[] Points;
			public string Color;

			public SharpDXFigure(SharpDX.Vector2[] points, string color)
			{
				Points = points;
				Color = color;
			}
		}

		private void DrawFigure(SharpDXFigure figure)
		{
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();

			sink.BeginFigure(figure.Points[0], new SharpDX.Direct2D1.FigureBegin());

			for (int i = 0; i < figure.Points.Length; i++)
				sink.AddLine(figure.Points[i]);

			sink.AddLine(figure.Points[0]);

			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();

			RenderTarget.FillGeometry(geometry, dxmBrushes[figure.Color].DxBrush);
			geometry.Dispose();
			sink.Dispose();
		}

		private void DrawRegionBetweenSeries(ChartScale chartScale, Series<double> firstSeries, Series<double> secondSeries, string upperColor, string lowerColor, int displacement)
		{
			string BrushName = String.Empty;

			List<SharpDX.Vector2> SeriesAPoints = new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> SeriesBPoints = new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> tmpPoints = new List<SharpDX.Vector2>();
			List<SharpDXFigure> SharpDXFigures = new List<SharpDXFigure>();

			// Convert SeriesA and SeriesB to points
			int start = ChartBars.FromIndex - displacement * 2 > 0 ? ChartBars.FromIndex - displacement * 2 : 0;
			int end = ChartBars.ToIndex;

			float x0 = (float)ChartControl.GetXByBarIndex(ChartBars, 0);
			float x1 = (float)ChartControl.GetXByBarIndex(ChartBars, 1);

			if (ChartControl.Properties.EquidistantBarSpacing)
				for (int barIndex = start; barIndex <= end; barIndex++)
				{
					if (firstSeries.IsValidDataPointAt(barIndex))
					{
						SeriesAPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex + displacement), (float)chartScale.GetYByValue(firstSeries.GetValueAt(barIndex))));
						SeriesBPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex + displacement), (float)chartScale.GetYByValue(secondSeries.GetValueAt(barIndex))));
					}
				}
			else
				for (int barIndex = start; barIndex <= end; barIndex++)
				{
					if (firstSeries.IsValidDataPointAt(barIndex))
					{
						SeriesAPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex) + displacement * (x1 - x0), (float)chartScale.GetYByValue(firstSeries.GetValueAt(barIndex))));
						SeriesBPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex) + displacement * (x1 - x0), (float)chartScale.GetYByValue(secondSeries.GetValueAt(barIndex))));
					}
				}

			int lastCross = 0;
			bool isTouching = false;
			bool colorNeeded = true;

			for (int i = 0; i < SeriesAPoints.Count; i++)
			{
				if (colorNeeded)
				{
					colorNeeded = false;

					// Set initial color or wait until we need to start a shape
					if (SeriesAPoints[i].Y < SeriesBPoints[i].Y)
						BrushName = upperColor;
					else if (SeriesAPoints[i].Y > SeriesBPoints[i].Y)
						BrushName = lowerColor;
					else
					{
						colorNeeded = true;
						lastCross = i;
					}

					if (!colorNeeded)
						tmpPoints.Add(SeriesAPoints[i]);

					continue;
				}

				// Check if SeriesA and SeriesB meet or have crossed to loop back and close figure
				if ((SeriesAPoints[i].Y == SeriesBPoints[i].Y && isTouching == false)
					|| (SeriesAPoints[i].Y > SeriesBPoints[i].Y && SeriesAPoints[i - 1].Y < SeriesBPoints[i - 1].Y)
					|| (SeriesBPoints[i].Y > SeriesAPoints[i].Y && SeriesBPoints[i - 1].Y < SeriesAPoints[i - 1].Y))
				{
					// reset isTouching
					isTouching = false;

					// Set the endpoint
					SharpDX.Vector2 endpoint = (SeriesAPoints[i].Y != SeriesBPoints[i].Y) ? FindIntersection(SeriesAPoints[i - 1], SeriesAPoints[i], SeriesBPoints[i - 1], SeriesBPoints[i]) : SeriesAPoints[i];
					tmpPoints.Add(endpoint);

					// Loop back and add SeriesBPoints
					for (int j = i - 1; j >= lastCross; j--)
						tmpPoints.Add(SeriesBPoints[j]);

					// Create figure
					SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(), (SeriesAPoints[i - 1].Y < SeriesBPoints[i - 1].Y) ? upperColor : lowerColor);
					SharpDXFigures.Add(figure);

					// Clear Points
					tmpPoints.Clear();

					// Start new figure if we crossed, otherwise we will wait until we need a new figure
					if (SeriesAPoints[i].Y != SeriesBPoints[i].Y)
					{
						tmpPoints.Add(SeriesBPoints[i]);
						tmpPoints.Add(endpoint);
						tmpPoints.Add(SeriesAPoints[i]);
					}
					else
						isTouching = true;

					// Set last cross
					lastCross = i;
				}

				// Check if we are at the end of our rendering pass to loop back to loop back and close figure
				else if (i == SeriesAPoints.Count - 1)
				{
					tmpPoints.Add(SeriesAPoints[i]);

					// Loop back and add SeriesBPoints
					for (int j = i; j >= lastCross; j--)
						tmpPoints.Add(SeriesBPoints[j]);

					// Create figure
					SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(), (SeriesAPoints[i].Y < SeriesBPoints[i].Y) ? upperColor : lowerColor);
					SharpDXFigures.Add(figure);

					// Clear Points
					tmpPoints.Clear();
					break;
				}

				// Figure does not need to be closed. Add more points or open a new figure if we were touching
				else if (SeriesAPoints[i].Y != SeriesBPoints[i].Y)
				{
					if (isTouching == true)
					{
						tmpPoints.Add(SeriesAPoints[i - 1]);
						lastCross = i - 1;
					}

					tmpPoints.Add(SeriesAPoints[i]);

					isTouching = false;
				}
			}

			// Draw figures
			foreach (SharpDXFigure figure in SharpDXFigures)
				DrawFigure(figure);
		}
		#endregion

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 0)]
		public int MACDFastPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 1)]
		public int MACDSlowPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 2)]
		public int MACDSmoothPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 3)]
		public int BBPeriod
		{ get; set; }

		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 4)]
		public double StdDevMultiplier
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 5)]
		public bool UseLineCrossDotColorFilter
		{ get; set; }






		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "CloudColorOpacity", Description = "Cloud color opacity. Value 0 indicates complete transparency. Value 100 indicates complete opaqueness.", GroupName = "Parameters", Order = 6)]
		public int CloudColorOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "CloudAreaColor", Description = "Cloud area color.", GroupName = "Parameters", Order = 7)]
		public Brush CloudAreaColor
		{
			get { return dxmBrushes["CloudAreaColor"].MediaBrush; }
			set
			{
				dxmBrushes["CloudAreaColor"].MediaBrush = value;
				SetOpacity("CloudAreaColor");
			}
		}

		[Browsable(false)]
		public string CloudAreaColorUpSerializable
		{
			get { return Serialize.BrushToString(CloudAreaColor); }
			set { CloudAreaColor = Serialize.StringToBrush(value); }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BollingerUpperBand
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BollingerLowerBand
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineChange
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineBullish
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineBearish
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgChange
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgBullish
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgBearish
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DotChange
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DotBullish
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DotBearish
		{
			get { return Values[10]; }
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TickHunterTA.THCreeperSetup[] cacheTHCreeperSetup;
		public TickHunterTA.THCreeperSetup THCreeperSetup(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			return THCreeperSetup(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, bBPeriod, stdDevMultiplier, cloudColorOpacity, cloudAreaColor);
		}

		public TickHunterTA.THCreeperSetup THCreeperSetup(ISeries<double> input, string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			if (cacheTHCreeperSetup != null)
				for (int idx = 0; idx < cacheTHCreeperSetup.Length; idx++)
					if (cacheTHCreeperSetup[idx] != null && cacheTHCreeperSetup[idx].IndicatorName == indicatorName && cacheTHCreeperSetup[idx].MACDFastPeriod == mACDFastPeriod && cacheTHCreeperSetup[idx].MACDSlowPeriod == mACDSlowPeriod && cacheTHCreeperSetup[idx].MACDSmoothPeriod == mACDSmoothPeriod && cacheTHCreeperSetup[idx].BBPeriod == bBPeriod && cacheTHCreeperSetup[idx].StdDevMultiplier == stdDevMultiplier && cacheTHCreeperSetup[idx].CloudColorOpacity == cloudColorOpacity && cacheTHCreeperSetup[idx].CloudAreaColor == cloudAreaColor && cacheTHCreeperSetup[idx].EqualsInput(input))
						return cacheTHCreeperSetup[idx];
			return CacheIndicator<TickHunterTA.THCreeperSetup>(new TickHunterTA.THCreeperSetup(){ IndicatorName = indicatorName, MACDFastPeriod = mACDFastPeriod, MACDSlowPeriod = mACDSlowPeriod, MACDSmoothPeriod = mACDSmoothPeriod, BBPeriod = bBPeriod, StdDevMultiplier = stdDevMultiplier, CloudColorOpacity = cloudColorOpacity, CloudAreaColor = cloudAreaColor }, input, ref cacheTHCreeperSetup);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TickHunterTA.THCreeperSetup THCreeperSetup(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			return indicator.THCreeperSetup(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, bBPeriod, stdDevMultiplier, cloudColorOpacity, cloudAreaColor);
		}

		public Indicators.TickHunterTA.THCreeperSetup THCreeperSetup(ISeries<double> input , string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			return indicator.THCreeperSetup(input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, bBPeriod, stdDevMultiplier, cloudColorOpacity, cloudAreaColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TickHunterTA.THCreeperSetup THCreeperSetup(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			return indicator.THCreeperSetup(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, bBPeriod, stdDevMultiplier, cloudColorOpacity, cloudAreaColor);
		}

		public Indicators.TickHunterTA.THCreeperSetup THCreeperSetup(ISeries<double> input , string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int bBPeriod, double stdDevMultiplier, int cloudColorOpacity, Brush cloudAreaColor)
		{
			return indicator.THCreeperSetup(input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, bBPeriod, stdDevMultiplier, cloudColorOpacity, cloudAreaColor);
		}
	}
}

#endregion
