using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Class
{
    public class ClassDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<ScheduleDTO> Schedules { get; set; }
    }
}
