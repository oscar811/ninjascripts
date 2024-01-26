#region Using declarations
using System;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class TradingCondition
    {
        public bool useTimePeriod { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public DayOfWeek[] Days { get; set; }
        public Func<bool> CrossCondition { get; set; }
        public bool HasTraded { get; set; }
        public bool IsLongTrade { get; set; }

        public TradingCondition(bool useTimePeriod, DateTime start, DateTime stop, DayOfWeek[] days, Func<bool> crossCondition, bool isLongTrade)
        {
            this.useTimePeriod = useTimePeriod;
            Start = start;
            Stop = stop;
            Days = days;
            CrossCondition = crossCondition;
            IsLongTrade = isLongTrade;
        }

        public bool isTimeConditionMet(DateTime currentTime)
        {
            return useTimePeriod && (currentTime.TimeOfDay >= Start.TimeOfDay && currentTime.TimeOfDay <= Stop.TimeOfDay);
        }
    }
}
