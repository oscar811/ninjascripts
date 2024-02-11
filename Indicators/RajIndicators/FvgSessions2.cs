#region Assembly LuxAlgoFVGSessions, Version=1.0.0.1, Culture=neutral, PublicKeyToken=null
// C:\Users\sshrestha\Documents\NinjaTrader 8\bin\Custom\LuxAlgo - FVGSessions.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.LuxAlgo;

namespace NinjaTrader.NinjaScript.Indicators.RajIndicators
{
    public class FVGSessions2 : Indicator
    {
        private class FVG
        {
            public double Top;

            public double Bottom;

            public bool Mitigated;

            public bool IsNew;

            public bool IsBull;

            public NinjaTrader.NinjaScript.DrawingTools.Line Lvl;

            public Rectangle Area;

            public FVG(double top, double bottom, bool mitigated, bool isNew, bool isBull)
            {
                Top = top;
                Bottom = bottom;
                Mitigated = mitigated;
                IsNew = isNew;
                IsBull = isBull;
            }

            public void SetFVG(int offset, Brush bgCss, Brush lCss, int cb)
            {
                double num = (Top + Bottom) / 2.0;
                Area = Pine.Box.New(cb - offset, Top, cb, Bottom, null, 1, DashStyleHelper.Solid, bgCss, (offset == 2) ? 60 : 30);
                Lvl = Pine.Line.New(cb - offset, num, cb, num, lCss, DashStyleHelper.Dash);
            }
        }

        private class SessionRange
        {
            public NinjaTrader.NinjaScript.DrawingTools.Line Max;

            public NinjaTrader.NinjaScript.DrawingTools.Line Min;

            public SessionRange(NinjaTrader.NinjaScript.DrawingTools.Line _Max, NinjaTrader.NinjaScript.DrawingTools.Line _Min)
            {
                Max = _Max;
                Min = _Min;
            }

            public void SetRange(double high, double low, int cb)
            {
                double y = Math.Max(high, Pine.Line.GetY2(ref Max));
                double y2 = Math.Min(low, Pine.Line.GetY2(ref Min));
                Pine.Line.SetXY2(ref Max, cb, y);
                Pine.Line.SetY1(ref Max, y);
                Pine.Line.SetXY2(ref Min, cb, y2);
                Pine.Line.SetY1(ref Min, y2);
            }
        }

        private FVG sfvg;

        private SessionRange sesr;

        protected static PineLib Pine;

        public override string DisplayName => "LuxAlgo - FVG Sessions";

        [NinjaScriptProperty]
        [Display(Name = "Bullish Color", Description = "BullCss", Order = 1, GroupName = "Parameters")]
        [XmlIgnore]
        public Brush BullCss { get; set; }

