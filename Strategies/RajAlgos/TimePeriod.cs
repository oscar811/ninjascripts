#region Using declarations
using System;
using System.Linq;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class TimePeriod
    {
        public bool Enabled { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DayOfWeek[] Days { get; set; }
        public bool HasTraded { get; set; }

        public TimePeriod(bool enabled, DateTime startTime, DateTime stopTime, DayOfWeek[] days)
        {
            Enabled = enabled;
            StartTime = startTime;
            StopTime = stopTime;
            Days = days;
        }

        public bool isTimeConditionMet(DateTime currentTime, DayOfWeek currentDay)
        {
            return Enabled
                && (currentTime.TimeOfDay >= StartTime.TimeOfDay && currentTime.TimeOfDay <= StopTime.TimeOfDay)
                && (Days == null || Days.Contains(currentDay));
        }
    }
}
