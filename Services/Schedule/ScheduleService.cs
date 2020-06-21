using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Schedule
{
    public class ScheduleService
    {
        public ScheduleService()
        {
        }
            
        public async Task<GetSchedulesRs> Get(GetSchedulesRq rq)
        {
            using (DanceClassDbContext db = new DanceClassDbContext())
            {
                DateTime start = rq.Start.Date;
                DateTime end = rq.Start.AddDays(6).Date;

                var schedule = await db.Schedules
                    .Where(s => !(DbFunctions.TruncateTime(s.OpeningDate) > end || DbFunctions.TruncateTime(s.EndingDate) < start))
                    .ToListAsync();

                GetSchedulesRs rs = new GetSchedulesRs();
                rs.Schedules = MappingConfig.Mapper.Map<List<ScheduleDTO>>(schedule);

                return rs;
            }
        }
    }
}
