using AutoMapper;
using DataAccess.Entities;
using DataAccess.IdentityAccessor;
using Services.Members;
using System.Collections.Generic;

namespace Services.Common.AutoMapper.ApplicationUserResolver
{
    public class ApplicationUserRolesNameResolver : IValueResolver<ApplicationUser, MemberDTO, List<string>>
    {
        private readonly ApplicationUserManager _userManager;
        public ApplicationUserRolesNameResolver(ApplicationUserManager userManager)
        {
            _userManager = userManager;
        }

        public List<string> Resolve(ApplicationUser source, MemberDTO destination, List<string> destMember, ResolutionContext context)
        {
            return new List<string>(_userManager.GetRolesAsync(source.Id).Result);
        }
    }
}
