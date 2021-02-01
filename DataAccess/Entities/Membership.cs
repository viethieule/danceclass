using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DataAccess.Interfaces;

namespace DataAccess.Entities
{
    public class Membership : EntityBase, IFieldChangeLog
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string LatestAction { get; set; }
    }
}
