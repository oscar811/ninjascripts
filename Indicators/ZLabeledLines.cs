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
#endregion



//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	/// <summary>
	/// Represents an interface that exposes information regarding an Arrow ZLabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledArrowLine : ZLabeledLine
	{
		public override object Icon { get { return Gui.Tools.Icons.DrawArrowLine; } }
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				LineType									= ChartLineType.ArrowLine;
				Name										= "ZLabeled Arrow Line";
			}
		}
	}

	/// <summary>
	/// Represents an interface that exposes information regarding an Extended ZLabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledExtendedLine : ZLabeledLine
	{
		public override object Icon { get { return Gui.Tools.Icons.DrawExtendedLineTo; } }
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				LineType		= ChartLineType.ExtendedLine;
				Name			= "ZLabeled Extended Line";
				TextDisplayMode	= TextMode.PriceScale;
			}
		}
	}

	/// <summary>
	/// Represents an interface that exposes information regarding a Horizontal ZLabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledHorizontalLine : ZLabeledLine
	{
		// override this, we only need operations on a single anchor
		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor }; } }

		public override object Icon { get { return Gui.Tools.Icons.DrawHorizLineTool; } }
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				EndAnchor.IsBrowsable				= false;
				LineType							= ChartLineType.HorizontalLine;
				Name								= "ZLabeled Horizontal Line";
				StartAnchor.DisplayName				= Custom.Resource.NinjaScriptDrawingToolAnchor;
				StartAnchor.IsXPropertiesVisible	= false;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a Vertical ZLabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledVerticalLine : ZLabeledLine
	{
		// override this, we only need operations on a single anchor
		public override IEnumerable<ChartAnchor> Anchors
		{
			get { return new[] { StartAnchor }; }
		}
		
		public override object Icon { get { return Gui.Tools.Icons.DrawVertLineTool; } }

		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				EndAnchor.IsBrowsable				= false;
				LineType							= ChartLineType.VerticalLine;
				Name								= "ZLabeled Vertical Line";
				StartAnchor.DisplayName				= Custom.Resource.NinjaScriptDrawingToolAnchor;
				StartAnchor.IsYPropertyVisible		= false;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a ZLabeledRay IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledRay : ZLabeledLine
	{
		public override object Icon { get { return Gui.Tools.Icons.DrawRay; } }
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				LineType		= ChartLineType.Ray;
				Name			= "ZLabeled Ray";
				TextDisplayMode	= TextMode.PriceScale;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a ZLabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ZLabeledLine : Line
	{
		private bool appendPriceTime;
		private bool roundPrice;
		private bool needsLayoutUpdate;
		private bool offScreenDXBrushNeedsUpdate;
		private bool backgroundDXBrushNeedsUpdate;
		private bool textDXBrushNeedsUpdate;
		private string lastText;
		private string displayText;
		private Brush offScreenMediaBrush;
		private Brush backgroundMediaBrush;
		private Brush textMediaBrush;
		private SharpDX.Direct2D1.Brush offScreenDXBrush;
		private SharpDX.Direct2D1.Brush backgroundDXBrush;
		private SharpDX.Direct2D1.Brush textDXBrush;
		private SharpDX.DirectWrite.TextLayout cachedTextLayout;
		
		public enum TextMode
		{
			EndPointAtPriceScale,
			PriceScale,
			EndPoint
		}
		
		public enum RectSide
		{
			Top,
			Bottom,
			Left,
			Right,
			None
		}
		
		public override object Icon { get { return Gui.Tools.Icons.DrawLineTool; } }
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			
			if (State == State.SetDefaults)
			{
				Name						= "ZLabeled Line";
				OutlineStroke				= new Stroke(Brushes.Black, 2f);
				BackgroundBrush				= Brushes.Black;
				OffScreenBrush				= Brushes.Red;
				DisplayText 				= String.Empty;
				AppendPriceTime				= true;
				RoundPrice					= false;
				Font						= null;
				AreaOpacity 				= 75;
				TextDisplayMode				= TextMode.EndPointAtPriceScale;
				HorizontalOffset			= 0.5;
				VerticalOffset				= 3;
				offScreenDXBrushNeedsUpdate = true;
				backgroundDXBrushNeedsUpdate = true;
				textDXBrushNeedsUpdate = true;
			}
			else if (State == State.Terminated)
			{
				if (cachedTextLayout != null)
					cachedTextLayout.Dispose();
				cachedTextLayout = null;
			}
		}
		
		public override void OnRenderTargetChanged()
        {
			base.OnRenderTargetChanged();
			
			if (RenderTarget == null)
				return;
			
			if (offScreenDXBrush != null)
				offScreenDXBrush.Dispose();
			offScreenDXBrush = offScreenMediaBrush.ToDxBrush(RenderTarget);
			
			if (backgroundDXBrush != null)
				backgroundDXBrush.Dispose();
			backgroundDXBrush = backgroundMediaBrush.ToDxBrush(RenderTarget);
			backgroundDXBrush.Opacity = (float)AreaOpacity / 100f;

			if (textDXBrush != null)
				textDXBrush.Dispose();

			if (textMediaBrush != null)
				textDXBrush = textMediaBrush.ToDxBrush(RenderTarget);
		}
		
		/* Steps:
		*	1. Project start/end points for rays and extended lines
		*	2. Find collitions with ChartPanel for TextBox coordinates
		*	3. Determine price to be appended 
		*	4. Create message
		*	5. Draw TextBox
		*/

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			Stroke.RenderTarget 		= RenderTarget;
			OutlineStroke.RenderTarget	= RenderTarget;
						
			bool snap					= true;
			bool startsOnScreen			= true;
			bool priceOffScreen			= false;
			bool instrumentLoaded		= false;
			double priceToUse			= 0;
			string pricetime			= String.Empty;
			string TextToDisplay		= DisplayText;
			MasterInstrument masterInst = null;

			if (GetAttachedToChartBars().Bars != null)
			{
				masterInst = GetAttachedToChartBars().Bars.Instrument.MasterInstrument;
				instrumentLoaded = true;
			}
			else
				instrumentLoaded = false;

			Point	startPoint			= StartAnchor.GetPoint(chartControl, ChartPanel, chartScale);
			Point	endPoint			= EndAnchor.GetPoint(chartControl, ChartPanel, chartScale);
			
			double 	strokePixAdj		= ((double)(Stroke.Width % 2)).ApproxCompare(0) == 0 ? 0.5d : 0d;
			Vector	pixelAdjustVec		= new Vector(strokePixAdj, strokePixAdj);
			
			Point 	startAdj			= (LineType == ChartLineType.HorizontalLine ? new Point(ChartPanel.X, startPoint.Y) : new Point(startPoint.X, ChartPanel.Y)) + pixelAdjustVec;
			Point 	endAdj				= (LineType == ChartLineType.HorizontalLine ? new Point(ChartPanel.X + ChartPanel.W, startPoint.Y) : new Point(startPoint.X, ChartPanel.Y + ChartPanel.H)) + pixelAdjustVec;
			
			Vector 	distVec 			= Vector.Divide(Point.Subtract(endPoint, startPoint), 100);
			Vector 	scalVec				= (LineType == ChartLineType.ExtendedLine || LineType == ChartLineType.Ray || LineType == ChartLineType.HorizontalLine) ? Vector.Multiply(distVec, 10000) : Vector.Multiply(distVec, 100);
			Point 	extPoint			= Vector.Add(scalVec, startPoint);
				
			// Project extended line start point if it is off screen
			if (LineType == ChartLineType.ExtendedLine && TextDisplayMode != TextMode.EndPoint)
				startPoint 				= Point.Subtract(startPoint, scalVec);
			
			// Project TextBox coordinate for extended lines and rays to get ChartPanel bounds
			if (LineType == ChartLineType.ExtendedLine || LineType == ChartLineType.Ray)
				extPoint = Vector.Add(scalVec, extPoint);
			
			// Find collisions with ChartPanel bounds for PriceScale bound TextBox coordinates
			if (LineType == ChartLineType.HorizontalLine || LineType == ChartLineType.VerticalLine)
			{
				extPoint = endAdj;
				startPoint = startAdj;
			}
			else if (TextDisplayMode == TextMode.EndPoint)
			{
				extPoint = endPoint;
				snap 	 = false;
			}
			else
			{
				if (extPoint.X <= ChartPanel.X || extPoint.Y < ChartPanel.Y || extPoint.X > ChartPanel.X + ChartPanel.W || extPoint.Y > ChartPanel.Y + ChartPanel.H)
				{
					switch (LineIntersectsRect(startPoint, extPoint, new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H)))
					{
						case RectSide.Top:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y));
							break;
						case RectSide.Bottom:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H));
							break;
						case RectSide.Left:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y));
							break;
						case RectSide.Right:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H));
							break;
						default:
							return;
					}
				}
				
				if (startPoint.X <= ChartPanel.X || startPoint.Y < ChartPanel.Y || startPoint.X > ChartPanel.X + ChartPanel.W || startPoint.Y > ChartPanel.Y + ChartPanel.H)
				{
					switch (LineIntersectsRect(extPoint, startPoint, new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H)))
					{
						case RectSide.Top:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y));
							break;
						case RectSide.Bottom:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H));
							break;
						case RectSide.Left:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y));
							break;
						case RectSide.Right:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H));
							break;
						default:
							return;
					}
				}
				
				if (endPoint.X <= ChartPanel.X || endPoint.Y < ChartPanel.Y || endPoint.X > ChartPanel.X + ChartPanel.W || endPoint.Y > ChartPanel.Y + ChartPanel.H)
					priceOffScreen = true;
				
				if (endPoint.X == startPoint.X && startPoint.Y < endPoint.Y && priceOffScreen)
					extPoint.Y = ChartPanel.Y + ChartPanel.H;
			}
			
			// Scale coordinates by HorizontalOffset/VerticalOffset
			distVec 	= Point.Subtract(extPoint, startPoint);
			scalVec 	= Vector.Multiply(Vector.Divide(distVec, 100), HorizontalOffset);
			extPoint	= Point.Subtract(extPoint, scalVec);
			extPoint.Y 	-= VerticalOffset;

			// Get a Price or a Timestamp to append to the label
			switch (LineType)
			{
				case ChartLineType.VerticalLine:
					pricetime = StartAnchor.Time.ToString();
					break;
				case ChartLineType.HorizontalLine:
					priceToUse = StartAnchor.Price;
					break;
				case ChartLineType.ExtendedLine:
				case ChartLineType.Ray:
					priceToUse = TextDisplayMode == TextMode.PriceScale
							   ? chartScale.GetValueByY(endPoint.X >= startPoint.X
														? (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.Y + ChartPanel.H)).Y
						 								: (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H)).Y)
							   : EndAnchor.Price;
					break;
				default:
					priceToUse = priceOffScreen && TextDisplayMode == TextMode.PriceScale
							   ? chartScale.GetValueByY(endPoint.X >= startPoint.X
														? (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.Y + ChartPanel.H)).Y
						 								: (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X, ChartPanel.Y + ChartPanel.H)).Y)
							   : EndAnchor.Price;
					break;
			}
			
			// Round the price
			if (LineType != ChartLineType.VerticalLine)
			{
				string format = masterInst != null && (masterInst.InstrumentType == InstrumentType.Forex || masterInst.TickSize < 0.01) ? "0.0000000" : "0.00";

				if (IsGlobalDrawingTool)
					pricetime = "Append Price/Time is not compatible with Global Drawing Objects";
				else if (!instrumentLoaded)
					pricetime = "Instrument Loading...";
				else if (AttachedTo.AttachedToType == AttachedToType.Bars || RoundPrice)
					pricetime = priceToUse <= masterInst.RoundDownToTickSize(priceToUse) + masterInst.TickSize * 0.5
								? pricetime = masterInst.RoundDownToTickSize(priceToUse).ToString(format)
								: pricetime = masterInst.RoundToTickSize(priceToUse).ToString(format);
				else
					pricetime = priceToUse.ToString(format);
			}
			
			// Check if we need to append price or time
			if (AppendPriceTime && DisplayText.Length > 0)
				TextToDisplay = String.Format("{0} {1}", DisplayText, pricetime);
			else if (AppendPriceTime)
				TextToDisplay = pricetime;
			
			// Use Label Font if one is not specified by template
			if(Font == null)
				Font = new NinjaTrader.Gui.Tools.SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), 16);
			
			// Update DX Brushes
			if (offScreenDXBrushNeedsUpdate)
			{
				if (offScreenDXBrush != null)
					offScreenDXBrush.Dispose();
				offScreenDXBrush = offScreenMediaBrush.ToDxBrush(RenderTarget);
				offScreenDXBrushNeedsUpdate = false;
			}
			
			if (backgroundDXBrushNeedsUpdate)
			{
				if (backgroundDXBrush != null)
					backgroundDXBrush.Dispose();
				backgroundDXBrush = backgroundMediaBrush.ToDxBrush(RenderTarget);
				backgroundDXBrush.Opacity = (float)AreaOpacity / 100f;
				backgroundDXBrushNeedsUpdate = false;
			}

			
			if (textDXBrushNeedsUpdate)
			{
				if (textDXBrush != null)
					textDXBrush.Dispose();
				
				if (textMediaBrush != null)
					textDXBrush = textMediaBrush.ToDxBrush(RenderTarget);

				textDXBrushNeedsUpdate = false;
			}


			SharpDX.Direct2D1.Brush tempTextDXBrush = (textDXBrush != null) ? textDXBrush : Stroke.BrushDX;

			// Draw TextBoxes
			switch (LineType)
			{
				case ChartLineType.VerticalLine:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, tempTextDXBrush, backgroundDXBrush, OutlineStroke, 1.5708f);
					break;
				case ChartLineType.HorizontalLine:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, tempTextDXBrush, backgroundDXBrush, OutlineStroke, 0);
					break;
				default:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, priceOffScreen && TextDisplayMode == TextMode.EndPointAtPriceScale ? offScreenDXBrush : tempTextDXBrush, backgroundDXBrush, OutlineStroke, 0);
					break;					
			}
		}
		
		private void DrawTextBox(bool Snap, string displayText, double x, double y, SharpDX.Direct2D1.Brush brush, SharpDX.Direct2D1.Brush bgBrush, Stroke stroke, float rotate)
		{
			const int padding = 4;
			
			// Text has changed, need to update cached TextLayout
			if (displayText != lastText)
				needsLayoutUpdate = true;
			lastText = displayText;
			
			// Update cachedTextLayout
			if (needsLayoutUpdate || cachedTextLayout == null)
			{
				SharpDX.DirectWrite.TextFormat textFormat = Font.ToDirectWriteTextFormat();
				cachedTextLayout = 	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
									displayText, textFormat, ChartPanel.X + ChartPanel.W,
									textFormat.FontSize);
				textFormat.Dispose();
				needsLayoutUpdate = false;
			}
			
			// Snap TextBox coordinates to ChartPanel when out of bounds
			if (Snap)
			{
				if (rotate == 1.5708f)
					y = Math.Max(ChartPanel.Y + cachedTextLayout.Metrics.Width + 2 * padding, y);
				else
				{
					y = Math.Min(ChartPanel.H + ChartPanel.Y - padding, y);
					y = Math.Max(ChartPanel.Y + cachedTextLayout.Metrics.Height + padding, y);
					x = Math.Max(ChartPanel.X + cachedTextLayout.Metrics.Width + 2 * padding, x);
				}
			}
			
			// Apply rotation
			RenderTarget.Transform = SharpDX.Matrix3x2.Rotation(rotate, new SharpDX.Vector2((float)x, (float)y));
			
			// Add padding to TextPlotPoint
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(x - cachedTextLayout.Metrics.Width - padding / 2, y - cachedTextLayout.Metrics.Height).ToVector2();
			
			// Draw the TextBox
			if (displayText.Length > 0)
			{
	            SharpDX.RectangleF 					PLBoundRect		= new SharpDX.RectangleF((float)x - cachedTextLayout.Metrics.Width - padding, (float)y - cachedTextLayout.Metrics.Height - padding / 2, cachedTextLayout.Metrics.Width + padding, cachedTextLayout.Metrics.Height + padding);
				SharpDX.Direct2D1.RoundedRectangle 	PLRoundedRect 	= new SharpDX.Direct2D1.RoundedRectangle() { Rect = PLBoundRect, RadiusX = cachedTextLayout.FontSize/4, RadiusY = cachedTextLayout.FontSize/4 };
				RenderTarget.FillRoundedRectangle(PLRoundedRect, bgBrush);
				RenderTarget.DrawRoundedRectangle(PLRoundedRect, stroke.BrushDX, stroke.Width, stroke.StrokeStyle);
				
				// Draw the TextLayout
				RenderTarget.DrawTextLayout(TextPlotPoint, cachedTextLayout, brush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}
			
			// Restore rotation
			RenderTarget.Transform = SharpDX.Matrix3x2.Identity;
		}
		
		private Point FindIntersection(Point p1, Point p2, Point p3, Point p4)
		{
			Point intersection = new Point();
			
		    // Get the segments' parameters.
		    double dx12 = p2.X - p1.X;
		    double dy12 = p2.Y - p1.Y;
		    double dx34 = p4.X - p3.X;
		    double dy34 = p4.Y - p3.Y;

		    // Solve for t1 and t2
		    double denominator = (dy12 * dx34 - dx12 * dy34);

		    double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) 
						/ denominator;
		    
			if (double.IsInfinity(t1))
		        intersection = new Point(double.NaN, double.NaN);

		    // Find the point of intersection.
		    intersection = new Point(Math.Max(p1.X + dx12 * t1, 0), p1.Y + dy12 * t1);
			return intersection;
		}
		
		private RectSide LineIntersectsRect(Point p1, Point p2, SharpDX.RectangleF r)
	    {

	        if (LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) && p1.Y > r.Y)
				return RectSide.Top;
			if (LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) && p1.X < r.X + r.Width)
				return RectSide.Right;
			if (LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) && p1.Y < r.Y + r.Height)
				return RectSide.Bottom;
			if (LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)))
				return RectSide.Left;

			return RectSide.None;
		}

	    private bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
	    {
	        double q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
	        double d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

	        if( d == 0 )
	            return false;

	        double r = q / d;

	        q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
	        double s = q / d;

	        if( r < 0 || r > 1 || s < 0 || s > 1 )
	            return false;

	        return true;
	    }
		
		[Display(Name = "Text Horizontal Offset", Description = "Distance to offset from End Point", GroupName = "General", Order = 5)]
		[Range(0, 100)]
		public double HorizontalOffset
		{ get; set; }
		
		[Display(Name = "Text Vertical Offset", Description = "Distance from line", GroupName = "General", Order = 6)]
		[Range(-100, 100)]
		public double VerticalOffset
		{ get; set; }
		
		[ExcludeFromTemplate]
		[Display(Name = "Text", GroupName = "General", Order = 7)]
		[PropertyEditor("NinjaTrader.Gui.Tools.MultilineEditor")]
		public string DisplayText
		{
			get { return displayText; }
			set
			{
				if (displayText == value)
					return;
				displayText			= value;
				needsLayoutUpdate 	= true;
			}
		}
		
		[Display(Name = "Append Price/Time", GroupName = "General", Order = 8)]
		public bool AppendPriceTime
		{
			get { return appendPriceTime; }
			set
			{
				if (appendPriceTime == value)
					return;
				appendPriceTime			= value;
				needsLayoutUpdate		= true;
			}
		}
		
		[Display(Name = "Round Price to Tick Size", GroupName = "General", Order = 9)]
		public bool RoundPrice
		{
			get { return roundPrice; }
			set
			{
				if (roundPrice == value)
					return;
				roundPrice			= value;
				needsLayoutUpdate		= true;
			}
		}
		
		[Display(Name = "Text Display Mode", GroupName = "General", Order = 10)]
		public TextMode TextDisplayMode
		{ get; set; }
		
		[Display(Name = "Font", GroupName = "General", Order = 11)]
		public Gui.Tools.SimpleFont Font
		{ get; set; }
		
		[XmlIgnore]
		[Display(GroupName = "General", Name = "Price Offscreen Text Color", Order = 12)]
		public Brush OffScreenBrush 
		{ 
			get { return offScreenMediaBrush; } 
			set
			{
				offScreenMediaBrush = value;
				offScreenDXBrushNeedsUpdate = true;
			}
		}
		
		[Browsable(false)]
		public string OffScreenBrushSerializable
		{
			get { return Serialize.BrushToString(OffScreenBrush); }
			set { OffScreenBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(GroupName = "General", Name = "Text Box Outline", Order = 100)]
		public Stroke OutlineStroke { get; set; }
		
		[XmlIgnore]
		[Display(GroupName = "General", Name = "Text Box Background Color", Order = 101)]
		public Brush BackgroundBrush 
		{ 
			get { return backgroundMediaBrush; } 
			set
			{
				backgroundMediaBrush = value;
				backgroundDXBrushNeedsUpdate = true;
			}
		}

		[Browsable(false)]
		public string BackgroundBrushSerializable
		{
			get { return Serialize.BrushToString(BackgroundBrush); }
			set { BackgroundBrush = Serialize.StringToBrush(value); }
		}

		[Display(GroupName = "General", Name = "Text Box Background Opacity", Order = 102)]
		public int AreaOpacity { get; set; }

		[XmlIgnore]
		[Display(GroupName = "General", Name = "Text Color", Order = 101)]
		public Brush TextBrush
		{
			get { return textMediaBrush; }
			set
			{
				textMediaBrush = value;
				textDXBrushNeedsUpdate = true;
			}
		}

		[Browsable(false)]
		public string TextBrushSerializable
		{
			get { return Serialize.BrushToString(TextBrush); }
			set { TextBrush = Serialize.StringToBrush(value); }
		}

		

	}
	
	#region NinjaScript Overloads
	public static partial class DrawZLabledLine
	{
		private static T DrawZLabeledLineTypeCore<T>(NinjaScriptBase owner, bool isAutoScale, string tag,
										int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
										Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName) where T : ZLabeledLine
		{
			if (owner == null)
				throw new ArgumentException("owner");

			if (string.IsNullOrWhiteSpace(tag))
				throw new ArgumentException(@"tag cant be null or empty", "tag");

			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = string.Format("{0}{1}", GlobalDrawingToolManager.GlobalDrawingToolTagPrefix, tag);

			T lineT = DrawingTool.GetByTagOrNew(owner, typeof(T), tag, templateName) as T;

			if (lineT == null)
				return null;

			if (lineT is ZLabeledVerticalLine)
			{
				if (startTime == Core.Globals.MinDate && startBarsAgo == int.MinValue)
					throw new ArgumentException("missing vertical line time / bars ago");
			}
			else if (lineT is ZLabeledHorizontalLine)
			{
				if (startY.ApproxCompare(double.MinValue) == 0)
					throw new ArgumentException("missing horizontal line Y");
			}
			else if (startTime == Core.Globals.MinDate && endTime == Core.Globals.MinDate && startBarsAgo == int.MinValue && endBarsAgo == int.MinValue)
				throw new ArgumentException("bad start/end date/time");

			DrawingTool.SetDrawingToolCommonValues(lineT, tag, isAutoScale, owner, isGlobal);

			// dont nuke existing anchor refs on the instance
			ChartAnchor startAnchor;

			// check if its one of the single anchor lines
			if (lineT is ZLabeledHorizontalLine || lineT is ZLabeledVerticalLine)
			{
				startAnchor = DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
				startAnchor.CopyDataValues(lineT.StartAnchor);
			}
			else
			{
				startAnchor				= DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, startY);
				ChartAnchor endAnchor	= DrawingTool.CreateChartAnchor(owner, endBarsAgo, endTime, endY);
				startAnchor.CopyDataValues(lineT.StartAnchor);
				endAnchor.CopyDataValues(lineT.EndAnchor);
			}

			if (brush != null)
				lineT.Stroke = new Stroke(brush, dashStyle, width) { RenderTarget = lineT.Stroke.RenderTarget };

			lineT.SetState(State.Active);
			return lineT;
		}

		// arrow line overloads
		private static ZLabeledArrowLine ZLabeledArrowLineCore(NinjaScriptBase owner, bool isAutoScale, string tag,
											int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
											Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledArrowLine>(owner, isAutoScale,tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY,
				brush, dashStyle, width, isGlobal, templateName);
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return ZLabeledArrowLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush,
				DashStyleHelper.Solid, 1, false, null);
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush)
		{
			return ZLabeledArrowLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush,
				DashStyleHelper.Solid, 1, false, null);
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledArrowLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledArrowLineCore(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width, false, null));
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledArrowLineCore(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width, false, null));
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, bool isGlobal, string templateName)
		{
			return ZLabeledArrowLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, null,
				DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws an arrow line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledArrowLine ZLabeledArrowLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY,  bool isGlobal, string templateName)
		{
			return ZLabeledArrowLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, null,
				DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		// extended line overloads
		private static ZLabeledExtendedLine ZLabeledExtendedLineCore(NinjaScriptBase owner, bool isAutoScale, string tag,
												int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
												Brush brush, DashStyleHelper dashStyle, int width, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledExtendedLine>(owner, isAutoScale,tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY,
				brush, dashStyle, width, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush,
				DashStyleHelper.Solid, 1, false, null);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush,
				DashStyleHelper.Solid, 1, false, null);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
								brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY,
			Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledExtendedLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
								brush, dashStyle, width, false, null));
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledExtendedLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width, false, null));
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, bool isGlobal, string templateName)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, null,
				DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, bool isGlobal, string templateName)
		{
			return ZLabeledExtendedLineCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, null,
				DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime, double endY,
			Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledExtendedLineCore(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width, false, null);
		}


		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledExtendedLineCore(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
								brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledExtendedLineCore(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
								brush, dashStyle, width, false, null));
		}

		/// <summary>
		/// Draws a line with infinite end points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledExtendedLine ZLabeledExtendedLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime, double endY,
			Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledExtendedLineCore(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width, false, null));
		}


		// horizontal line overloads
		private static ZLabeledHorizontalLine ZLabeledHorizontalLineCore(NinjaScriptBase owner, bool isAutoScale, string tag,
												double y, Brush brush, DashStyleHelper dashStyle, int width)
		{
			return DrawZLabeledLineTypeCore<ZLabeledHorizontalLine>(owner, isAutoScale, tag, 0, Core.Globals.MinDate, y, 0, Core.Globals.MinDate,
											y, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, double y, Brush brush)
		{
			return ZLabeledHorizontalLineCore(owner, false, tag, y, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, double y, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return ZLabeledHorizontalLineCore(owner, false, tag, y, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, double y, Brush brush, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledHorizontalLineCore(owner, false, tag, y, brush, DashStyleHelper.Solid, 1));
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, double y, Brush brush,
													DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledHorizontalLineCore(owner, false, tag, y, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, double y, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledHorizontalLine>(owner, false, tag, int.MinValue, Core.Globals.MinDate, y, int.MinValue, Core.Globals.MinDate,
											y, null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, bool isAutoScale, double y, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return ZLabeledHorizontalLineCore(owner, isAutoScale, tag, y, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a horizontal line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoscale">if set to <c>true</c> [is autoscale].</param>
		/// <param name="y">The y value or Price for the object</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledHorizontalLine ZLabeledHorizontalLine(NinjaScriptBase owner, string tag, bool isAutoscale, double y, Brush brush, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledHorizontalLineCore(owner, isAutoscale, tag, y, brush, DashStyleHelper.Solid, 1));
		}

		// line overloads
		private static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, bool isAutoScale, string tag,
								int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
								Brush brush, DashStyleHelper dashStyle, int width)
		{
			return DrawZLabeledLineTypeCore<ZLabeledLine>(owner, isAutoScale, tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return ZLabeledLine(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo,
			double endY, Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledLine(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime,
			double endY, Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledLine(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo,
			double endY, Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledLine(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime,
			double endY, Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledLine(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime,
			double endY, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledLine>(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY,
				null, DashStyleHelper.Dash, 0, false, templateName);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo,
			double endY, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledLine>(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
				null, DashStyleHelper.Dash, 0, false, templateName);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo,
			double endY, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledLine>(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
				null, DashStyleHelper.Solid, 0, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledLine ZLabeledLine(NinjaScriptBase owner, string tag, bool isAutoScale, DateTime startTime, double startY, DateTime endTime,
			double endY, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledLine>(owner, isAutoScale, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY,
				null, DashStyleHelper.Solid, 0, isGlobal, templateName);
		}

		// vertical line overloads
		private static ZLabeledVerticalLine ZLabeledVerticalLineCore(NinjaScriptBase owner, bool isAutoScale, string tag,
												int barsAgo, DateTime time, Brush brush, DashStyleHelper dashStyle, int width)
		{
			return DrawZLabeledLineTypeCore<ZLabeledVerticalLine>(owner, isAutoScale, tag, barsAgo, time, double.MinValue, int.MinValue, Core.Globals.MinDate,
											double.MinValue, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="time"> The time the object will be drawn at.</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, DateTime time, Brush brush)
		{
			return ZLabeledVerticalLineCore(owner, false, tag, int.MinValue, time, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="time"> The time the object will be drawn at.</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, DateTime time, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return ZLabeledVerticalLineCore(owner, false, tag, int.MinValue, time, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="barsAgo">The bar the object will be drawn at. A value of 10 would be 10 bars ago</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, int barsAgo, Brush brush)
		{
			return ZLabeledVerticalLineCore(owner, false, tag, barsAgo, Core.Globals.MinDate, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="barsAgo">The bar the object will be drawn at. A value of 10 would be 10 bars ago</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, int barsAgo, Brush brush,
													DashStyleHelper dashStyle, int width)
		{
			return ZLabeledVerticalLineCore(owner, false, tag, barsAgo, Core.Globals.MinDate, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="time"> The time the object will be drawn at.</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, DateTime time, Brush brush,
													DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				 ZLabeledVerticalLineCore(owner, false, tag, int.MinValue, time, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="barsAgo">The bar the object will be drawn at. A value of 10 would be 10 bars ago</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, int barsAgo, Brush brush,
													DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledVerticalLineCore(owner, false, tag, barsAgo, Core.Globals.MinDate, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="barsAgo">The bar the object will be drawn at. A value of 10 would be 10 bars ago</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, int barsAgo, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledVerticalLine>(owner, false, tag, barsAgo, Core.Globals.MinDate,
				double.MinValue, int.MinValue, Core.Globals.MinDate, double.MinValue, null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a vertical line.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="time"> The time the object will be drawn at.</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledVerticalLine ZLabeledVerticalLine(NinjaScriptBase owner, string tag, DateTime time, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledVerticalLine>(owner, false, tag, int.MinValue, time,
				double.MinValue, int.MinValue, Core.Globals.MinDate, double.MinValue, null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		// ray overloads
		private static ZLabeledRay ZLabeledRayCore(NinjaScriptBase owner, bool isAutoScale, string tag,
								int startBarsAgo, DateTime startTime, double startY, int endBarsAgo, DateTime endTime, double endY,
								Brush brush, DashStyleHelper dashStyle, int width)
		{
			return DrawZLabeledLineTypeCore<ZLabeledRay>(owner, isAutoScale, tag, startBarsAgo, startTime, startY, endBarsAgo, endTime, endY, brush, dashStyle, width, false, null);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag,int startBarsAgo, double startY, int endBarsAgo, double endY, Brush brush)
		{
			return ZLabeledRayCore(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo, double endY,
								Brush brush, DashStyleHelper dashStyle, int width)
		{
			return ZLabeledRayCore(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush)
		{
			return ZLabeledRayCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, DashStyleHelper.Solid, 1);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush,
								DashStyleHelper dashStyle, int width)
		{
			return ZLabeledRayCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="isAutoScale">Determines if the draw object will be included in the y-axis scale</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, bool isAutoScale, int startBarsAgo, double startY, int endBarsAgo, double endY,
								Brush brush, DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledRayCore(owner, isAutoScale, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="dashStyle">The dash style used for the lines of the object.</param>
		/// <param name="width">The width of the draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, Brush brush,
								DashStyleHelper dashStyle, int width, bool drawOnPricePanel)
		{
			return DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				ZLabeledRayCore(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, brush, dashStyle, width));
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, int startBarsAgo, double startY, int endBarsAgo, double endY, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledRay>(owner, false, tag, startBarsAgo, Core.Globals.MinDate, startY, endBarsAgo, Core.Globals.MinDate, endY,
				null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}

		/// <summary>
		/// Draws a line which has an infinite end point in one direction.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="startY">The starting y value coordinate where the draw object will be drawn</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="endY">The end y value coordinate where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static ZLabeledRay ZLabeledRay(NinjaScriptBase owner, string tag, DateTime startTime, double startY, DateTime endTime, double endY, bool isGlobal, string templateName)
		{
			return DrawZLabeledLineTypeCore<ZLabeledRay>(owner, false, tag, int.MinValue, startTime, startY, int.MinValue, endTime, endY, null, DashStyleHelper.Solid, 1, isGlobal, templateName);
		}
	}
	#endregion
}
