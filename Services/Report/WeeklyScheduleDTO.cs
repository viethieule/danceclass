using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Report
{
    public class WeeklyScheduleDTO
    {
        public TimeSpan TimeStart { get; set; }
        public TimeSpan TimeEnd
        {
            get
            {
                return TimeStart.Add(new TimeSpan(1, 0, 0));
            }
        }
        public List<ScheduleDetailDTO> Sessions { get; set; }
    }
}
