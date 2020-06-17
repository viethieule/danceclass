using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class ScheduleMember
    {
        [Key, Column(Order = 1)]
        public int ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; }

        [Key, Column(Order = 2)]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime DateRegistered { get; set; }
        public int RemainingSessions { get; set; }
    }
}
