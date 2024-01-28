#region Using declarations
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.Linq;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class SwingLevel
    {
        public double Price { get; set; }
        public bool Broken { get; set; }
        public bool Swept { get; set; }
        public Ray Ray { get; set; }

        public SwingLevel(double price, Ray ray)
        {
            this.Price = price;
            this.Ray = ray;
        }        
    }
}
