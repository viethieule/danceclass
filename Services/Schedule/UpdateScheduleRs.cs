using System.Collections.Generic;

namespace Services.Schedule
{
    public class UpdateScheduleRs
    {
        public ScheduleDTO Schedule { get; set; }
        public List<string> Messages { get; set; }
        public bool IsSelectedSessionDeleted { get; set; }
        public bool IsSelectedSessionUpdated { get; set; }
        public int UpdatedSessionId { get; set; }
    }
}