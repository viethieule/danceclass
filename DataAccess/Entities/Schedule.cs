using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class Schedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        public int TrainerId { get; set; }
        [ForeignKey("TrainerId")]
        public virtual Trainer Trainer { get; set; }

        public virtual IEnumerable<ScheduleDetail> ScheduleDetails { get; set; }
    }
}
