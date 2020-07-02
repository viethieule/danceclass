using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.Members
{
    public class MemberService
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private const string DEFAULT_PASSWORD = "P@ssw0rd";

        public MemberService(ApplicationUserManager userManager , ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<CreateMemberRs> Create(CreateMemberRq rq)
        {
            using (DanceClassDbContext dbContext = new DanceClassDbContext())
            {
                MemberDTO member = rq.Member;
                ApplicationUser appUser = MapFromDTO(rq.Member);
                var result = await _userManager.CreateAsync(appUser, DEFAULT_PASSWORD);
                if (result.Succeeded)
                {
                    CreateMemberRs rs = new CreateMemberRs();
                    ApplicationUser user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == member.Email);
                    member.Username = appUser.Email;
                    member.Id = user.Id;
                    rs.Member = member;
                    return rs;
                }

                throw new Exception("Cannot create new member");
            }
        }

        public ApplicationUser MapFromDTO(MemberDTO member)
        {
            ApplicationUser appUser = new ApplicationUser
            {
                FullName = member.FullName,
                Email = member.Email,
                UserName = member.Email,
                Birthdate = member.Birthdate,
                PackageId = member.PackageId
            };

            return appUser;
        }
    }
}
