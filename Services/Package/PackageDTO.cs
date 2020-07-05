using Services.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Package
{
    public class PackageDTO
    {
        public int? Id { get; set; }
        public int NumberOfSessions { get; set; }
        public double Price { get; set; }
        public int Months { get; set; }
        public bool IsDefault { get; set; }
    }
}
