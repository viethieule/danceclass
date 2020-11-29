using System;

namespace Services.Report
{
    public class WeeklyScheduleReportRq
    {
        public DateTime Start { get; set; }
        public DateTime End {
            get
            {
                return this.Start.AddDays(7).AddMilliseconds(-1);
            }
        }
    }
}