using AutoMapper;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MemberPackage
{
    public interface IMemberPackageService
    {
        Task<CreateMemberPackageRs> AddForMember(CreateMemberPackageRq rq);
        Task<EditMemberPackageRs> Edit(EditMemberPackageRq rq);
    }

    public class MemberPackageService : BaseService, IMemberPackageService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public MemberPackageService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<CreateMemberPackageRs> AddForMember(CreateMemberPackageRq rq)
        {
            if (!(await _dbContext.Users.AnyAsync(x => x.Id == rq.UserId)))
            {
                throw new Exception("Học viên không tồn tại");
            }

            var memberPackage = new DataAccess.Entities.MemberPackage
            {
                UserId = rq.UserId
            };

            DataAccess.Entities.Package package = null;
            if (rq.Package.Id.HasValue)
            {
                int packageId = rq.Package.Id.Value;
                package = await _dbContext.Packages.FirstOrDefaultAsync(p => p.Id == packageId);
                if (package == null)
                {
                    throw new Exception("Gói tập đang chọn không tồn tại");
                }

                memberPackage.PackageId = packageId;
                memberPackage.Package = package;
                memberPackage.RemainingSessions = package.NumberOfSessions;
            }
            else
            {
                memberPackage.Package = _mapper.Map<DataAccess.Entities.Package>(rq.Package);
                memberPackage.RemainingSessions = memberPackage.Package.NumberOfSessions;
            }

            var membership = await _dbContext.Memberships.SingleAsync(x => x.UserId == rq.UserId);
            var addedMonths = memberPackage.Package.Months;
            if (membership.ExpiryDate > DateTime.Now && membership.RemainingSessions > 0)
            {
                membership.ExpiryDate = membership.ExpiryDate.AddMonths(addedMonths);
            }
            else
            {
                memberPackage.IsActive = true;
                var currentMemberPackage = await _dbContext.MemberPackages.FirstOrDefaultAsync(m => m.UserId == rq.UserId && m.IsActive);
                if (currentMemberPackage == null)
                {
                    currentMemberPackage.IsActive = false;
                }

                membership.ExpiryDate = DateTime.Now.AddMonths(addedMonths);
            }

            membership.RemainingSessions += memberPackage.Package.NumberOfSessions;

            _dbContext.MemberPackages.Add(memberPackage);
            await _dbContext.SaveChangesAsync();

            var rs = new CreateMemberPackageRs();
            return rs;
        }

        public async Task<EditMemberPackageRs> Edit(EditMemberPackageRq rq)
        {
            var rs = new EditMemberPackageRs();
            return rs;
        }
    }
}
