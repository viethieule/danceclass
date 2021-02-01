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

            LogLatestAction(new List<IFieldChangeLog> { membership });

            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(membership).State = EntityState.Detached;

            var rs = new UpdateMembershipRs();
            rs.Membership = _mapper.Map<MembershipDTO>(membership);
            return rs;
        }
    }
}
