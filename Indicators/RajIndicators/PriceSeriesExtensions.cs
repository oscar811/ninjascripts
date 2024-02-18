using NinjaTrader.NinjaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTrader.Custom.Indicators.RajIndicators
{
    public static class PriceSeriesExtensions
    {
        public static IEnumerable<double> AsEnumerable(this ISeries<double> series)
        {
            for (int i = series.Count - 1; i >= 0; i--)
            {
                yield return series.GetValueAt(i);
            }
        }
    }
}