        [Browsable(false)]
        public string BullCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BullCss);
            }
            set
            {
                BullCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Area Color", Description = "BullAreaCss", Order = 2, GroupName = "Parameters")]
        public Brush BullAreaCss { get; set; }

        [Browsable(false)]
        public string BullAreaCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BullAreaCss);
            }
            set
            {
                BullAreaCss = Serialize.StringToBrush(value);
            }
        }

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Bullish Mitigated Color", Description = "BullMitigatedCss", Order = 3, GroupName = "Parameters")]
        public Brush BullMitigatedCss { get; set; }

        [Browsable(false)]
        public string BullMitigatedCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BullMitigatedCss);
            }
            set
            {
                BullMitigatedCss = Serialize.StringToBrush(value);
            }
        }

        [XmlIgnore]
        [Display(Name = "Bearish Color", Description = "BearCss", Order = 4, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public Brush BearCss { get; set; }

        [Browsable(false)]
        public string BearCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BearCss);
            }
            set
            {
                BearCss = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Area Color", Description = "BearAreaCss", Order = 5, GroupName = "Parameters")]
        public Brush BearAreaCss { get; set; }

        [Browsable(false)]
        public string BearAreaCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BearAreaCss);
            }
            set
            {
                BearAreaCss = Serialize.StringToBrush(value);
            }
        }

        [XmlIgnore]
        [Display(Name = "Bearish Mitigated Color", Description = "BearMitigatedCss", Order = 6, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public Brush BearMitigatedCss { get; set; }

        [Browsable(false)]
        public string BearMitigatedCssSerializable
        {
            get
            {
                return Serialize.BrushToString(BearMitigatedCss);
            }
            set
            {
                BearMitigatedCss = Serialize.StringToBrush(value);
            }
        }

        protected override void OnStateChange()
        {
            if (base.State == State.SetDefaults)
            {
                base.Description = "The FVG Sessions indicator highlights the first fair value gap of the trading session as well as the session range. Detected fair value gaps extend to the end of the trading session.";
                base.Name = "FVG Sessions";
                base.Calculate = Calculate.OnBarClose;
                base.IsOverlay = true;
                base.DisplayInDataBox = true;
                base.DrawOnPricePanel = true;
                base.DrawHorizontalGridLines = true;
                base.DrawVerticalGridLines = true;
                base.PaintPriceMarkers = true;
                base.ScaleJustification = ScaleJustification.Right;
                base.IsSuspendedWhileInactive = true;
                BullCss = Brushes.DarkCyan;
                BullAreaCss = Brushes.DarkCyan;
                BullMitigatedCss = Brushes.DarkCyan;
                BearCss = Brushes.Crimson;
                BearAreaCss = Brushes.Crimson;
                BearMitigatedCss = Brushes.Crimson;
            }
            else if (base.State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Day, 1);
            }
            else if (base.State == State.DataLoaded)
            {
                Pine = new PineLib(this, this, base.DrawObjects);
                sfvg = new FVG(0.0, 0.0, mitigated: false, isNew: true, isBull: false);
                sesr = null;
            }
        }

        protected override void OnBarUpdate()
        {
            if (base.BarsInProgress != 0 || base.CurrentBar < 2)
            {
                return;
            }

            bool flag = base.Low[0] > base.High[2] && base.Close[1] > base.High[2];
            bool flag2 = base.High[0] < base.Low[2] && base.Close[1] < base.Low[2];
            if (Pine.IsTimeframeChanged(1))
            {
                Draw.Ray(this, "FVGSession Day Divider " + base.CurrentBar, isAutoScale: false, 0, 0.0, 0, base.Low[0], Brushes.Gray, DashStyleHelper.Dash, 1);
                sesr = new SessionRange(Pine.Line.New(base.CurrentBar, base.High[0], base.CurrentBar, base.High[0], Brushes.Gray), Pine.Line.New(base.CurrentBar, base.Low[0], base.CurrentBar, base.Low[0], Brushes.Gray));
                sfvg.IsNew = true;
                if (sfvg.Lvl != null)
                {
                    Pine.Line.SetX2(ref sfvg.Lvl, base.CurrentBar - 2);
                    Pine.Box.SetRight(ref sfvg.Area, base.CurrentBar - 2);
                }
            }
            else if (sesr != null)
            {
                sesr.SetRange(base.High[0], base.Low[0], base.CurrentBar);
                Pine.Line.SetColor(ref sesr.Max, sfvg.IsBull ? BullCss : BearCss);
                Pine.Line.SetColor(ref sesr.Min, sfvg.IsBull ? BullCss : BearCss);
            }

            if (flag && sfvg.IsNew)
            {
                sfvg = new FVG(base.Low[0], base.High[2], mitigated: false, isNew: false, isBull: true);
                sfvg.SetFVG(2, BullAreaCss, BullCss, base.CurrentBar);
            }
            else if (flag2 && sfvg.IsNew)
            {
                sfvg = new FVG(base.Low[2], base.High[0], mitigated: false, isNew: false, isBull: false);
                sfvg.SetFVG(2, BearAreaCss, BearCss, base.CurrentBar);
            }

            if (!sfvg.Mitigated)
            {
                if (sfvg.IsBull && base.Close[0] < sfvg.Bottom)
                {
                    sfvg.SetFVG(1, BullMitigatedCss, BullCss, base.CurrentBar);
                    sfvg.Mitigated = true;
                }
                else if (!sfvg.IsBull && base.Close[0] > sfvg.Top)
                {
                    sfvg.SetFVG(1, BearMitigatedCss, BearCss, base.CurrentBar);
                    sfvg.Mitigated = true;
                }
            }

            if (!sfvg.IsNew)
            {
                Pine.Line.SetX2(ref sfvg.Lvl, base.CurrentBar);
                Pine.Box.SetRight(ref sfvg.Area, base.CurrentBar);
            }
        }
    }
}

