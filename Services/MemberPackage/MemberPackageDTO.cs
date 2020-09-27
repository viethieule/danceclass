using Services.Members;
using Services.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MemberPackage
{
    public class MemberPackageDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public MemberDTO User { get; set; }

        public int? PackageId { get; set; }

        public PackageDTO Package { get; set; }

        public int RemainingSessions { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; }
    }
}
