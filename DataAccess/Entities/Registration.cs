using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DataAccess.Enums;

namespace DataAccess.Entities
{
    public class Registration : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index]
        public int ScheduleDetailId { get; set; }
        [ForeignKey("ScheduleDetailId")]
        public ScheduleDetail ScheduleDetail { get; set; }

        [Index]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime DateRegistered { get; set; }
        public RegistrationStatus Status { get; set; }
    }
}