#if false // Decompilation log
'36' items in cache
------------------
Resolve: 'NinjaTrader.Core, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Core, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\NinjaTrader 8\bin\NinjaTrader.Core.dll'
------------------
Resolve: 'NinjaTrader.Gui, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Gui, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Program Files\NinjaTrader 8\bin\NinjaTrader.Gui.dll'
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll'
------------------
Resolve: 'NinjaTrader.Vendor, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'NinjaTrader.Vendor, Version=8.1.1.7, Culture=neutral, PublicKeyToken=null'
Load from: 'C:\Users\sshrestha\Documents\NinjaTrader 8\bin\Custom\NinjaTrader.Vendor.dll'
------------------
Resolve: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationCore.dll'
------------------
Resolve: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\WPF\WindowsBase.dll'
------------------
Resolve: 'SharpDX.Direct2D1, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Found single assembly: 'SharpDX.Direct2D1, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\SharpDX.Direct2D1.dll'
------------------
Resolve: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\WPF\PresentationFramework.dll'
------------------
Resolve: 'SharpDX, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Found single assembly: 'SharpDX, Version=2.6.3.0, Culture=neutral, PublicKeyToken=b4dcf0f35e5521f1'
Load from: 'C:\Program Files\NinjaTrader 8\bin\SharpDX.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll'
------------------
Resolve: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.ComponentModel.DataAnnotations.dll'
------------------
Resolve: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll'
------------------
Resolve: 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Windows.Forms.dll'
------------------
Resolve: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xaml.dll'
#endif

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RajIndicators.FVGSessions2[] cacheFVGSessions2;
		public RajIndicators.FVGSessions2 FVGSessions2(Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			return FVGSessions2(Input, bullCss, bullAreaCss, bullMitigatedCss, bearCss, bearAreaCss, bearMitigatedCss);
		}

		public RajIndicators.FVGSessions2 FVGSessions2(ISeries<double> input, Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			if (cacheFVGSessions2 != null)
				for (int idx = 0; idx < cacheFVGSessions2.Length; idx++)
					if (cacheFVGSessions2[idx] != null && cacheFVGSessions2[idx].BullCss == bullCss && cacheFVGSessions2[idx].BullAreaCss == bullAreaCss && cacheFVGSessions2[idx].BullMitigatedCss == bullMitigatedCss && cacheFVGSessions2[idx].BearCss == bearCss && cacheFVGSessions2[idx].BearAreaCss == bearAreaCss && cacheFVGSessions2[idx].BearMitigatedCss == bearMitigatedCss && cacheFVGSessions2[idx].EqualsInput(input))
						return cacheFVGSessions2[idx];
			return CacheIndicator<RajIndicators.FVGSessions2>(new RajIndicators.FVGSessions2(){ BullCss = bullCss, BullAreaCss = bullAreaCss, BullMitigatedCss = bullMitigatedCss, BearCss = bearCss, BearAreaCss = bearAreaCss, BearMitigatedCss = bearMitigatedCss }, input, ref cacheFVGSessions2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RajIndicators.FVGSessions2 FVGSessions2(Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			return indicator.FVGSessions2(Input, bullCss, bullAreaCss, bullMitigatedCss, bearCss, bearAreaCss, bearMitigatedCss);
		}

		public Indicators.RajIndicators.FVGSessions2 FVGSessions2(ISeries<double> input , Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			return indicator.FVGSessions2(input, bullCss, bullAreaCss, bullMitigatedCss, bearCss, bearAreaCss, bearMitigatedCss);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RajIndicators.FVGSessions2 FVGSessions2(Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			return indicator.FVGSessions2(Input, bullCss, bullAreaCss, bullMitigatedCss, bearCss, bearAreaCss, bearMitigatedCss);
		}

		public Indicators.RajIndicators.FVGSessions2 FVGSessions2(ISeries<double> input , Brush bullCss, Brush bullAreaCss, Brush bullMitigatedCss, Brush bearCss, Brush bearAreaCss, Brush bearMitigatedCss)
		{
			return indicator.FVGSessions2(input, bullCss, bullAreaCss, bullMitigatedCss, bearCss, bearAreaCss, bearMitigatedCss);
		}
	}
}

#endregion
