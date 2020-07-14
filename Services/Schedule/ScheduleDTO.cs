using DataAccess.Entities;
using Services.Class;
using Services.Registration;
using Services.Trainer;
using System;
using System.Collections.Generic;

namespace Services.Schedule
{
    public class ScheduleDTO
    {
        public int Id { get; set; }

        public string Song { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int Sessions { get; set; }
        public int SessionsPerWeek { get; set; }
        public string DaysPerWeek { get; set; }
        public string Branch { get; set; }
        
        public int ClassId { get; set; }
        public virtual ClassDTO Class { get; set; }

        public int TrainerId { get; set; }
        public virtual TrainerDTO Trainer { get; set; }

        public virtual IEnumerable<RegistrationDTO> Registrations { get; set; }
    }
}