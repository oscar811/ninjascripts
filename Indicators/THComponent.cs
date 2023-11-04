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


namespace NinjaTrader.NinjaScript.Indicators.TickHunterTA
{
    public partial class TickHunter
    {
		private int GetTHComponentSysCheck()
		{
			const int SysCheckValue = -1;
			return SysCheckValue;
		}


        private double FilterSurgePriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}

		private double FilterGush1PriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}

		private double FilterFlow1PriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}

		private double FilterFlow2PriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}

		private double FilterFlow3PriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}

		private double FilterFlow4PriceMovement(string signalName, Instrument instrument, MarketPosition marketPosition, double positionAveragePrice, double oldStopLossPrice, double newStopLossPrice)
		{
			return newStopLossPrice;
		}
	}

}
