using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DataAccess.Entities
{
    public class ScheduleDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public Schedule Schedule { get; set; }

        public ICollection<Registration> Registrations { get; set; }

        public int SessionNo { get; set; }
    }
}
