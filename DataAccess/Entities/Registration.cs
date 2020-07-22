using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Registration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ScheduleDetailId { get; set; }
        [ForeignKey("ScheduleDetailId")]
        public ScheduleDetail ScheduleDetail { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime DateRegistered { get; set; }
    }
}
