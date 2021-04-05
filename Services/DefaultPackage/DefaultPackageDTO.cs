using Services.Package;
using System.Collections.Generic;

namespace Services.DefaultPackage
{
    public class DefaultPackageDTO
    {
        public int? Id { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
        public bool IsPrivate { get; set; }
        public List<PackageDTO> Packages { get; set; }
    }
}
