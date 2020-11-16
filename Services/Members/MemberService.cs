using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using DataAccess.Entities;
using DataAccess.IdentityAccessor;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.Common;
using Services.Members;
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
        Task<GetMemberRs> Get(GetMemberRq rq);
        Task<GetMemberRs> GetCurrentUser();
        bool IsNeedToChangePassword(int userId);
        Task<SearchMemberRs> Search(SearchMemberRq rq);
    }

    public class MemberService : UserService, IMemberService
    {
        public MemberService(
            ApplicationUserManager userManager,
            DanceClassDbContext dbContext,
            IMapper mapper,
            IConfigurationProvider mappingConfig) : base(userManager, dbContext, mapper, mappingConfig)
        {
        }

        public async Task<CreateMemberRs> Create(CreateMemberRq rq)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                MemberDTO member = rq.Member;
                member.UserName = await GenerateUserName(member.FullName);

                ApplicationUser appUser = _mapper.Map<ApplicationUser>(member);
                appUser.IsNeedToChangePassword = true;

                var result = await _userManager.CreateAsync(appUser, DEFAULT_PASSWORD);
                if (!result.Succeeded)
                {
                    throw new Exception("Cannot create new member. " + string.Join(" ", result.Errors));
                }

                var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.UserName == member.UserName);
                await _userManager.AddToRoleAsync(user.Id, "Member");

                var package = new DataAccess.Entities.Package
                {
                    UserId = user.Id,
                    IsActive = true
                };

                if (rq.Package.DefaultPackageId.HasValue)
                {
                    var defaultPackageId = rq.Package.DefaultPackageId.Value;
                    var defaultPackage = await _dbContext.DefaultPackages.FirstOrDefaultAsync(p => p.Id == defaultPackageId);
                    if (defaultPackage == null)
                    {
                        throw new Exception("Gói tập đang chọn không tồn tại");
                    }
                    package.DefaultPackageId = defaultPackageId;
                    package.NumberOfSessions = defaultPackage.NumberOfSessions;
                    package.Months = defaultPackage.Months;
                    package.Price = defaultPackage.Price;
                    package.RemainingSessions = defaultPackage.NumberOfSessions;
                }
                else
                {
                    package.NumberOfSessions = rq.Package.NumberOfSessions;
                    package.Months = rq.Package.Months;
                    package.Price = rq.Package.Price;
                    package.RemainingSessions = rq.Package.NumberOfSessions;
                }

                _dbContext.Packages.Add(package);
                _dbContext.Memberships.Add(new DataAccess.Entities.Membership
                {
                    UserId = user.Id,
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

        public async Task<GetMemberRs> Get(GetMemberRq rq)
        {
            bool isMember = HttpContext.Current.User.IsInRole("Member");
            string currentUserName = HttpContext.Current.User.Identity.GetUserName();
            if (isMember && currentUserName != rq.UserName)
            {
                throw new Exception("Not found");
            }

            GetMemberRs rs = new GetMemberRs();

            var query = _dbContext.Users.ProjectTo<MemberDTO>(_mappingConfig, dest => dest.Membership);
            if (!string.IsNullOrEmpty(rq.UserName))
            {
                rs.Member = await query.FirstOrDefaultAsync(u => u.UserName == rq.UserName);
            }
            else if (!string.IsNullOrEmpty(rq.PhoneNumber))
            {
                rs.Member = await query.FirstOrDefaultAsync(u => u.PhoneNumber == rq.PhoneNumber);
            }

            return rs;
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
                    .ProjectTo<MemberDTO>(_mappingConfig, dest => dest.Membership)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.RoleNames = new List<string>(await _userManager.GetRolesAsync(userId));
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

        public bool IsNeedToChangePassword(int userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user != null && user.IsNeedToChangePassword;
        }

        public async Task<SearchMemberRs> Search(SearchMemberRq rq)
        {
            string query = rq.Query;
            if (string.IsNullOrEmpty(query) || query.Length < 2 || (query.Length < 5 && query.All(char.IsDigit)))
            {
                return new SearchMemberRs
                {
                    Members = new List<MemberDTO>()
                };
            }

            var results = await _dbContext.Users
                .Where(u => u.FullName.Contains(query) || u.PhoneNumber.Contains(query) || u.UserName.Contains(query))
                .ProjectTo<MemberDTO>(_mappingConfig, u => u.Membership)
                .ToListAsync();

            return new SearchMemberRs
            {
                Members = results
            };
        }
    }
}
