using DataAccess.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class Package : EntityBase, IFieldChangeLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public int? DefaultPackageId { get; set; }

        [ForeignKey("DefaultPackageId")]
        public virtual DefaultPackage DefaultPackage { get; set; }

        public int? RegisteredBranchId { get; set; }
        [ForeignKey("RegisteredBranchId")]
        public virtual Branch RegisteredBranch { get; set; }

        public bool IsActive { get; set; }
        public string LatestAction { get; set; }
    }
}
