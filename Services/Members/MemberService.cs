using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.Common;
using Services.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace Services.Members
{
    public interface IMemberService
    {
        Task<CreateMemberRs> Create(CreateMemberRq rq);
    }

    public class MemberService : IMemberService
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private const string DEFAULT_PASSWORD = "P@ssw0rd";

        public MemberService(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<CreateMemberRs> Create(CreateMemberRq rq)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (DanceClassDbContext dbContext = new DanceClassDbContext())
            {
                MemberDTO member = rq.Member;
                member.UserName = await GenerateUserName(member.FullName, dbContext);

                ApplicationUser appUser = MappingConfig.Mapper.Map<ApplicationUser>(member);
                var result = await _userManager.CreateAsync(appUser, DEFAULT_PASSWORD);
                if (!result.Succeeded)
                {
                    throw new Exception("Cannot create new member. " + string.Join(" ", result.Errors));
                }

                DataAccess.Entities.Package package = MappingConfig.Mapper.Map<DataAccess.Entities.Package>(rq.Package);
                if (rq.Package.Id != null)
                {
                    if (!dbContext.Packages.Any(p => p.Id == rq.Package.Id && p.IsDefault))
                    {
                        throw new Exception("Selected package not exist");
                    }
                }
                else
                {
                    dbContext.Packages.Add(package);
                    await dbContext.SaveChangesAsync();
                }

                ApplicationUser user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == member.UserName);
                dbContext.MemberPackages.Add(new MemberPackage
                {
                    UserId = user.Id,
                    PackageId = package.Id,
                    ExpiryDate = DateTime.Now.AddMonths(package.Months),
                    RemainingSessions = package.NumberOfSessions
                });

                await dbContext.SaveChangesAsync();
                scope.Complete();

                CreateMemberRs rs = new CreateMemberRs();
                member.Id = user.Id;
                rs.Member = member;
                return rs;
            }
        }

        private async Task<string> GenerateUserName(string fullName, DanceClassDbContext dbContext)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            fullName = StringHelper.ConverToUnsignedString(fullName.Trim());
            string[] names = Regex.Split(fullName.ToLower(), @"\s+");

            if (names.Length == 1)
            {
                string userName = names[0];

                int numberOfExistingUsernames = await dbContext.Users.CountAsync(u => u.UserName.StartsWith(userName) && u.UserName.Length == userName.Length);
                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
            else
            {
                string userName = names[names.Length - 1] + "." + names[0];

                int numberOfExistingUsernames = await dbContext.Users.CountAsync(u => u.UserName.StartsWith(userName));
                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
        }
    }
}
