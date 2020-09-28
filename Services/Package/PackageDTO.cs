using Services.Members;
using Services.DefaultPackage;
using System;

namespace Services.Package
{
    public class PackageDTO
    {
        public int? Id { get; set; }

        public int? UserId { get; set; }
        public MemberDTO User { get; set; }

        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public int? DefaultPackageId { get; set; }

        public DefaultPackageDTO DefaultPackage { get; set; }

        public bool IsActive { get; set; }
    }
}
