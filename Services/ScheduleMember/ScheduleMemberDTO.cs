using Services.Members;
using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ScheduleMember
{
    public class ScheduleMemberDTO
    {
        public int ScheduleId { get; set; }
        public virtual ScheduleDTO Schedule { get; set; }
        public int UserId { get; set; }
        public virtual MemberDTO User { get; set; }
        public DateTime DateRegistered { get; set; }
        public int RemainingSessions { get; set; }
    }
}
