using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class MemberPackage
    {
        [Key, Column(Order = 1)]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Key, Column(Order = 2)]
        public int PackageId { get; set; }
        
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
