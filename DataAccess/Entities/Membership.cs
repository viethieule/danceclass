using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Membership
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
