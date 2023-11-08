#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

namespace NinjaTrader.NinjaScript.Indicators.Infinity
{
	public class SupDemZones : Indicator
	{
		#region Zone
		
		public class Zone
		{
			public double h = 0.0;   // high
			public double l = 0.0;   // low
			public int    b = 0;     // bar
			public int    e = 0;     // end
			public string t = "";    // type
			public string c = "";    // context
			public bool   f = false; // flipped
			public bool   a = true;  // active
			
			public Zone(double l, double h, int b, string t, string c, bool a)
			{
				this.l = l;
				this.h = h;
				this.b = b;
				this.t = t;
				this.c = c;
				this.a = a;
			}
		}
		
		#endregion
		
		#region Variables
		
		private int  barIndex  = 0;
		private int  atrPeriod = 10;
		
		// ---
		
		private int    currHiBar,currLoBar,prevHiBar,prevLoBar = 0;
		private double currHiVal,currLoVal,prevHiVal,prevLoVal = 0;
		
		private int    con;
		private double currLoCon,currHiCon;
		
		private double zr,zl,zh,br;
		private int    zb;
		private string zt,zc;
		private bool   za;
		
		private double atr;
		
		// --- //
		
		private List<Zone> Zones = new List<Zone>();
		
		// --- //
		
		private Separator menuSepa;
		private MenuItem  menuItem;
	    private MenuItem  menuItemActive;
		private MenuItem  menuItemBroken;
        private bool	  initOne;
		private bool	  initTwo;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if(State == State.SetDefaults)
			{
				Description					= @"";
				Name						= "SupDemZones";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				IsAutoScale 				= false;
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
				IsSuspendedWhileInactive	= false;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				
				useMTF				= false;
				barType				= barTypes.Minute;
				barPeriod 			= 30;
				demandColor			= Brushes.YellowGreen;
				supplyColor			= Brushes.Tomato;
				activeLineOpacity	= 0.30f;
				activeAreaOpacity	= 0.15f;
				brokenLineOpacity	= 0.10f;
				brokenAreaOpacity	= 0.05f;
				lineWidth			= 1;
				extendZones			= false;
				hideActiveZones 	= false;
				hideBrokenZones 	= true;
				indicatorVersion	= "1.2 | Dec. 2016";
			}
			else if(State == State.Configure)
			{
				if(useMTF)
				{
					barIndex = 1;
					AddDataSeries((BarsPeriodType)barType, barPeriod);
				}
				
				ZOrder = ChartBars.ZOrder - 7;
				
				if(!initOne)
                {
					menuSepa	   = new Separator();
					menuItem	   = new MenuItem { Header = "Zones" };
					menuItemActive = new MenuItem { Header = (hideActiveZones) ? "Show Active Zones" : "Hide Active Zones" };
					menuItemBroken = new MenuItem { Header = (hideBrokenZones) ? "Show Broken Zones" : "Hide Broken Zones" };
					
					menuItemActive.Click += toggleActiveZones;
			        menuItemBroken.Click += toggleBrokenZones;
					
					menuItem.Items.Add(menuItemActive);
					menuItem.Items.Add(menuItemBroken);
					
					initOne = true;
				}
			}
			else if(State == State.Terminated)
			{
				if(ChartControl == null || menuItem == null) return;
				
				try
        		{
					ChartControl.Dispatcher.InvokeAsync(() =>
	                {
						if(ChartControl.ContextMenu.Items.Contains(menuSepa)) ChartControl.ContextMenu.Items.Remove(menuSepa);
						if(ChartControl.ContextMenu.Items.Contains(menuItem)) ChartControl.ContextMenu.Items.Remove(menuItem);
					});
					
					if(menuItemActive == null || menuItemBroken == null) return;
					
					ChartControl.Dispatcher.InvokeAsync(() =>
	                {
						menuItemActive.Click -= toggleActiveZones;
	                	menuItemBroken.Click -= toggleBrokenZones;
					
	                	ChartControl.ContextMenuOpening -= contextMenuOpening;
	                	ChartControl.ContextMenuClosing -= contextMenuClosing;
					});
				}
				catch(Exception e)
		        {
		            Console.WriteLine("Zones: '{0}'", e);
		        }
			}
			
			if(ChartControl == null) return;
			
