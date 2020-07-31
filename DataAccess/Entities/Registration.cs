using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Entities
{
    public class Registration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ScheduleDetailId { get; set; }
        [ForeignKey("ScheduleDetailId")]
        public virtual ScheduleDetail ScheduleDetail { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime DateRegistered { get; set; }
        public RegistrationStatus Status { get; set; }
    }
}
