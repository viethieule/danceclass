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
        Task<UpdateScheduleRs> Update(UpdateScheduleRq rq);
        Task<UpdateScheduleRs> PreUpdate(UpdateScheduleRq rq);
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

        public async Task<UpdateScheduleRs> Update(UpdateScheduleRq rq)
        {
            var rs = new UpdateScheduleRs();

            var scheduleDto = rq.Schedule;
            var updatedSchedule = _mapper.Map<DataAccess.Entities.Schedule>(scheduleDto);

            var currentSchedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.Id == scheduleDto.Id);
            if (currentSchedule == null)
            {
                throw new Exception("Lịch học không tồn tại");
            }

            if (updatedSchedule.OpeningDate.Date != currentSchedule.OpeningDate.Date)
            {
                if (DateTime.Now > currentSchedule.OpeningDate.Add(updatedSchedule.StartTime))
                {
                    throw new Exception("Không thể thay đổi ngày bắt đầu của lớp học đã bắt đầu");
                }
            }

            if (updatedSchedule.StartTime != currentSchedule.StartTime)
            {
                rs.Messages.Add("Thời gian lớp học đã thay đổi. Vui lòng thông báo lại cho các hội viên đã đăng ký");
            }

            if (updatedSchedule.OpeningDate.Date != currentSchedule.OpeningDate.Date ||
                updatedSchedule.DaysPerWeek != currentSchedule.DaysPerWeek ||
                updatedSchedule.Sessions != currentSchedule.Sessions)
            {
                var updatedScheduleDetails = GenerateScheduleDetails(updatedSchedule);
                var currentScheduleDetails = await _dbContext.ScheduleDetails.Where(s => s.ScheduleId == updatedSchedule.Id).ToListAsync();
                var changedSessionNumbers = new List<int>();

                foreach (var updatedSession in updatedScheduleDetails)
                {
                    var currentSession = currentScheduleDetails.FirstOrDefault(s => s.SessionNo == updatedSession.SessionNo);
                    if (currentSession != null)
                    {
                        if (updatedSession.Date.Date != currentSession.Date.Date)
                        {
                            _dbContext.ScheduleDetails.Remove(currentSession);

                            updatedSession.DateBeforeUpdated = currentSession.Date;
                            updatedSession.Registrations = currentSession.Registrations;
                            _dbContext.ScheduleDetails.Add(updatedSession);

                            changedSessionNumbers.Add(updatedSession.SessionNo);
                        }
                    }
                    else
                    {
                        _dbContext.ScheduleDetails.Add(updatedSession);
                    }
                }

                if (changedSessionNumbers.Count > 0)
                {
                    rs.Messages.Add(string.Format("Đã có thay đổi trong buổi học {0}. Vui lòng thông báo lại cho các học viên đã đăng ký", string.Join(", ", changedSessionNumbers)));
                }

                if (updatedScheduleDetails.Count < currentScheduleDetails.Count)
                {
                    var deletedScheduleDetails = currentScheduleDetails.Where(s => s.SessionNo > updatedScheduleDetails.Count);

                    var deletedSessionWithRegistrationNumbers = new List<int>();
                    
                    foreach (var deletedSession in deletedScheduleDetails)
                    {
                        if (deletedSession.Registrations.Any())
                        {
                            foreach (var registration in deletedSession.Registrations)
                            {
                                var memberPackage = await _dbContext.MemberPackages.FirstOrDefaultAsync(m => m.UserId == registration.UserId && m.IsActive);
                                memberPackage.RemainingSessions++;
                            }

                            deletedSessionWithRegistrationNumbers.Add(deletedSession.SessionNo);
                        }
                    }

                    _dbContext.ScheduleDetails.RemoveRange(deletedScheduleDetails);

                    if (deletedSessionWithRegistrationNumbers.Count > 0)
                    {
                        rs.Messages.Add(string.Format("Buổi học {0} đã có hội viên đăng ký và đã được xóa. Hội viên đã được hoàn lại một buổi. Vui lòng thông báo lại cho các học viên đã đăng ký", string.Join(", ", deletedSessionWithRegistrationNumbers)));
                    }
                }
            }

            _mapper.Map(updatedSchedule, currentSchedule);
            _dbContext.SaveChanges();

            return rs;
        }

        public Task<UpdateScheduleRs> PreUpdate(UpdateScheduleRq rq)
        {
            throw new NotImplementedException();
        }
    }
}
