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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class ZigZagStrat : Strategy
    {
        private bool useHA = false;
        private bool useAltTF = true;
        private int tf = 60;
        private bool showPatterns = true;
        private bool showFib0000 = true;
        private bool showFib0236 = true;
        private bool showFib0382 = true;
        private bool showFib0500 = true;
        private bool showFib0618 = true;
        private bool showFib0764 = true;
        private bool showFib1000 = true;

        private Series<bool> _isUp;
        private Series<bool> _isDown;
        private Series<int> _direction;
        private Series<double?> sz;
		
		private bool countOnce = false;        

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ZigZagStrat";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 40;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
				
				countOnce = true;
				tf = 60;
            }
            else if (State == State.Configure)
            {
				BarsRequiredToTrade = 40;
                useHA = false;
                _isUp = new Series<bool>(this);
                _isDown = new Series<bool>(this);
                _direction = new Series<int>(this);               

                sz = new Series<double?>(this);
				
				AddDataSeries(Data.BarsPeriodType.Minute, 60);
            }
            else if (State == State.DataLoaded)
            {
                //zig1 = ZigZag(deviationType: DeviationType.Points, deviationValue: 5, useHighLow: true);
                //zig2 = ZigZag(BarsArray[1], deviationType: DeviationType.Points, deviationValue: 5, useHighLow: true);

                ClearOutputWindow(); //Clears Output window every time strategy is enabled
            }
        }
		
		private double? zigzag()
        {
            _isUp[0] = Closes[1][0] >= Opens[1][0];
            _isDown[0] = Closes[1][0] <= Opens[1][0];
            _direction[0] = _isUp[1] && _isDown[0] ? -1 : _isDown[1] && _isUp[0] ? 1 : nz(_direction[1]);
            double? highest = Math.Max(Highs[1][0], Highs[1][1]);
            double? lowest = Math.Min(Lows[1][0], Lows[1][1]);

            double? _zigzag = (_isUp[1] && _isDown[0] && _direction[1] != -1) ? highest : _isDown[1] && _isUp[0] && _direction[1] != 1 ? lowest : null;
            return _zigzag;
        }

        protected override void OnBarUpdate()
        {			
            try{
				if (State == State.Realtime && IsFirstTickOfBar)  // Ignore intrabar ticks
					return;
				
				if (BarsInProgress != 0)
	                return;

	            if (Bars.BarsSinceNewTradingDay < 1) //Needs more than 1 bar on new day to begin trading. (Prevents trades if previous day closed as a pattern for our entry)			
	                return;

	            if (CurrentBar < BarsRequiredToTrade) 
					return;
				
				if (countOnce)
				{
                    //_ticker = useHA ? HeikenAshi(tickerid) : tickerid;
                    //sz = useAltTF ? (change(time(tf)) != 0 ? security(_ticker, tf, zigzag()) : na) : zigzag();

                    // sz = !useAltTF ? zigzag() : ();

                    sz[0] = (double?)zigzag();
                    
		            // plot(sz, title='zigzag', col||=black, linewidth=2);

		            //  ||---   Pattern Recognition:;		            
		            double? x = ValueWhen(sz, 4);
		            double? a = ValueWhen(sz, 3);
		            double? b = ValueWhen(sz, 2);
		            double? c = ValueWhen(sz, 1);
		            double? d = ValueWhen(sz, 0);
					
					if (x == null || a == null || b == null || c == null || d == null)
						return;

		            double? xab = (Math.Abs(b.Value - a.Value) / Math.Abs(x.Value - a.Value));
		            double? xad = (Math.Abs(a.Value - d.Value) / Math.Abs(x.Value - a.Value));
		            double? abc = (Math.Abs(b.Value - c.Value) / Math.Abs(a.Value - b.Value));
		            double? bcd = (Math.Abs(c.Value - d.Value) / Math.Abs(b.Value - c.Value));


		            // plotshape(not showPatterns ? na : isABCD(-1) && not isABCD(-1)[1], text="\nAB=CD", title='Bear ABCD', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0, offset=-2);
		            // plotshape(not showPatterns ? na : isBat(-1) && not isBat(-1)[1], text="Bat", title='Bear Bat', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0, offset=-2);
		            // plotshape(not showPatterns ? na : isAntiBat(-1) && not isAntiBat(-1)[1], text="Anti Bat", title='Bear Anti Bat', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0, offset=-2);
		            // plotshape(not showPatterns ? na : isAltBat(-1) && not isAltBat(-1)[1], text="Alt Bat", title='Bear Alt Bat', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isButterfly(-1) && not isButterfly(-1)[1], text="Butterfly", title='Bear Butterfly', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isAntiButterfly(-1) && not isAntiButterfly(-1)[1], text="Anti Butterfly", title='Bear Anti Butterfly', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isGartley(-1) && not isGartley(-1)[1], text="Gartley", title='Bear Gartley', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isAntiGartley(-1) && not isAntiGartley(-1)[1], text="Anti Gartley", title='Bear Anti Gartley', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isCrab(-1) && not isCrab(-1)[1], text="Crab", title='Bear Crab', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isAntiCrab(-1) && not isAntiCrab(-1)[1], text="Anti Crab", title='Bear Anti Crab', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isShark(-1) && not isShark(-1)[1], text="Shark", title='Bear Shark', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isAntiShark(-1) && not isAntiShark(-1)[1], text="Anti Shark", title='Bear Anti Shark', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : is5o(-1) && not is5o(-1)[1], text="5-O", title='Bear 5-O', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isWolf(-1) && not isWolf(-1)[1], text="Wolf Wave", title='Bear Wolf Wave', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isHnS(-1) && not isHnS(-1)[1], text="Head && Shoulders", title='Bear Head && Shoulders', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isConTria(-1) && not isConTria(-1)[1], text="Contracting Triangle", title='Bear Contracting triangle', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            // plotshape(not showPatterns ? na : isExpTria(-1) && not isExpTria(-1)[1], text="Exp&&ing Triangle", title='Bear Exp&&ing Triangle', style=shape.labeldown, col||=maroon, textcol||=white, location=location.top, transp=0);
		            ;
		            // plotshape(not showPatterns ? na : isABCD(1) && not isABCD(1)[1], text="AB=CD\n", title='Bull ABCD', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isBat(1) && not isBat(1)[1], text="Bat", title='Bull Bat', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAntiBat(1) && not isAntiBat(1)[1], text="Anti Bat", title='Bull Anti Bat', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAltBat(1) && not isAltBat(1)[1], text="Alt Bat", title='Bull Alt Bat', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isButterfly(1) && not isButterfly(1)[1], text="Butterfly", title='Bull Butterfly', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAntiButterfly(1) && not isAntiButterfly(1)[1], text="Anti Butterfly", title='Bull Anti Butterfly', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isGartley(1) && not isGartley(1)[1], text="Gartley", title='Bull Gartley', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAntiGartley(1) && not isAntiGartley(1)[1], text="Anti Gartley", title='Bull Anti Gartley', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isCrab(1) && not isCrab(1)[1], text="Crab", title='Bull Crab', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAntiCrab(1) && not isAntiCrab(1)[1], text="Anti Crab", title='Bull Anti Crab', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isShark(1) && not isShark(1)[1], text="Shark", title='Bull Shark', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isAntiShark(1) && not isAntiShark(1)[1], text="Anti Shark", title='Bull Anti Shark', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : is5o(1) && not is5o(1)[1], text="5-O", title='Bull 5-O', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isWolf(1) && not isWolf(1)[1], text="Wolf Wave", title='Bull Wolf Wave', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isHnS(1) && not isHnS(1)[1], text="Head && Shoulders", title='Bull Head && Shoulders', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isConTria(1) && not isConTria(1)[1], text="Contracting Triangle", title='Bull Contracting Triangle', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            // plotshape(not showPatterns ? na : isExpTria(1) && not isExpTria(1)[1], text="Exp&&ing Triangle", title='Bull Exp&&ing Triangle', style=shape.labelup, col||=green, textcol||=white, location=location.bottom, transp=0);
		            
		            //-------------------------------------------------------------------------------------------------------------------------------------------------------------;

		            double fib_range = Math.Abs(d.Value - c.Value);
		            // fib_0000 = !showFib0000 ? na : d > c ? d-(fib_range*0.000):d+(fib_range*0.000);
		            // fib_0236 = !showFib0236 ? na : d > c ? d-(fib_range*0.236):d+(fib_range*0.236);
		            // fib_0382 = !showFib0382 ? na : d > c ? d-(fib_range*0.382):d+(fib_range*0.382);
		            // fib_0500 = !showFib0500 ? na : d > c ? d-(fib_range*0.500):d+(fib_range*0.500);
		            // fib_0618 = !showFib0618 ? na : d > c ? d-(fib_range*0.618):d+(fib_range*0.618);
		            // fib_0764 = !showFib0764 ? na : d > c ? d-(fib_range*0.764):d+(fib_range*0.764);
		            // fib_1000 = !showFib1000 ? na : d > c ? d-(fib_range*1.000):d+(fib_range*1.000);
		            // plot(title='Fib 0.000', series=fib_0000, col||=fib_0000 != fib_0000[1] ? na : black);
		            // plot(title='Fib 0.236', series=fib_0236, col||=fib_0236 != fib_0236[1] ? na : red);
		            // plot(title='Fib 0.382', series=fib_0382, col||=fib_0382 != fib_0382[1] ? na : olive);
		            // plot(title='Fib 0.500', series=fib_0500, col||=fib_0500 != fib_0500[1] ? na : lime);
		            // plot(title='Fib 0.618', series=fib_0618, col||=fib_0618 != fib_0618[1] ? na : teal);
		            // plot(title='Fib 0.764', series=fib_0764, col||=fib_0764 != fib_0764[1] ? na : blue);
		            // plot(title='Fib 1.000', series=fib_1000, col||=fib_1000 != fib_1000[1] ? na : black);
		            ;
		            // bgcol||(!useAltTF ? na : change(time(tf))!=0?black:na);
		            // f_last_fib(_rate)=>d > c ? d-(fib_range*_rate):d+(fib_range*_rate);

		            double target01_ew_rate = 0.236;
		            double target01_tp_rate = 0.618;
		            double target01_sl_rate = -0.236;
		            bool target02_active = false;
		            double target02_ew_rate = 0.236;
		            double target02_tp_rate = 1.618;
		            double target02_sl_rate = -0.236;


		            Func<int, bool> isBat = (_mode) =>
		            {
		                bool _xab = xab >= 0.382 && xab <= 0.5;
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 1.618 && bcd <= 2.618;
		                bool _xad = xad <= 0.618 && xad <= 1.000;    // 0.886
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAntiBat = (_mode) =>
		            {
		                bool _xab = xab >= 0.500 && xab <= 0.886;    // 0.618
		                bool _abc = abc >= 1.000 && abc <= 2.618;    // 1.13 --> 2.618
		                bool _bcd = bcd >= 1.618 && bcd <= 2.618;    // 2.0  --> 2.618
		                bool _xad = xad >= 0.886 && xad <= 1.000;    // 1.13
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAltBat = (_mode) =>
		            {
		                bool _xab = xab <= 0.382;
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 2.0 && bcd <= 3.618;
		                bool _xad = xad <= 1.13;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isButterfly = (_mode) =>
		            {
		                bool _xab = xab <= 0.786;
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 1.618 && bcd <= 2.618;
		                bool _xad = xad >= 1.27 && xad <= 1.618;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAntiButterfly = (_mode) =>
		            {
		                bool _xab = xab >= 0.236 && xab <= 0.886;    // 0.382 - 0.618
		                bool _abc = abc >= 1.130 && abc <= 2.618;    // 1.130 - 2.618
		                bool _bcd = bcd >= 1.000 && bcd <= 1.382;    // 1.27
		                bool _xad = xad >= 0.500 && xad <= 0.886;    // 0.618 - 0.786
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isABCD = (_mode) =>
		            {
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 1.13 && bcd <= 2.618;
		                return _abc && _bcd && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isGartley = (_mode) =>
		            {
		                bool _xab = xab >= 0.5 && xab <= 0.618;
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 1.13 && bcd <= 2.618;
		                bool _xad = xad >= 0.75 && xad <= 0.875;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAntiGartley = (_mode) =>
		            {
		                bool _xab = xab >= 0.500 && xab <= 0.886;    // 0.618 -> 0.786
		                bool _abc = abc >= 1.000 && abc <= 2.618;    // 1.130 -> 2.618
		                bool _bcd = bcd >= 1.500 && bcd <= 5.000;    // 1.618
		                bool _xad = xad >= 1.000 && xad <= 5.000;    // 1.272
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isCrab = (_mode) =>
		            {
		                bool _xab = xab >= 0.500 && xab <= 0.875;    // 0.886
		                bool _abc = abc >= 0.382 && abc <= 0.886;
		                bool _bcd = bcd >= 2.000 && bcd <= 5.000;    // 3.618
		                bool _xad = xad >= 1.382 && xad <= 5.000;    // 1.618
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAntiCrab = (_mode) =>
		            {
		                bool _xab = xab >= 0.250 && xab <= 0.500;    // 0.276 -> 0.446
		                bool _abc = abc >= 1.130 && abc <= 2.618;    // 1.130 -> 2.618
		                bool _bcd = bcd >= 1.618 && bcd <= 2.618;    // 1.618 -> 2.618
		                bool _xad = xad >= 0.500 && xad <= 0.750;    // 0.618
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isShark = (_mode) =>
		            {
		                bool _xab = xab >= 0.500 && xab <= 0.875;    // 0.5 --> 0.886
		                bool _abc = abc >= 1.130 && abc <= 1.618;    //;
		                bool _bcd = bcd >= 1.270 && bcd <= 2.240;    //;
		                bool _xad = xad >= 0.886 && xad <= 1.130;    // 0.886 --> 1.13
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isAntiShark = (_mode) =>
		            {
		                bool _xab = xab >= 0.382 && xab <= 0.875;    // 0.446 --> 0.618
		                bool _abc = abc >= 0.500 && abc <= 1.000;    // 0.618 --> 0.886
		                bool _bcd = bcd >= 1.250 && bcd <= 2.618;    // 1.618 --> 2.618
		                bool _xad = xad >= 0.500 && xad <= 1.250;    // 1.130 --> 1.130
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> is5o = (_mode) =>
		            {
		                bool _xab = xab >= 1.13 && xab <= 1.618;
		                bool _abc = abc >= 1.618 && abc <= 2.24;
		                bool _bcd = bcd >= 0.5 && bcd <= 0.625; // 0.5
		                bool _xad = xad >= 0.0 && xad <= 0.236; // negative?;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isWolf = (_mode) =>
		            {
		                bool _xab = xab >= 1.27 && xab <= 1.618;
		                bool _abc = abc >= 0 && abc <= 5;
		                bool _bcd = bcd >= 1.27 && bcd <= 1.618;
		                bool _xad = xad >= 0.0 && xad <= 5;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isHnS = (_mode) =>
		            {
		                bool _xab = xab >= 2.0 && xab <= 10;
		                bool _abc = abc >= 0.90 && abc <= 1.1;
		                bool _bcd = bcd >= 0.236 && bcd <= 0.88;
		                bool _xad = xad >= 0.90 && xad <= 1.1;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isConTria = (_mode) =>
		            {
		                bool _xab = xab >= 0.382 && xab <= 0.618;
		                bool _abc = abc >= 0.382 && abc <= 0.618;
		                bool _bcd = bcd >= 0.382 && bcd <= 0.618;
		                bool _xad = xad >= 0.236 && xad <= 0.764;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            Func<int, bool> isExpTria = (_mode) =>
		            {
		                bool _xab = xab >= 1.236 && xab <= 1.618;
		                bool _abc = abc >= 1.000 && abc <= 1.618;
		                bool _bcd = bcd >= 1.236 && bcd <= 2.000;
		                bool _xad = xad >= 2.000 && xad <= 2.236;
		                return _xab && _abc && _bcd && _xad && (_mode == 1 ? d < c : d > c);
		            };

		            bool buy_patterns_00 = isABCD(1) || isBat(1) || isAltBat(1) || isButterfly(1) || isGartley(1) || isCrab(1) || isShark(1) || is5o(1) || isWolf(1) || isHnS(1) || isConTria(1) || isExpTria(1);
		            bool buy_patterns_01 = isAntiBat(1) || isAntiButterfly(1) || isAntiGartley(1) || isAntiCrab(1) || isAntiShark(1);
		            bool sel_patterns_00 = isABCD(-1) || isBat(-1) || isAltBat(-1) || isButterfly(-1) || isGartley(-1) || isCrab(-1) || isShark(-1) || is5o(-1) || isWolf(-1) || isHnS(-1) || isConTria(-1) || isExpTria(-1);
		            bool sel_patterns_01 = isAntiBat(-1) || isAntiButterfly(-1) || isAntiGartley(-1) || isAntiCrab(-1) || isAntiShark(-1);

		            Func<double, double> f_last_fib = (_rate) => d.Value > c.Value ? d.Value - (fib_range * _rate) : d.Value + (fib_range * _rate);

		            bool target01_buy_entry = (buy_patterns_00 || buy_patterns_01) && Close[0] <= f_last_fib(target01_ew_rate);
		            bool target01_buy_close = High[0] >= f_last_fib(target01_tp_rate) || Low[0] <= f_last_fib(target01_sl_rate);
		            bool target01_sel_entry = (sel_patterns_00 || sel_patterns_01) && Close[0] >= f_last_fib(target01_ew_rate);
		            bool target01_sel_close = Low[0] <= f_last_fib(target01_tp_rate) || High[0] >= f_last_fib(target01_sl_rate);
					
		//			Print("before entry target01_buy_entry: " + target01_buy_entry);
		//			Print("before entry target01_buy_close: " + target01_buy_close);
		//			Print("before entry target01_sel_entry: " + target01_sel_entry);
		//			Print("before entry target01_sel_close: " + target01_sel_close);
					
		//			Print(CurrentBar);
					
		            if (target01_buy_entry)
		            {
		                EnterLong("Buy");
		            }
		            else if (target01_buy_close && Position.MarketPosition == MarketPosition.Long)
		            {
		                ExitLong("Buy");
		            }
		            else if (target01_sel_entry)
		            {
		                EnterShort("Sell");
		            }
		            else if (target01_sel_close && Position.MarketPosition == MarketPosition.Short)
		            {
		                ExitShort("Sell");
		            }
					
		            bool target02_buy_entry = target02_active && (buy_patterns_00 || buy_patterns_01) && Close[0] <= f_last_fib(target02_ew_rate);
		            bool target02_buy_close = target02_active && High[0] >= f_last_fib(target02_tp_rate) || Low[0] <= f_last_fib(target02_sl_rate);
		            bool target02_sel_entry = target02_active && (sel_patterns_00 || sel_patterns_01) && Close[0] >= f_last_fib(target02_ew_rate);
		            bool target02_sel_close = target02_active && Low[0] <= f_last_fib(target02_tp_rate) || High[0] >= f_last_fib(target02_sl_rate);			

		            if (target02_buy_entry)
		            {
						Print("before entry 1: " + target01_buy_entry);
		                EnterLong("Buy");
		            }
		            else if (target02_buy_close && Position.MarketPosition == MarketPosition.Long)
		            {
		                ExitLong("Buy");
		            }
		            else if (target02_sel_entry)
		            {
		                EnterShort("Sell");
		            }
		            else if (target02_sel_close && Position.MarketPosition == MarketPosition.Short)
		            {
		                ExitShort("Sell");
		            }
					
					countOnce = false;
				}
				
				if (IsFirstTickOfBar)
				{
					countOnce = true;
				}	            
			}			
	        catch (Exception e)
	        {
	            Print("Exception caught: " + e.Message);
	            Print("Stack Trace: " + e.StackTrace);
				
				// CloseStrategy("Error");
	        }
        }

        private double? ValueWhen(Series<double?> series, int occurrence)
        {
            int found = 0;
            for (int i = 0; i < 256; i++)
            {
                if (series[i] != null)
                {
                    if (found == occurrence)
                    {
                        return series[i];
                    }
                    found++;
                }
            }
            return null;
        }

        private int nz(int? value)
        {
            return value != null ? value.Value : 0;
        }
    }
}
