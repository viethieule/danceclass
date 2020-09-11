using System.Collections.Generic;

namespace Services.Schedule
{
    public class UpdateScheduleRs
    {
        public UpdateScheduleRs()
        {
            Messages = new List<string>();
        }

        public ScheduleDTO Schedule { get; set; }
        public List<string> Messages { get; set; }
        public bool IsSelectedSessionDeleted { get; set; }
        public bool IsSelectedSessionUpdated { get; set; }
        public int? UpdatedSessionId { get; set; }
        public int? SelectedSessionNumber { get; set; }
    }
}