using Services.Package;

namespace Services.MemberPackage
{
    public class CreateMemberPackageRq
    {
        public int UserId { get; set; }
        public PackageDTO Package { get; set; }
    }
}