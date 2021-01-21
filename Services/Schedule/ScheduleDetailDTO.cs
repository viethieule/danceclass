using Services.Registration;
using System;
using System.Collections.Generic;

namespace Services.Schedule
{
    public class ScheduleDetailDTO
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int ScheduleId { get; set; }
        public ScheduleDTO Schedule { get; set; }

        public List<RegistrationDTO> Registrations { get; set; }
        public int TotalRegistered { get; set; }
        public int SessionNo { get; set; }
        public bool IsCurrentUserRegistered { get; set; }
        public RegistrationDTO CurrentUserRegistration { get; set; }
    }
}