using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using DataAccess.Entities;
using DataAccess.IdentityAccessor;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.Common;
using Services.Members.Get;
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
        Task<GetMemberRs> GetById(GetMemberRq rq);
        Task<GetMemberRs> GetCurrentUser();
    }

    public class MemberService : BaseService, IMemberService
    {
        private ApplicationUserManager _userManager;
        private readonly IConfigurationProvider _mappingConfig;
        private const string DEFAULT_PASSWORD = "P@ssw0rd";

        public MemberService(
            ApplicationUserManager userManager,
            DanceClassDbContext dbContext,
            IMapper mapper,
            IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _userManager = userManager;
            _mappingConfig = mappingConfig;
        }

        public async Task<CreateMemberRs> Create(CreateMemberRq rq)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                MemberDTO member = rq.Member;
                member.UserName = await GenerateUserName(member.FullName, _dbContext);

                ApplicationUser appUser = _mapper.Map<ApplicationUser>(member);
                var result = await _userManager.CreateAsync(appUser, DEFAULT_PASSWORD);
                if (!result.Succeeded)
                {
                    throw new Exception("Cannot create new member. " + string.Join(" ", result.Errors));
                }

                DataAccess.Entities.Package package = _mapper.Map<DataAccess.Entities.Package>(rq.Package);
                if (rq.Package.Id != null)
                {
                    if (!_dbContext.Packages.Any(p => p.Id == rq.Package.Id && p.IsDefault))
                    {
                        throw new Exception("Selected package not exist");
                    }
                }
                else
                {
                    _dbContext.Packages.Add(package);
                    await _dbContext.SaveChangesAsync();
                }

                ApplicationUser user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == member.UserName);
                _dbContext.MemberPackages.Add(new DataAccess.Entities.MemberPackage
                {
                    UserId = user.Id,
                    PackageId = package.Id,
                    ExpiryDate = DateTime.Now.AddMonths(package.Months),
                    RemainingSessions = package.NumberOfSessions
                });

                await _dbContext.SaveChangesAsync();
                scope.Complete();

                CreateMemberRs rs = new CreateMemberRs();
                member.Id = user.Id;
                rs.Member = member;
                return rs;
            }
        }

        public async Task<GetMemberRs> GetById(GetMemberRq rq)
        {
            var member = await _dbContext.Users.ProjectTo<MemberDTO>(_mappingConfig).FirstOrDefaultAsync(u => u.UserName == rq.UserName);

            GetMemberRs rs = new GetMemberRs();
            rs.Member = member;
            return rs;
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

        public async Task<GetMemberRs> GetCurrentUser()
        {
            GetMemberRs rs = new GetMemberRs();
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                rs.IsAuthenticated = false;
                rs.Member = null;
            }
            else
            {
                int userId = int.Parse(HttpContext.Current.User.Identity.GetUserId());

                var user = await _dbContext.Users
                    .Where(x => x.Id == userId)
                    .ProjectTo<MemberDTO>(_mappingConfig, dest => dest.MemberPackages)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.RoleNames = new List<string>(await _userManager.GetRolesAsync(userId));
                    //user.ActivePackage = user.MemberPackages.FirstOrDefault(x => x.IsActive);
                    rs.Member = user;

                    rs.IsAuthenticated = true;
                }
                else
                {
                    rs.IsAuthenticated = false;
                }
            }

            return rs;
        }
    }
}
