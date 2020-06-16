﻿using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
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
                    member.DefaultPassword = DEFAULT_PASSWORD;
                    member.Username = appUser.Email;
                    rs.Member = member;
                    return rs;
                }

                throw new Exception("Cannot create new member");
            }
        }

        public ApplicationUser MapFromDTO(MemberDTO member)
        {
            ApplicationUser appUser = new ApplicationUser();
            appUser.FirstName = member.FirstName;
            appUser.LastName = member.LastName;
            appUser.Email = member.Email;
            appUser.UserName = member.Email;
            appUser.Birthdate = member.Birthdate;
            appUser.PackageId = member.PackageId;

            return appUser;
        }
    }
}
