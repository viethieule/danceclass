using System.Collections.Generic;

namespace Services.Schedule
{
    public class UpdateScheduleRs
    {
        public ScheduleDTO Schedule { get; set; }
        public List<string> Messages { get; set; }
    }
}