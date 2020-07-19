using AutoMapper;
using DataAccess;
using Microsoft.AspNet.Identity;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Services.Schedule
{
    public interface IScheduleService
    {
        Task<GetDetailedScheduleRs> GetDetail(GetDetailedScheduleRq rq);
    }

    public class ScheduleService : BaseService, IScheduleService
    {
        public ScheduleService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<GetDetailedScheduleRs> GetDetail(GetDetailedScheduleRq rq)
        {
            DateTime start = rq.Start.Date;
            DateTime end = rq.Start.AddDays(6).Date;

            var schedule = await _dbContext.Schedules
                .Where(x => !(DbFunctions.TruncateTime(x.OpeningDate) > end || DbFunctions.TruncateTime(x.EndingDate) < start))
                .ToListAsync();

            var scheduleIds = schedule.Select(x => x.Id);
            var scheduleDetails = await _dbContext.ScheduleDetails
                .Where(x => scheduleIds.Contains(x.ScheduleId))
                .ToListAsync();

            GetDetailedScheduleRs rs = new GetDetailedScheduleRs();
            var scheduleDetailDtos = _mapper.Map<List<ScheduleDetailDTO>>(scheduleDetails);

            var isMember = HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.User.IsInRole("Member");

            if (isMember)
            {
                int userId = int.Parse(HttpContext.Current.User.Identity.GetUserId());
                scheduleDetailDtos.ForEach(x =>
                {
                    var currentUserRegistration = x.Registrations.FirstOrDefault(r => r.UserId == userId);
                    if (currentUserRegistration != null)
                    {
                        x.CurrentUserRegistration = currentUserRegistration;
                        x.IsCurrentUserRegistered = true;
                    }
                });
            }

            rs.ScheduleDetails = scheduleDetailDtos;

            return rs;
        }
    }
}
