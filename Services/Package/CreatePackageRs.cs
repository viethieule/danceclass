using Services.Membership;

namespace Services.Package
{
    public class CreatePackageRs
    {
        public MembershipDTO Membership { get; set; }
        public PackageDTO PrivatePackage { get; internal set; }
    }
}