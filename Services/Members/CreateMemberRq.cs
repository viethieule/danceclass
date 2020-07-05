using Services.Package;
using System;

namespace Services.Members
{
    public class CreateMemberRq
    {
        public MemberDTO Member { get; set; }
        public PackageDTO Package { get; set; }
    }
}