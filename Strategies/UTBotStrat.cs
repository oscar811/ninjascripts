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
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class UTBotAlerts : Indicator
    {
        private double a, nLoss, xATRTrailingStop, prevATR = 0;
        private int c;
        private string lastSignal = "";



        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "UT Bot Alerts";
                Name = "UTBotAlerts";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                a = 2;  // Key value
                c = 11;  // ATR period
                AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Dot, "BuySignal");
                AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "SellSignal");

            }
            else if (State == State.Configure)
            {


            }
        }

        protected override void OnBarUpdate()
        {

            if (CurrentBar < 1) return;

            Values[0][0] = double.NaN;  // BuySignal
            Values[1][0] = double.NaN;  // SellSignal

            // Calculate the True Range
            double tr = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));

            // Wilder's smoothing method for ATR
            double atr = ((prevATR * (c - 1)) + tr) / c;
            prevATR = atr;

            double nLoss = a * atr;


            // Adjust xATRTrailingStop value based on Close[0]
            if (Close[0] > xATRTrailingStop)
            {
                xATRTrailingStop = Math.Max(xATRTrailingStop, Close[0] - nLoss);
            }
            else
            {
                xATRTrailingStop = Close[0] + nLoss;
            }



            bool buyCondition = Close[0] > xATRTrailingStop && Close[1] <= xATRTrailingStop;
            bool sellCondition = Close[0] < xATRTrailingStop && Close[1] >= xATRTrailingStop;

            if (buyCondition && lastSignal != "Buy")
            {
                Draw.Text(this, "Buy" + CurrentBar.ToString(), "Buy", 0, Low[0] - TickSize * 10, Brushes.Green);
                Alert("UT Long", Priority.High, "UT Long", "Alert.wav", 10, Brushes.Green, Brushes.Black);
                Values[0][0] = Low[0] - 2 * TickSize;
                lastSignal = "Buy";  // Update the lastSignal
            }
            else if (sellCondition && lastSignal != "Sell")
            {


                Draw.Text(this, "Sell" + CurrentBar.ToString(), "Sell", 0, High[0] + TickSize * 10, Brushes.Red);
                Alert("UT Short", Priority.High, "UT Short", "Alert.wav", 10, Brushes.Red, Brushes.Black);
                Values[1][0] = High[0] - 2 * TickSize;
                lastSignal = "Sell";  // Update the lastSignal
            }


        }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> UTBotSignal
        {
            get { return Values[0]; }
        }


        #region Properties

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        public double AValue
        {
            get { return a; }
            set { a = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        public int CValue
        {
            get { return c; }
            set { c = value; }
        }



        #endregion
    }
}