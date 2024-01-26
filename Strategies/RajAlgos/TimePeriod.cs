#region Using declarations
using System;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.RajAlgos
{
    public class TimePeriod
    {
        private bool useTimePeriod;
        private DateTime startTime;
        private DateTime stopTime;

        public TimePeriod(bool useTimePeriod, DateTime startTime, DateTime stopTime)
        {
            this.useTimePeriod = useTimePeriod;
            this.startTime = startTime;
            this.stopTime = stopTime;
        }

        public bool isTimeConditionMet(DateTime currentTime)
        {
            return useTimePeriod && (currentTime.TimeOfDay >= startTime.TimeOfDay && currentTime.TimeOfDay <= stopTime.TimeOfDay);
        }
    }
}
