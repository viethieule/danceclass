using Services.Members;
using Services.Schedule;
using System;

namespace Services.Registration
{
    public class RegistrationDTO
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public virtual ScheduleDTO Schedule { get; set; }
        public int UserId { get; set; }
        public virtual MemberDTO User { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime DateAttending { get; set; }
    }
}
