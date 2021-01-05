using DataAccess.Interfaces;
using Services.Branch;
using Services.Membership;
using Services.Package;
using System;
using System.Collections.Generic;

namespace Services.Members
{
    public class MemberDTO : IAuditable
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthdate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ICollection<UserRoleDTO> Roles { get; set; }
        public List<string> RoleNames { get; set; }
        public int? RegisteredBranchId { get; set; }
        public virtual BranchDTO RegisteredBranch { get; set; }
        public List<PackageDTO> Packages { get; set; }
        public PackageDTO ActivePackage { get; set; }
        public MembershipDTO Membership { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
