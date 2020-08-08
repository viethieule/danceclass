using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using Microsoft.AspNet.Identity;
using Services.Common;
using Services.Registration;
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
        Task<CreateScheduleRs> Create(CreateScheduleRq rq);
    }

    public class ScheduleService : BaseService, IScheduleService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public ScheduleService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<CreateScheduleRs> Create(CreateScheduleRq rq)
        {
            var scheduleDTO = rq.Schedule;
            var schedule = _mapper.Map<DataAccess.Entities.Schedule>(scheduleDTO);

            if (!string.IsNullOrEmpty(scheduleDTO.TrainerName))
            {
                schedule.Trainer = new DataAccess.Entities.Trainer
                {
                    Name = scheduleDTO.TrainerName
                };
            }

            if (!string.IsNullOrEmpty(scheduleDTO.ClassName))
            {
                schedule.Class = new DataAccess.Entities.Class
                {
                    Name = scheduleDTO.ClassName
                };
            }

            if (schedule.Sessions.HasValue && !string.IsNullOrEmpty(schedule.DaysPerWeek))
            {
                var scheduleDetails = GenerateScheduleDetails(schedule);
                if (scheduleDetails.Count == 0)
                {
                    throw new Exception("Không thể tạo thời khóa biểu chi tiết cho lớp học!");
                }

                schedule.ScheduleDetails = scheduleDetails;
                schedule.EndingDate = scheduleDetails.Last().Date;
            }
            else
            {
                schedule.ScheduleDetails = new List<DataAccess.Entities.ScheduleDetail>
                {
                    new DataAccess.Entities.ScheduleDetail
                    {
                        Date = schedule.OpeningDate,
                        ScheduleId = schedule.Id,
                        SessionNo = 1
                    }
                };
            }

            _dbContext.Schedules.Add(schedule);
            await _dbContext.SaveChangesAsync();
            _dbContext.Entry(schedule).State = EntityState.Detached;

            return new CreateScheduleRs
            {
                Schedule = _mapper.Map<ScheduleDTO>(schedule)
            };
        }

        private List<DataAccess.Entities.ScheduleDetail> GenerateScheduleDetails(DataAccess.Entities.Schedule schedule)
        {
            var scheduleDetails = new List<DataAccess.Entities.ScheduleDetail>();

            if (!schedule.Sessions.HasValue || string.IsNullOrEmpty(schedule.DaysPerWeek))
            {
                return scheduleDetails;
            }

            DateTime date = schedule.OpeningDate;
            int totalSessions = schedule.Sessions.Value;
            int[] recurDays = schedule.DaysPerWeek.Select(x => int.Parse(x.ToString())).ToArray();

            int startIndex = Array.IndexOf(recurDays, (int)date.DayOfWeek);
            if (startIndex == -1)
            {
                throw new Exception("Ngày bắt đầu không nằm trong số buổi hàng tuần!");
            }

            for (int i = startIndex, j = 1; i >= -1 && j > 0; i++, j++)
            {
                scheduleDetails.Add(new DataAccess.Entities.ScheduleDetail
                {
                    ScheduleId = schedule.Id,
                    Date = date,
                    SessionNo = j
                });

                if (scheduleDetails.Count == totalSessions)
                {
                    break;
                }

                if (i == recurDays.Length - 1)
                {
                    date = date.AddDays(7 - (recurDays[i] - recurDays[0]));
                    i = -1;
                }
                else
                {
                    date = date.AddDays(recurDays[i + 1] - recurDays[i]);
                }
            }

            return scheduleDetails;
        }

        public async Task<GetDetailedScheduleRs> GetDetail(GetDetailedScheduleRq rq)
        {
            DateTime start = rq.Start.Date;
            DateTime end = rq.Start.AddDays(6).Date;

            var scheduleDetailDtos = await _dbContext.ScheduleDetails
                .Where(x => !(DbFunctions.TruncateTime(x.Date) > end || DbFunctions.TruncateTime(x.Date) < start))
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Registrations.Select(r => r.User), dest => dest.Schedule.Class)
                .ToListAsync();

            var isMember = HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.User.IsInRole("Member");
            
            if (isMember)
            {
                int userId = int.Parse(HttpContext.Current.User.Identity.GetUserId());
                AppendCurrentUserRegistrationToDtos(scheduleDetailDtos);
            }

            GetDetailedScheduleRs rs = new GetDetailedScheduleRs
            {
                ScheduleDetails = scheduleDetailDtos
            };

            return rs;
        }

        private void AppendCurrentUserRegistrationToDtos(List<ScheduleDetailDTO> dtos)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            dtos.ForEach(dto =>
            {
                var currentUserRegistration = dto.Registrations.FirstOrDefault(r => r.UserId.ToString() == userId);
                if (currentUserRegistration != null)
                {
                    dto.CurrentUserRegistration = currentUserRegistration;
                    dto.IsCurrentUserRegistered = true;
                }
            });
        }
    }
}