			if(!initTwo)
			{
				try
        		{
					ChartControl.Dispatcher.InvokeAsync(() =>
                	{
				        ChartControl.ContextMenuOpening += contextMenuOpening;
				    	ChartControl.ContextMenuClosing += contextMenuClosing;
				    });
					
					initTwo = true;
				}
				catch(Exception e)
		        {
		            Console.WriteLine("Zones: '{0}'", e);
		        }
			}
		}
		
		#region contextMenu
		
		// contextMenuOpening
		//
		private void contextMenuOpening(object sender, ContextMenuEventArgs e)
        {
			try
        	{
				ChartControl.ContextMenu.Items.Add(menuSepa);
				ChartControl.ContextMenu.Items.Add(menuItem);
			}
			catch (Exception error)
	        {
	            //Console.WriteLine("Burp: '{0}'", error);
	        }
        }
		
		// contextMenuClosing
		//
		private void contextMenuClosing(object sender, ContextMenuEventArgs e)
        {
			try
        	{
				if(ChartControl.ContextMenu.Items.Contains(menuSepa)) ChartControl.ContextMenu.Items.Remove(menuSepa);
				if(ChartControl.ContextMenu.Items.Contains(menuItem)) ChartControl.ContextMenu.Items.Remove(menuItem);
			}
			catch (Exception error)
	        {
	            //Console.WriteLine("Burp: '{0}'", error);
	        }
        }
		
		// toggleActiveZones
		//
		private void toggleActiveZones(object sender, RoutedEventArgs e)
        {
            hideActiveZones = !hideActiveZones;
			menuItemActive.Header = (hideActiveZones) ? "Show Active Zones" : "Hide Active Zones";
			ForceRefresh();
        }
		
		// toggleBrokenZones
		//
		private void toggleBrokenZones(object sender, RoutedEventArgs e)
        {
            hideBrokenZones = !hideBrokenZones;
			menuItemBroken.Header = (hideActiveZones) ? "Show Broken Zones" : "Hide Broken Zones";
			ForceRefresh();
        }
		
		#endregion
		
		// OnBarUpdate
		//
		protected override void OnBarUpdate()
		{
			if(CurrentBars[barIndex] < 20) { return; }
			
			if(isDnSwing(3))
			{
				currHiBar = 3;
				currHiVal = Highs[barIndex][3];
			}
			
			if(isUpSwing(3))
			{
				currLoBar = 3;
				currLoVal = Lows[barIndex][3];
			}
			
			atr = Instrument.MasterInstrument.RoundToTickSize(ATR(BarsArray[barIndex], atrPeriod)[0] * 1.25);
			
			checkSupply();
			checkDemand();
			updateZones();
			
			prevHiBar = currHiBar;
			prevHiVal = currHiVal;
			
			prevLoBar = currLoBar;
			prevLoVal = currLoVal;
		}
		
		// isUpSwing
		//
		private bool isUpSwing(int index)
		{
			if(
			Lows[barIndex][index] <= Lows[barIndex][index-1] &&
			Lows[barIndex][index] <= Lows[barIndex][index-2] &&
			Lows[barIndex][index] <= Lows[barIndex][index-3] &&
			Lows[barIndex][index] <= Lows[barIndex][index+1] &&
			Lows[barIndex][index] <= Lows[barIndex][index+2] &&
			Lows[barIndex][index] <= Lows[barIndex][index+3] &&
			(Lows[barIndex][index] < Lows[barIndex][index-1] || Lows[barIndex][index] < Lows[barIndex][index-2] || Lows[barIndex][index] < Lows[barIndex][index-3]) &&
			(Lows[barIndex][index] < Lows[barIndex][index+1] || Lows[barIndex][index] < Lows[barIndex][index+2] || Lows[barIndex][index] < Lows[barIndex][index+3])
			) {
				return true;
			}
			
			return false;
		}
		
		// isDnSwing
		//
		private bool isDnSwing(int index)
		{
			if(
			Highs[barIndex][index] >= Highs[barIndex][index-1] &&
			Highs[barIndex][index] >= Highs[barIndex][index-2] &&
			Highs[barIndex][index] >= Highs[barIndex][index-3] &&
			Highs[barIndex][index] >= Highs[barIndex][index+1] &&
			Highs[barIndex][index] >= Highs[barIndex][index+2] &&
			Highs[barIndex][index] >= Highs[barIndex][index+3] &&
			(Highs[barIndex][index] > Highs[barIndex][index-1] || Highs[barIndex][index] > Highs[barIndex][index-2] || Highs[barIndex][index] > Highs[barIndex][index-3]) &&
			(Highs[barIndex][index] > Highs[barIndex][index+1] || Highs[barIndex][index] > Highs[barIndex][index+2] || Highs[barIndex][index] > Highs[barIndex][index+3])
			) {
				return true;
			}
			
			return false;
		}
		
