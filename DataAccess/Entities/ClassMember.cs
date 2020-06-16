using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class ClassMember
    {
        [Key, Column(Order = 1)]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        [Key, Column(Order = 2)]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public DateTime DateRegister { get; set; }
        public int DaysRegistered { get; set; }
    }
}
