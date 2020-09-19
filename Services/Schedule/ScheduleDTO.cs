using DataAccess.Entities;
using Services.Class;
using Services.Registration;
using Services.Trainer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Schedule
{
    public class ScheduleDTO
    {
        public int Id { get; set; }

        public string Song { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? EndingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? Sessions { get; set; }
        public int? SessionsPerWeek { get; set; }
        public string DaysPerWeek { get; set; }
        public string DaysPerWeekString
        {
            get
            {
                var days = DaysPerWeek.Select(d =>
                {
                    int day = int.Parse(d.ToString());
                    if (day == (int)DayOfWeek.Sunday)
                    {
                        return "Chủ nhật";
                    }

                    return "Thứ " + (day + 1).ToString();
                });

                return string.Join(", ", days);
            }
        }
        public string Branch { get; set; }
        
        public int? ClassId { get; set; }
        public ClassDTO Class { get; set; }
        public string ClassName { get; set; }

        public int? TrainerId { get; set; }
        public TrainerDTO Trainer { get; set; }
        public string TrainerName { get; set; }

        public List<ScheduleDetailDTO> ScheduleDetails { get; set; }
    }
}