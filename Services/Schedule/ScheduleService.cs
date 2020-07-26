using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        Task<GetRegisteredSessionRs> GetRegisteredSessions(GetRegisteredSessionRq rq);
    }

    public class ScheduleService : BaseService, IScheduleService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public ScheduleService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<GetDetailedScheduleRs> GetDetail(GetDetailedScheduleRq rq)
        {
            DateTime start = rq.Start.Date;
            DateTime end = rq.Start.AddDays(6).Date;

            var scheduleDetailDtos = await _dbContext.ScheduleDetails
                .Where(x => !(DbFunctions.TruncateTime(x.Date) > end || DbFunctions.TruncateTime(x.Date) < start))
                //.Include(x => x.Schedule.Class)
                //.Include(x => x.Registrations)
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Registrations, dest => dest.Schedule.Class)
                .ToListAsync();

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

            GetDetailedScheduleRs rs = new GetDetailedScheduleRs
            {
                ScheduleDetails = scheduleDetailDtos
            };

            return rs;
        }

        public async Task<GetRegisteredSessionRs> GetRegisteredSessions(GetRegisteredSessionRq rq)
        {
            var sessions = await _dbContext.ScheduleDetails
                .Where(s => s.Registrations.Any(r => r.UserId == rq.UserId))
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, s => s.Schedule.Trainer, s => s.Schedule.Class)
                .ToListAsync();

            return new GetRegisteredSessionRs
            {
                Sessions = sessions
            };
        }
    }
}
