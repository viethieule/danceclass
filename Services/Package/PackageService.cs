using AutoMapper;
using DataAccess;
using Services.Common;
using Services.Membership;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Package
{
    public interface IPackageService
    {
        Task<CreatePackageRs> AddForMember(CreatePackageRq rq);
        Task<EditPackageRs> Edit(EditPackageRq rq);
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
                UserId = rq.UserId
            };

            if (rq.DefaultPackageId.HasValue)
            {
                int defaultPackageId = rq.DefaultPackageId.Value;
                var defaultPackage = await _dbContext.DefaultPackages.FirstOrDefaultAsync(p => p.Id == defaultPackageId);
                package.DefaultPackageId = defaultPackageId;

                package.DefaultPackage = defaultPackage ?? throw new Exception("Gói tập đang chọn không tồn tại");
                package.RemainingSessions = defaultPackage.NumberOfSessions;
                package.NumberOfSessions = defaultPackage.NumberOfSessions;
                package.Price = defaultPackage.Price;
                package.Months = defaultPackage.Months;
            }
            else
            {
                package.NumberOfSessions = rq.NumberOfSessions;
                package.Price = rq.Price;
                package.Months = rq.Months;
            }

            var membership = await _dbContext.Memberships.SingleAsync(x => x.UserId == rq.UserId);
            var addedMonths = package.Months;
            if (membership.ExpiryDate > DateTime.Now && membership.RemainingSessions > 0)
            {
                membership.ExpiryDate = membership.ExpiryDate.AddMonths(addedMonths);
            }
            else
            {
                package.IsActive = true;
                var currentPackage = await _dbContext.Packages.FirstOrDefaultAsync(m => m.UserId == rq.UserId && m.IsActive);
                if (currentPackage == null)
                {
                    currentPackage.IsActive = false;
                }

                membership.ExpiryDate = DateTime.Now.AddMonths(addedMonths);
            }

            membership.RemainingSessions += package.NumberOfSessions;

            _dbContext.Packages.Add(package);
            await _dbContext.SaveChangesAsync();

            var rs = new CreatePackageRs();
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

                membership.RemainingSessions += deltaNumberSession;
                membership.ExpiryDate = membership.ExpiryDate.AddMonths(deltaMonths);
            }

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
    }
}
