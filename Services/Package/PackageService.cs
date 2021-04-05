using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.AspNet.Identity;
using Services.Common;
using Services.Membership;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.Package
{
    public interface IPackageService
    {
        Task<CreatePackageRs> AddForMember(CreatePackageRq rq);
        Task<EditPackageRs> Edit(EditPackageRq rq);
        Task<EditPackageRs> EditPrivate(EditPackageRq rq);
        Task<GetPackagesRs> GetByUserId(GetPackagesRq rq);
    }

    public class PackageService : BaseService, IPackageService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public PackageService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<CreatePackageRs> AddForMember(CreatePackageRq rq)
        {
            if (!(await _dbContext.Users.AnyAsync(x => x.Id == rq.UserId)))
            {
                throw new Exception("Học viên không tồn tại");
            }

            var package = new DataAccess.Entities.Package
            {
                UserId = rq.UserId,
                RegisteredBranchId = rq.RegisteredBranchId
            };

            bool isPrivatePackage = false;
            if (rq.DefaultPackageId.HasValue)
            {
                int defaultPackageId = rq.DefaultPackageId.Value;
                var defaultPackage = await _dbContext.DefaultPackages.FirstOrDefaultAsync(p => p.Id == defaultPackageId);
                package.DefaultPackageId = defaultPackageId;

                package.DefaultPackage = defaultPackage ?? throw new Exception("Gói tập đang chọn không tồn tại");
                package.RemainingSessions = defaultPackage.NumberOfSessions;
                package.NumberOfSessions = defaultPackage.NumberOfSessions;
                package.Price = defaultPackage.IsPrivate ? rq.Price : defaultPackage.Price;
                package.Months = defaultPackage.Months;

                isPrivatePackage = defaultPackage.IsPrivate;
            }
            else
            {
                package.RemainingSessions = rq.NumberOfSessions;
                package.NumberOfSessions = rq.NumberOfSessions;
                package.Price = rq.Price;
                package.Months = rq.Months;
            }

            CreatePackageRs rs = new CreatePackageRs();
            if (!isPrivatePackage)
            {
                var membership = await _dbContext.Memberships.SingleAsync(x => x.UserId == rq.UserId);
                var addedMonths = package.Months;
                if (membership.ExpiryDate >= DateTime.Now.Date && membership.RemainingSessions > 0)
                {
                    var expiryDate = membership.ExpiryDate.AddMonths(addedMonths);
                    membership.ExpiryDate = expiryDate;
                    package.ExpiryDate = expiryDate;
                }
                else
                {
                    package.IsActive = true;
                    var currentPackage = await _dbContext.Packages.FirstOrDefaultAsync(m => m.UserId == rq.UserId && m.IsActive);
                    if (currentPackage != null)
                    {
                        currentPackage.IsActive = false;
                    }

                    // Since it is expired, reset remaining session to 0
                    membership.RemainingSessions = 0;

                    var expiryDate = DateTime.Now.AddMonths(addedMonths);
                    membership.ExpiryDate = expiryDate;
                    package.ExpiryDate = expiryDate;
                }

                membership.RemainingSessions += package.NumberOfSessions;

                LogLatestAction(new List<IFieldChangeLog> { membership });

                _dbContext.Packages.Add(package);
                await _dbContext.SaveChangesAsync();
                _dbContext.Entry(membership).State = EntityState.Detached;

                rs.Membership = _mapper.Map<MembershipDTO>(membership);
            }
            else
            {
                _dbContext.Packages.Add(package);
                await _dbContext.SaveChangesAsync();
                _dbContext.Entry(package).State = EntityState.Detached;

                rs.PrivatePackage = _mapper.Map<PackageDTO>(package);
            }
            
            return rs;
        }

        public async Task<EditPackageRs> Edit(EditPackageRq rq)
        {
            var package = await _dbContext.Packages.FirstOrDefaultAsync(x => x.Id == rq.PackageId);
            if (package == null)
            {
                throw new Exception("Gói tập của hội viên bạn muốn cập nhật không tồn tại");
            }

            var membership = await _dbContext.Memberships.FirstOrDefaultAsync(x => x.UserId == package.UserId);
            if (membership == null)
            {
                throw new Exception("Membership của hội viên bạn muốn cập nhật không tồn tại");
            }

            if (rq.DefaultPackageId.HasValue && rq.DefaultPackageId.Value != package.DefaultPackageId)
            {
                int defaultPackageId = rq.DefaultPackageId.Value;
                var defaultPackage = await _dbContext.DefaultPackages.FirstOrDefaultAsync(p => p.Id == defaultPackageId);
                if (defaultPackage == null)
                {
                    throw new Exception("Gói tập mặc định bạn muốn cập nhật không tồn tại");
                }

                int deltaNumberSession = defaultPackage.NumberOfSessions - package.NumberOfSessions;
                int deltaMonths = defaultPackage.Months - package.Months;

                package.DefaultPackageId = defaultPackageId;
                package.NumberOfSessions = defaultPackage.NumberOfSessions;
                package.Price = defaultPackage.Price;
                package.Months = defaultPackage.Months;
                package.RemainingSessions += deltaNumberSession;
                package.ExpiryDate = package.ExpiryDate.HasValue ? (DateTime?)package.ExpiryDate.Value.AddMonths(deltaMonths) : null;

                membership.RemainingSessions += deltaNumberSession;
                membership.ExpiryDate = membership.ExpiryDate.AddMonths(deltaMonths);
            }
            else
            {
                int deltaNumberSession = rq.NumberOfSessions - package.NumberOfSessions;
                int deltaMonths = rq.Months - package.Months;

                package.DefaultPackageId = null;
                package.NumberOfSessions = rq.NumberOfSessions;
                package.Price = rq.Price;
                package.Months = rq.Months;
                package.RemainingSessions += deltaNumberSession;
                package.ExpiryDate = package.ExpiryDate.HasValue ? (DateTime?)package.ExpiryDate.Value.AddMonths(deltaMonths) : null;

                membership.RemainingSessions += deltaNumberSession;
                membership.ExpiryDate = membership.ExpiryDate.AddMonths(deltaMonths);
            }

            LogLatestAction(new List<IFieldChangeLog> { package, membership });

            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(package).State = EntityState.Detached;
            _dbContext.Entry(membership).State = EntityState.Detached;

            var rs = new EditPackageRs
            {
                Membership = _mapper.Map<MembershipDTO>(membership),
                Package = _mapper.Map<PackageDTO>(package)
            };
            return rs;
        }

        public async Task<EditPackageRs> EditPrivate(EditPackageRq rq)
        {
            DataAccess.Entities.Package privatePackage = await _dbContext.Packages.FirstOrDefaultAsync(p => p.Id == rq.PackageId);
            if (privatePackage == null)
            {
                throw new Exception("Gói tập không tồn tại");
            }

            privatePackage.NumberOfSessions = rq.NumberOfSessions;
            privatePackage.Price = rq.Price;
            privatePackage.Months = rq.Months;

            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(privatePackage).State = EntityState.Detached;

            return new EditPackageRs
            {
                Package = _mapper.Map<PackageDTO>(privatePackage)
            };
        }

        public async Task<GetPackagesRs> GetByUserId(GetPackagesRq rq)
        {
            if (!(await _dbContext.Users.AnyAsync(u => u.Id == rq.UserId)))
            {
                throw new Exception("Hội viên không tồn tại");
            }

            bool isMember = HttpContext.Current.User.IsInRole("Member");
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            if (isMember && rq.UserId.ToString() != currentUserId)
            {
                throw new Exception("Không đủ quyền");
            }

            var packages = await _dbContext.Packages.Where(p => p.UserId == rq.UserId).OrderByDescending(p => p.Id).ProjectTo<PackageDTO>(_mappingConfig).ToListAsync();

            return new GetPackagesRs { Packages = packages };
        }
    }
}
