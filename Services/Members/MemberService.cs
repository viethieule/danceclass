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
using System.Linq.Expressions;
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
        Task<GetAllMemberRs> GetAll(GetAllMemberRq rq);
        Task<EditMemberRs> Edit(EditMemberRq rq);
        Task<DeleteMemberRs> Delete(int userId);
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

                // When using user manager Http context is null so set current user here
                appUser.CreatedBy = HttpContext.Current.User.Identity.Name;

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
                    IsActive = true,
                    RegisteredBranchId = appUser.RegisteredBranchId
                };

                bool isPrivate = false;

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

                    isPrivate = defaultPackage.IsPrivate;
                    // Allow modify price of private package
                    package.Price = isPrivate ? rq.Package.Price : defaultPackage.Price;
                    package.RemainingSessions = defaultPackage.NumberOfSessions;
                    package.IsPrivate = isPrivate;
                }
                else
                {
                    package.NumberOfSessions = rq.Package.NumberOfSessions;
                    package.Months = rq.Package.Months;
                    package.Price = rq.Package.Price;
                    package.RemainingSessions = rq.Package.NumberOfSessions;
                }

                DateTime expiryDate = DateTime.Now.AddMonths(package.Months);
                package.ExpiryDate = expiryDate;

                _dbContext.Packages.Add(package);

                if (!isPrivate)
                {
                    _dbContext.Memberships.Add(new DataAccess.Entities.Membership
                    {
                        UserId = user.Id,
                        ExpiryDate = expiryDate,
                        RemainingSessions = package.NumberOfSessions
                    });
                }

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

            var query = _dbContext.Users.ProjectTo<MemberDTO>(_mappingConfig, dest => dest.Membership, dest => dest.RegisteredBranch);
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

            query = query.NormalizeVietnameseDiacritics();

            var results = await _dbContext.Users
                .Where(u => u.NormalizedFullName.Contains(query) || u.PhoneNumber.Contains(query) || u.UserName.Contains(query))
                .ProjectTo<MemberDTO>(_mappingConfig, u => u.Membership)
                .ToListAsync();

            return new SearchMemberRs
            {
                Members = results
            };
        }

        public async Task<GetAllMemberRs> GetAll(GetAllMemberRq rq)
        {
            var memberRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
            var query = _dbContext.Users
                .Where(u => u.Roles.Any(r => r.RoleId == memberRole.Id));

            if (!string.IsNullOrEmpty(rq.Name))
            {
                query = query.Where(u => u.FullName.Contains(rq.Name) || u.UserName.Contains(rq.Name));
            }

            if (!string.IsNullOrEmpty(rq.PhoneNumber))
            {
                query = query.Where(u => u.PhoneNumber.Contains(rq.PhoneNumber));
            }
            
            if (rq.DefaultPackageId.HasValue)
            {
                bool isOtherPackage = rq.DefaultPackageId == -1;
                if (isOtherPackage)
                {
                    query = query.Where(u => u.Packages.Any(p => !p.DefaultPackageId.HasValue));
                }
                else
                {
                    query = query.Where(u => u.Packages.Any(p => p.DefaultPackageId.HasValue && p.DefaultPackageId == rq.DefaultPackageId));
                }
            }

            if (rq.CreatedDateFrom.HasValue)
            {
                query = query.Where(u => u.CreatedDate >= rq.CreatedDateFrom);
            }

            if (rq.CreatedDateTo.HasValue)
            {
                DateTime createdDateTo = rq.CreatedDateTo.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(u => u.CreatedDate <= createdDateTo);
            }

            if (rq.ExpiryDateFrom.HasValue)
            {
                query = query.Where(u => u.Membership.ExpiryDate >= rq.ExpiryDateFrom);
            }

            if (rq.ExpiryDateTo.HasValue)
            {
                DateTime expDateTo = rq.ExpiryDateTo.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(u => u.Membership.ExpiryDate <= expDateTo);
            }

            var members = await query.ProjectTo<MemberDTO>(_mappingConfig, u => u.Membership)
                .ToListAsync();

            return new GetAllMemberRs
            {
                Members = members
            };
        }

        public async Task<EditMemberRs> Edit(EditMemberRq rq)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == rq.Id);
            if (user == null)
            {
                throw new Exception("User không tồn tại");
            }

            user.FullName = rq.FullName.Trim();
            user.UserName = rq.UserName.Trim();
            user.PhoneNumber = rq.PhoneNumber.Trim();
            user.Birthdate = rq.Birthdate;

            await _dbContext.SaveChangesAsync();

            return new EditMemberRs
            {
                Member = await _dbContext.Users.ProjectTo<MemberDTO>(_mappingConfig, m => m.RegisteredBranch).FirstOrDefaultAsync(u => u.Id == user.Id)
            };
        }

        public async Task<DeleteMemberRs> Delete(int userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User không tồn tại");
            }

            if (await _userManager.IsInRoleAsync(userId, "Admin"))
            {
                throw new Exception("Không thể xóa admin");
            }

            _dbContext.Memberships.Remove(user.Membership);
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return new DeleteMemberRs
            {
                Success = true
            };
        }
    }
}
