using AutoMapper;
using DataAccess;
using DataAccess.Interfaces;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.Membership
{
    public interface IMembershipService
    {
        Task<UpdateMembershipRs> Update(UpdateMembershipRq rq);
    }

    public class MembershipService : BaseService, IMembershipService
    {
        public MembershipService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<UpdateMembershipRs> Update(UpdateMembershipRq rq)
        {
            var membership = await _dbContext.Memberships.FirstOrDefaultAsync(x => x.UserId == rq.UserId);
            if (membership == null)
            {
                throw new Exception("Membership của hội viên không tồn tại");
            }

            if (rq.ExpiryDate.HasValue)
            {
                membership.ExpiryDate = rq.ExpiryDate.Value;
            }

            if (rq.RemainingSessions.HasValue)
            {
                membership.RemainingSessions = rq.RemainingSessions.Value;
            }

            var activePackage = await _dbContext.Packages.OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(x => x.UserId == membership.UserId && x.IsActive);
            if (activePackage != null)
            {
                bool isActivePackageExpired = activePackage.RemainingSessions < 0 || activePackage.ExpiryDate < DateTime.Now;
                if (activePackage.MembershipEdition > 0)
                {
                    var nextActivePackage = await _dbContext.Packages.FirstOrDefaultAsync(x => x.UserId == membership.UserId && x.Id > activePackage.Id && x.RemainingSessions > 0);
                    if (nextActivePackage != null && nextActivePackage.MembershipEdition > 0)
                    {
                        throw new Exception("");
                    }
                    else if (nextActivePackage == null)
                    {

                    }
                }
                else
                {
                    activePackage.MembershipEdition++;
                    activePackage.MembershipEditedBy = HttpContext.Current.User.Identity.Name;
                    activePackage.MembershipEditedOn = DateTime.Now;
                }
            }
            else
            {
                throw new Exception("Hội viên không có gói tập đang sử dụng. Vui lòng liên hệ ngay admin.");
            }

            LogLatestAction(new List<IFieldChangeLog> { membership });

            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(membership).State = EntityState.Detached;

            var rs = new UpdateMembershipRs();
            rs.Membership = _mapper.Map<MembershipDTO>(membership);
            return rs;
        }
    }
}