		// checkSupply
		//
		private void checkSupply()
		{
			// Regular
			
			if(currHiVal != prevHiVal)
			{
				if(MAX(Highs[barIndex], currHiBar)[0] <= currHiVal)
				{
					if(!activeSupplyZoneExists(currHiVal) && isValidSupplyZone(currHiVal, currLoVal))
					{
						br = Highs[barIndex][currHiBar] - Lows[barIndex][currHiBar];
						zr = Highs[barIndex][currHiBar] - Math.Min(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]);
					 	zl = (zr > atr) ? Math.Max(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]): Math.Min(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]);
						zh = currHiVal;
						zb = CurrentBars[barIndex] - currHiBar;
						zt = "s";
						zc = "r";
						za = true;
						
						zl = (zh - zl < TickSize) ? (zh - TickSize) : zl;
						//zl = (zh - zl > atr) ? (zh - atr) : zl;
						
						Zones.Add(new Zone(zl, zh, zb, zt, zc, za));
					}
				}
			}
			
			// Continuation
			
			con = isDnContinuation();
			
			if(con != -1)
			{
				currHiCon = MAX(Highs[barIndex], con)[0];
				currLoCon = MIN(Lows[barIndex], con)[1];
				
				if(currHiCon - currLoCon <= atr)
				{
					if(!activeSupplyZoneExists(currHiCon) && isValidSupplyZone(currHiCon, currLoCon))
					{
						zl = currLoCon;
						zh = currHiCon;
						zb = CurrentBars[barIndex] - (con);
						zt = "s";
						zc = "c";
						za = true;
						
						zl = (zh - zl < TickSize) ? (zh - TickSize) : zl;
						
						Zones.Add(new Zone(zl, zh, zb, zt, zc, za));
					}
				}
			}
		}
		
		#region Supply
		
		// getNextSupplyZone
		//
		private int getNextSupplyZone(double price)
		{
			double min = double.MaxValue;
			int    ind = -1;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "s")
				{
					if(Zones[i].l > price && Zones[i].l < min)
					{
						ind = i;
					}
				}
			}
			
			return ind;
		}
		
		// activeSupplyZoneExists
		//
		private bool activeSupplyZoneExists(double hi)
		{
			bool exists = false;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "s")
				{
					if(Zones[i].h == hi)
					{
						exists = true;
						break;
					}
				}
			}
			
			return exists;
		}
		
		// isValidSupplyZone
		//
		private bool isValidSupplyZone(double hi, double lo)
		{
			bool valid = true;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "s")
				{
					if(
					(hi <= Zones[i].h && hi >= Zones[i].l) ||
					(lo <= Zones[i].h && lo >= Zones[i].l)
					) {
						valid = false;
						break;
					}
				}
			}
			
			return valid;
		}
		
		// isDnContinuation
		//
		private int isDnContinuation()
		{
			bool val = true;
			int  bar = -1;
			
			for(int i=10;i>=2;i--)
			{
				if(isDnMove(i))
				{
					val = true;
					
					for(int j=i;j>=1;j--)
					{
						if(!isInsideDnBar(j, i))
						{
							val = false;
							break;
						}
					}
					
					if(val)
					{
						val = false;
						
						for(int j=i;j>=1;j--)
						{
							if(Closes[barIndex][j] >= Opens[barIndex][j])
							{
								val = true;
								break;
							}
						}
					}
					
					if(val)
					{
						if(isInsideDnBreakoutBar(0, i))
						{
							bar = i;
							break;
						}
					}
				}
			}
			
			return bar;
		}
		
		// isDnMove
		//
		private bool isDnMove(int index)
		{
			if(
			Closes[barIndex][index]   < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index]   ||
			Closes[barIndex][index+1] < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index+1] ||
			Closes[barIndex][index+2] < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index+2]
			) {
				if(
				isDnBar(index) &&
				isDnBar(index+1) &&
				isDnBar(index+2)
				) {
					return true;
				}
				
				if(
				isStrongDnBar(index)
				) {
					return true;
				}
			}
			
			return false;
		}
		
		// isDnBar
		//
		private bool isDnBar(int index)
		{
			if(
			Closes[barIndex][index] < Opens[barIndex][index] &&
			Closes[barIndex][index] < Closes[barIndex][index+1] &&
			Highs[barIndex][index]  < Highs[barIndex][index+1] &&
			Lows[barIndex][index]   < Lows[barIndex][index+1]
			) {
				return true;
			}
			
			return false;
		}
		
		// isStrongDnBar
		//
		private bool isStrongDnBar(int index)
		{
			if(
			Closes[barIndex][index] < Opens[barIndex][index] &&
			Closes[barIndex][index] < Closes[barIndex][index+1] &&
			Highs[barIndex][index]  < Highs[barIndex][index+1] &&
			Lows[barIndex][index]   < Lows[barIndex][index+1] &&
			Lows[barIndex][index]   < MIN(Lows[barIndex], 3)[index+1] &&
			Highs[barIndex][index]  - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1]
			) {
				return true;
			}
			
			if(
			Closes[barIndex][index] < Opens[barIndex][index] &&
			Closes[barIndex][index] < Closes[barIndex][index+1] &&
			Closes[barIndex][index] < MIN(Lows[barIndex], 3)[index+1] &&
			Highs[barIndex][index]  - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1] * 2
			) {
				return true;
			}
			
			return false;
		}
		
		// isInsideDnBar
		//
		private bool isInsideDnBar(int indexOne, int indexTwo)
		{
			if(
			Highs[barIndex][indexOne] <= Highs[barIndex][indexTwo] &&
			Math.Min(Opens[barIndex][indexOne], Closes[barIndex][indexOne]) >= Lows[barIndex][indexTwo]
			) {
				return true;
			}
			
			return false;
		}
		
		// isInsideDnBreakoutBar
		//
		private bool isInsideDnBreakoutBar(int indexOne, int indexTwo)
		{
			if(
			Highs[barIndex][indexOne]  <= Highs[barIndex][indexTwo] &&
			Closes[barIndex][indexOne] <= MIN(Lows[barIndex], indexTwo-indexOne)[1] &&
			Lows[barIndex][indexOne]   <  MIN(Lows[barIndex], indexTwo-indexOne)[1]
			) {
				return true;
			}
			
			return false;
		}
		
		#endregion
		
		// checkDemand
		//
		private void checkDemand()
		{
			// Regular
			
			if(currLoVal != prevLoVal)
			{
				if(MIN(Lows[barIndex], currLoBar)[0] >= currLoVal)
				{
					if(!activeDemandZoneExists(currLoVal) && isValidDemandZone(currHiVal, currLoVal))
					{
						br = Highs[barIndex][currHiBar] - Lows[barIndex][currHiBar];
						zr = Math.Max(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]) - Lows[barIndex][currHiBar];
						zl = currLoVal;
						zh = (zr > atr) ? Math.Min(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]) : Math.Max(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]);
						zb = CurrentBars[barIndex] - currLoBar;
						zt = "d";
						zc = "r";
						za = true;
						
						zh = (zh - zl < TickSize) ? (zl + TickSize) : zh;
						//zh = (zh - zl > atr) ? (zl + atr) : zh;
						
						Zones.Add(new Zone(zl, zh, zb, zt, zc, za));
					}
				}
			}
			
			// Continuation
			
			con = isUpContinuation();
			
			if(con != -1)
			{
				currHiCon = MAX(Highs[barIndex], con)[1];
				currLoCon = MIN(Lows[barIndex], con)[0];
				
				if(currHiCon - currLoCon <= atr)
				{
					if(!activeDemandZoneExists(currLoCon) && isValidDemandZone(currHiCon, currLoCon))
					{
						zl = currLoCon;
						zh = currHiCon;
						zb = CurrentBars[barIndex] - (con);
						zt = "d";
						zc = "c";
						za = true;
						
						zh = (zh - zl < TickSize) ? (zl + TickSize) : zh;
						
						Zones.Add(new Zone(zl, zh, zb, zt, zc, za));
					}
				}
			}
		}
		
		#region Demand
		
		// getNextDemandZone
		//
		private int getNextDemandZone(double price)
		{
			double max = double.MinValue;
			int    ind = -1;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "d")
				{
					if(Zones[i].l < price && Zones[i].h > max)
					{
						ind = i;
					}
				}
			}
			
			return ind;
		}
		
		// activeDemandZoneExists
		//
		private bool activeDemandZoneExists(double lo)
		{
			bool exists = false;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "d")
				{
					if(Zones[i].l == lo)
					{
						exists = true;
						break;
					}
				}
			}
			
			return exists;
		}
		
		// isValidDemandZone
		//
		private bool isValidDemandZone(double hi, double lo)
		{
			bool valid = true;
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && Zones[i].t == "d")
				{
					if(
					(lo >= Zones[i].l && lo <= Zones[i].h) ||
					(hi >= Zones[i].l && hi <= Zones[i].h)
					) {
						valid = false;
						break;
					}
				}
			}
			
			return valid;
		}
		
		// isUpContinuation
		//
		private int isUpContinuation()
		{
			bool val = true;
			int  bar = -1;
			
			for(int i=10;i>=2;i--)
			{
				if(isUpMove(i))
				{
					val = true;
					
					for(int j=i-1;j>=1;j--)
					{
						if(!isInsideUpBar(j, i))
						{
							val = false;
							break;
						}
					}
					
					if(val)
					{
						val = false;
						
						for(int j=i;j>=1;j--)
						{
							if(Closes[barIndex][j] <= Opens[barIndex][j])
							{
								val = true;
								break;
							}
						}
					}
					
					if(val)
					{
						if(isInsideUpBreakoutBar(0, i))
						{
							bar = i;
							break;
						}
					}
				}
			}
			
			return bar;
		}
		
		// isUpMove
		//
		private bool isUpMove(int index)
		{
			if(
			Closes[barIndex][index]   > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index]   ||
			Closes[barIndex][index+1] > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index+1] ||
			Closes[barIndex][index+2] > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index+2]
			) {
				if(
				isUpBar(index) &&
				isUpBar(index+1) &&
				isUpBar(index+2)
				) {
					return true;
				}
				
				if(
				isStrongUpBar(index)
				) {
					return true;
				}
			}
			
			return false;
		}
		
		// isUpBar
		//
		private bool isUpBar(int index)
		{
			if(
			Closes[barIndex][index] > Opens[barIndex][index] &&
			Closes[barIndex][index] > Closes[barIndex][index+1] &&
			Highs[barIndex][index]  > Highs[barIndex][index+1] &&
			Lows[barIndex][index]   > Lows[barIndex][index+1]
			) {
				return true;
			}
			
			return false;
		}
		
		// isStrongUpBar
		//
		private bool isStrongUpBar(int index)
		{
			if(
			Closes[barIndex][index] > Opens[barIndex][index] &&
			Closes[barIndex][index] > Closes[barIndex][index+1] &&
			Highs[barIndex][index]  > Highs[barIndex][index+1] &&
			Lows[barIndex][index]   > Lows[barIndex][index+1] &&
			Highs[barIndex][index]  > MAX(Highs[barIndex], 3)[index+1] &&
			Highs[barIndex][index]  - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1]
			) {
				return true;
			}
			
			if(
			Closes[barIndex][index] > Opens[barIndex][index] &&
			Closes[barIndex][index] > Closes[barIndex][index+1] &&
			Closes[barIndex][index] > MAX(Highs[barIndex], 3)[index+1] &&
			Highs[barIndex][index]  - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1] * 2
			) {
				return true;
			}
			
			return false;
		}
		
		// isInsideUpBar
		//
		private bool isInsideUpBar(int indexOne, int indexTwo)
		{
			if(
			Lows[barIndex][indexOne]  >= Lows[barIndex][indexTwo] &&
			Highs[barIndex][indexOne] <= Highs[barIndex][indexTwo] &&
			Math.Max(Opens[barIndex][indexOne], Closes[barIndex][indexOne]) <= Highs[barIndex][indexTwo]
			) {
				return true;
			}
			
			return false;
		}
		
		// isInsideUpBreakoutBar
		//
		private bool isInsideUpBreakoutBar(int indexOne, int indexTwo)
		{
			if(
			Lows[barIndex][indexOne]   >= Lows[barIndex][indexTwo] &&
			Closes[barIndex][indexOne] >= MAX(Highs[barIndex], indexTwo-indexOne)[1] &&
			Highs[barIndex][indexOne]  >  MAX(Highs[barIndex], indexTwo-indexOne)[1]
			) {
				return true;
			}
			
			return false;
		}
		
		#endregion
		
		// updateZones
		//
		private void updateZones()
		{
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true)
				{
					if(Zones[i].t == "s")
					{
						if(Highs[barIndex][0] > Zones[i].h)
						{
							Zones[i].e = CurrentBars[barIndex];
							Zones[i].a = false;
						}
					}
					
					if(Zones[i].t == "d")
					{
						if(Lows[barIndex][0] < Zones[i].l)
						{
							Zones[i].e = CurrentBars[barIndex];
							Zones[i].a = false;
						}
					}
				}
			}
		}
		
		// formatPrice
		//
		public string formatPrice(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
		}
		
		// findBar
		//
		private int findBar(Zone z)
		{
			int curr = BarsArray[0].GetBar(BarsArray[1].GetTime(z.b));
			int prev = BarsArray[0].GetBar(BarsArray[1].GetTime(z.b - 1));
			int rVal = curr;
			
			for(int i=prev;i<=curr;i++)
			{
				if(z.t == "s")
				{
					if(BarsArray[0].GetHigh(i) == z.h)
					{
						rVal = i;
						break;
					}
				}
				
				if(z.t == "d")
				{
					if(BarsArray[0].GetLow(i) == z.l)
					{
						rVal = i;
						break;
					}
				}
			}
				
			return rVal;
		}
		
		// OnRender
		//
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if(Bars == null || Bars.Instrument == null || IsInHitTest) { return; }
			
			base.OnRender(chartControl, chartScale);
			
			drawZones(chartControl, chartScale);
		}
		
		// drawZones
		//
		private void drawZones(ChartControl chartControl, ChartScale chartScale)
		{
			if(hideActiveZones && hideBrokenZones) { return; }
			if(Zones.Count == 0) { return; }
			
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
			
			SharpDX.Direct2D1.Brush demandBrush = demandColor.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush supplyBrush = supplyColor.ToDxBrush(RenderTarget);
			
			int x1 = 0;
			int x2 = 0;
			int y1 = 0;
			int y2 = 0;
			
			int wd = (int)(chartControl.BarWidth / 2.0) + (int)(chartControl.BarMarginLeft / 2.0);
			
			for(int i=0;i<Zones.Count;i++)
			{
				if(Zones[i].a == true && hideActiveZones)
				{
					continue;
				}
				if(Zones[i].a == false && hideBrokenZones)
				{
					continue;
				}
				
				if(barIndex == 0)
				{
					x1 = ChartControl.GetXByBarIndex(ChartBars, Zones[i].b);
					x2 = (Zones[i].a == false) ? ChartControl.GetXByBarIndex(ChartBars, Zones[i].e) : (int)(ChartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex) + wd);
					x2 = (Zones[i].a == true && extendZones) ? chartControl.CanvasRight: x2;
				}
				else
				{
					x1 = ChartControl.GetXByBarIndex(ChartBars, findBar(Zones[i]));
					//x1 = ChartControl.GetXByBarIndex(ChartBars, BarsArray[0].GetBar(BarsArray[1].GetTime(Zones[i].b)));
					x2 = (Zones[i].a == false) ? ChartControl.GetXByBarIndex(ChartBars, ChartBars.GetBarIdxByTime(chartControl, BarsArray[1].GetTime(Zones[i].e))) : (int)(ChartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex) + wd);
					x2 = (Zones[i].a == true && extendZones) ? chartControl.CanvasRight: x2;
				}
				
				if(x2 < x1) { continue; }
				
				y1 = chartScale.GetYByValue(Zones[i].h);
				y2 = chartScale.GetYByValue(Zones[i].l);
				
				// area
				
				SharpDX.RectangleF rect = new SharpDX.RectangleF();
	 			
				rect.X      = (float)x1;
				rect.Y      = (float)y1;
				rect.Width  = (float)Math.Abs(x2 - x1);
				rect.Height = (float)Math.Abs(y1 - y2) - 1;
				
				if(Zones[i].a == true)
				{
					demandBrush.Opacity = activeAreaOpacity;
					supplyBrush.Opacity = activeAreaOpacity;
				}
				else
				{
					demandBrush.Opacity = brokenAreaOpacity;
					supplyBrush.Opacity = brokenAreaOpacity;
				}
				
				if(Zones[i].t == "d")
				{
					RenderTarget.FillRectangle(rect, demandBrush);
				}
				
				if(Zones[i].t == "s")
				{
					RenderTarget.FillRectangle(rect, supplyBrush);
				}
				
				// line one
				
				if(Zones[i].a == true)
				{
					demandBrush.Opacity = activeLineOpacity;
					supplyBrush.Opacity = activeLineOpacity;
				}
				else
				{
					demandBrush.Opacity = brokenLineOpacity;
					supplyBrush.Opacity = brokenLineOpacity;
				}
				
				SharpDX.Vector2 pOne = new SharpDX.Vector2();
				SharpDX.Vector2 pTwo = new SharpDX.Vector2();
				
				pOne.X = (float)x1;
				pOne.Y = (float)y1;
				pTwo.X = (float)x2;
				pTwo.Y = (float)y1;
				
				if(Zones[i].t == "d")
				{
					RenderTarget.DrawLine(pOne, pTwo, demandBrush, lineWidth);
				}
				
				if(Zones[i].t == "s")
				{
					RenderTarget.DrawLine(pOne, pTwo, supplyBrush, lineWidth);
				}
				
				// line two
				
				pOne.X = (float)x1;
				pOne.Y = (float)y2;
				pTwo.X = (float)x2;
				pTwo.Y = (float)y2;
				
				if(Zones[i].t == "d")
				{
					RenderTarget.DrawLine(pOne, pTwo, demandBrush, lineWidth);
				}
				
				if(Zones[i].t == "s")
				{
					RenderTarget.DrawLine(pOne, pTwo, supplyBrush, lineWidth);
				}
			}
			
			RenderTarget.AntialiasMode = oldAntialiasMode;
			
			// ---
			
			demandBrush.Dispose();
			supplyBrush.Dispose();
		}
		
		#region Poperties
		
		[NinjaScriptProperty]
		[Display(Name = "Use MTF", GroupName = "1.0 MTF Parameters", Order = 0)]
		public bool useMTF
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Display(Name = "Bar Type", GroupName = "1.0 MTF Parameters", Order = 1)]
		public barTypes barType
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Bar Period", GroupName = "1.0 MTF Parameters", Order = 2)]
		public int barPeriod
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Demand Color", GroupName = "2.0 Zone Parameters", Order = 0)]
		public Brush demandColor
		{ get; set; }
		
		[Browsable(false)]
		public string demandColorSerializable
		{
			get { return Serialize.BrushToString(demandColor); }
			set { demandColor = Serialize.StringToBrush(value); }
		}
		
		// ---
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Supply Color", GroupName = "2.0 Zone Parameters", Order = 1)]
		public Brush supplyColor
		{ get; set; }
		
		[Browsable(false)]
		public string supplyColorSerializable
		{
			get { return Serialize.BrushToString(supplyColor); }
			set { supplyColor = Serialize.StringToBrush(value); }
		}
		
		// ---
		
		[NinjaScriptProperty]
		[Range(0.0, 1.0)]
		[Display(Name = "Active Zone Line Opacity", GroupName = "2.0 Zone Parameters", Order = 2)]
		public float activeLineOpacity
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Range(0.0, 1.0)]
		[Display(Name = "Active Zone Area Opacity", GroupName = "2.0 Zone Parameters", Order = 3)]
		public float activeAreaOpacity
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Range(0.0, 1.0)]
		[Display(Name = "Broken Zone Line Opacity", GroupName = "2.0 Zone Parameters", Order = 4)]
		public float brokenLineOpacity
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Range(0.0, 1.0)]
		[Display(Name = "Broken Zone Area Opacity", GroupName = "2.0 Zone Parameters", Order = 5)]
		public float brokenAreaOpacity
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Line Width", GroupName = "2.0 Zone Parameters", Order = 6)]
		public int lineWidth
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Display(Name = "Extend Zones", GroupName = "2.0 Zone Parameters", Order = 7)]
		public bool extendZones
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Display(Name = "Hide Active Zones", GroupName = "2.0 Zone Parameters", Order = 8)]
		public bool hideActiveZones
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[Display(Name = "Hide Broken Zones", GroupName = "2.0 Zone Parameters", Order = 9)]
		public bool hideBrokenZones
		{ get; set; }
		
		// ---
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name = "Indicator Version", GroupName = "2.0 Zone Parameters", Order = 10)]
		public string indicatorVersion
		{ get; set; }
		
		#endregion
	}
}

