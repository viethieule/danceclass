using Services.Membership;
using Services.Package;
using System;

namespace Services.Package
{
    public class EditPackageRq
    {
        public int PackageId { get; set; }
        public int? DefaultPackageId { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
    }
}