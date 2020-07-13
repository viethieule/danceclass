using AutoMapper;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Schedule
{
    public interface IScheduleService
    {
        Task<GetSchedulesRs> Get(GetSchedulesRq rq);
    }

    public class ScheduleService : BaseService, IScheduleService
    {
        public ScheduleService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<GetSchedulesRs> Get(GetSchedulesRq rq)
        {
            DateTime start = rq.Start.Date;
            DateTime end = rq.Start.AddDays(6).Date;

            var schedule = await _dbContext.Schedules
                .Where(s => !(DbFunctions.TruncateTime(s.OpeningDate) > end || DbFunctions.TruncateTime(s.EndingDate) < start))
                .ToListAsync();

            GetSchedulesRs rs = new GetSchedulesRs();
            rs.Schedules = _mapper.Map<List<ScheduleDTO>>(schedule);

            return rs;
        }
    }
}