public enum barTypes
{
	Day     	= BarsPeriodType.Day,
	Minute  	= BarsPeriodType.Minute,
	Range   	= BarsPeriodType.Range,
	Second  	= BarsPeriodType.Second,
	Tick    	= BarsPeriodType.Tick,
	Volume  	= BarsPeriodType.Volume,
	Renko   	= BarsPeriodType.Renko
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Infinity.SupDemZones[] cacheSupDemZones;
		public Infinity.SupDemZones SupDemZones(bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			return SupDemZones(Input, useMTF, barType, barPeriod, demandColor, supplyColor, activeLineOpacity, activeAreaOpacity, brokenLineOpacity, brokenAreaOpacity, lineWidth, extendZones, hideActiveZones, hideBrokenZones, indicatorVersion);
		}

		public Infinity.SupDemZones SupDemZones(ISeries<double> input, bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			if (cacheSupDemZones != null)
				for (int idx = 0; idx < cacheSupDemZones.Length; idx++)
					if (cacheSupDemZones[idx] != null && cacheSupDemZones[idx].useMTF == useMTF && cacheSupDemZones[idx].barType == barType && cacheSupDemZones[idx].barPeriod == barPeriod && cacheSupDemZones[idx].demandColor == demandColor && cacheSupDemZones[idx].supplyColor == supplyColor && cacheSupDemZones[idx].activeLineOpacity == activeLineOpacity && cacheSupDemZones[idx].activeAreaOpacity == activeAreaOpacity && cacheSupDemZones[idx].brokenLineOpacity == brokenLineOpacity && cacheSupDemZones[idx].brokenAreaOpacity == brokenAreaOpacity && cacheSupDemZones[idx].lineWidth == lineWidth && cacheSupDemZones[idx].extendZones == extendZones && cacheSupDemZones[idx].hideActiveZones == hideActiveZones && cacheSupDemZones[idx].hideBrokenZones == hideBrokenZones && cacheSupDemZones[idx].indicatorVersion == indicatorVersion && cacheSupDemZones[idx].EqualsInput(input))
						return cacheSupDemZones[idx];
			return CacheIndicator<Infinity.SupDemZones>(new Infinity.SupDemZones(){ useMTF = useMTF, barType = barType, barPeriod = barPeriod, demandColor = demandColor, supplyColor = supplyColor, activeLineOpacity = activeLineOpacity, activeAreaOpacity = activeAreaOpacity, brokenLineOpacity = brokenLineOpacity, brokenAreaOpacity = brokenAreaOpacity, lineWidth = lineWidth, extendZones = extendZones, hideActiveZones = hideActiveZones, hideBrokenZones = hideBrokenZones, indicatorVersion = indicatorVersion }, input, ref cacheSupDemZones);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Infinity.SupDemZones SupDemZones(bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			return indicator.SupDemZones(Input, useMTF, barType, barPeriod, demandColor, supplyColor, activeLineOpacity, activeAreaOpacity, brokenLineOpacity, brokenAreaOpacity, lineWidth, extendZones, hideActiveZones, hideBrokenZones, indicatorVersion);
		}

		public Indicators.Infinity.SupDemZones SupDemZones(ISeries<double> input , bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			return indicator.SupDemZones(input, useMTF, barType, barPeriod, demandColor, supplyColor, activeLineOpacity, activeAreaOpacity, brokenLineOpacity, brokenAreaOpacity, lineWidth, extendZones, hideActiveZones, hideBrokenZones, indicatorVersion);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Infinity.SupDemZones SupDemZones(bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			return indicator.SupDemZones(Input, useMTF, barType, barPeriod, demandColor, supplyColor, activeLineOpacity, activeAreaOpacity, brokenLineOpacity, brokenAreaOpacity, lineWidth, extendZones, hideActiveZones, hideBrokenZones, indicatorVersion);
		}

		public Indicators.Infinity.SupDemZones SupDemZones(ISeries<double> input , bool useMTF, barTypes barType, int barPeriod, Brush demandColor, Brush supplyColor, float activeLineOpacity, float activeAreaOpacity, float brokenLineOpacity, float brokenAreaOpacity, int lineWidth, bool extendZones, bool hideActiveZones, bool hideBrokenZones, string indicatorVersion)
		{
			return indicator.SupDemZones(input, useMTF, barType, barPeriod, demandColor, supplyColor, activeLineOpacity, activeAreaOpacity, brokenLineOpacity, brokenAreaOpacity, lineWidth, extendZones, hideActiveZones, hideBrokenZones, indicatorVersion);
		}
	}
}

#endregion
