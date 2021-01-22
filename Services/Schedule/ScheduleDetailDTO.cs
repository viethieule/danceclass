using Services.Common.Enums;
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
        public TimesOfDay TimeOfDay
        {
            get
            {
                int hourStart = this.Schedule.StartTime.Hours;
                if (hourStart >= (int)TimesOfDay.Morning && hourStart < (int)TimesOfDay.Afternoon)
                {
                    return TimesOfDay.Morning;
                }
                else if (hourStart >= (int)TimesOfDay.Afternoon && hourStart < (int)TimesOfDay.Evening)
                {
                    return TimesOfDay.Afternoon;
                }
                else
                {
                    return TimesOfDay.Evening;
                }
            }
        }
    }
}