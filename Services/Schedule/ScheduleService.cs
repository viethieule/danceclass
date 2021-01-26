using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using Microsoft.AspNet.Identity;
using Services.Common;
using Services.Registration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        Task<DeleteScheduleSessionRs> Delete(int id);
        Task<DeleteScheduleSessionRs> DeleteSession(int scheduleDetailId);
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
            DateTime end = rq.Start.AddDays(7).AddSeconds(-1).Date;

            List<ScheduleDetailDTO> scheduleDetailDtos = await _dbContext.ScheduleDetails
                .Where(x => x.Date <= end && x.Date >= start)
                .ProjectTo<ScheduleDetailDTO>(_mappingConfig, dest => dest.Schedule.Class, dest => dest.Schedule.Trainer)
                .ToListAsync();

            var isMember = HttpContext.Current.User.Identity.IsAuthenticated && HttpContext.Current.User.IsInRole("Member");
            
            if (isMember)
            {
                int userId = int.Parse(HttpContext.Current.User.Identity.GetUserId());
                await AppendCurrentUserRegistrationToDtos(scheduleDetailDtos);
            }

            GetDetailedScheduleRs rs = new GetDetailedScheduleRs
            {
                ScheduleDetails = scheduleDetailDtos
            };

            return rs;
        }

        private async Task AppendCurrentUserRegistrationToDtos(List<ScheduleDetailDTO> dtos)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var scheduleDetailIds = dtos.Select(d => d.Id);
            var currentUserRegistrations = await _dbContext.Registrations
                .Where(r => scheduleDetailIds.Contains(r.ScheduleDetailId) && r.UserId.ToString() == userId)
                .ProjectTo<RegistrationDTO>(_mappingConfig)
                .ToListAsync();

            dtos.ForEach(dto =>
            {
                var currentUserRegistration = currentUserRegistrations.FirstOrDefault(r => r.ScheduleDetailId == dto.Id);
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

            bool newTrainerCreate = !string.IsNullOrEmpty(scheduleDto.TrainerName);
            bool newClassCreate = !string.IsNullOrEmpty(scheduleDto.ClassName);

            var query = _dbContext.Schedules.AsQueryable();

            if (newTrainerCreate)
            {
                query = query.Include(s => s.Trainer.Schedules);
            }

            if (newClassCreate)
            {
                query = query.Include(s => s.Class.Schedules);
            }

            var currentSchedule = await query.FirstOrDefaultAsync(s => s.Id == scheduleDto.Id);

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

            if (newTrainerCreate)
            {
                currentSchedule.Trainer.Schedules.Remove(currentSchedule);
                currentSchedule.Trainer = new DataAccess.Entities.Trainer
                {
                    Name = scheduleDto.TrainerName
                };
            }
            else if (updatedSchedule.TrainerId != null && currentSchedule.TrainerId != updatedSchedule.TrainerId)
            {
                currentSchedule.TrainerId = updatedSchedule.TrainerId;
            }

            if (newClassCreate)
            {
                currentSchedule.Class.Schedules.Remove(currentSchedule);
                currentSchedule.Class = new DataAccess.Entities.Class
                {
                    Name = scheduleDto.ClassName
                };
            }
            else if (updatedSchedule.ClassId != null && currentSchedule.ClassId != updatedSchedule.ClassId)
            {
                currentSchedule.ClassId = updatedSchedule.ClassId;
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
                var currentScheduleDetails = await _dbContext.ScheduleDetails
                    .Where(s => s.ScheduleId == updatedSchedule.Id)
                    .Include(s => s.Registrations)
                    .ToListAsync();
                var changedSessionWithRegistrationNumbers = new List<int>();

                foreach (var updatedSession in updatedScheduleDetails)
                {
                    var currentSession = currentScheduleDetails.FirstOrDefault(s => s.SessionNo == updatedSession.SessionNo);
                    if (currentSession != null)
                    {
                        if (updatedSession.Date.Date != currentSession.Date.Date)
                        {
                            updatedSession.DateBeforeUpdated = currentSession.Date;
                            if (currentSession.Registrations.Any())
                            {
                                changedSessionWithRegistrationNumbers.Add(updatedSession.SessionNo);
                                foreach (var registration in currentSession.Registrations)
                                {
                                    registration.ScheduleDetail = updatedSession;
                                }
                            }

                            _dbContext.ScheduleDetails.Remove(currentSession);
                            _dbContext.ScheduleDetails.Add(updatedSession);                            

                            if (rq.SelectedScheduleDetailId == currentSession.Id)
                            {
                                rs.IsSelectedSessionUpdated = true;
                                rs.SelectedSessionNumber = currentSession.SessionNo;
                            }
                        }
                    }
                    else
                    {
                        _dbContext.ScheduleDetails.Add(updatedSession);
                    }
                }

                if (changedSessionWithRegistrationNumbers.Count > 0)
                {
                    rs.Messages.Add(string.Format("Đã có thay đổi về thời gian trong buổi học {0}. Vui lòng thông báo lại cho các học viên đã đăng ký", string.Join(", ", changedSessionWithRegistrationNumbers)));
                }

                if (updatedScheduleDetails.Count < currentScheduleDetails.Count)
                {
                    var deletedScheduleDetails = currentScheduleDetails.Where(s => s.SessionNo > updatedScheduleDetails.Count);

                    var deletedSessionWithRegistrationNumbers = new List<int>();
                    
                    foreach (var deletedSession in deletedScheduleDetails)
                    {
                        if (deletedSession.Registrations.Any())
                        {
                            var registeredUserIds = deletedSession.Registrations.Select(r => r.UserId).ToList();
                            var relatedPackages = await _dbContext.Packages.Where(p => p.IsActive && registeredUserIds.Contains(p.UserId)).ToListAsync();
                            var relatedMemberships = await _dbContext.Memberships.Where(m => registeredUserIds.Contains(m.UserId)).ToListAsync();

                            foreach (var package in relatedPackages)
                            {
                                package.RemainingSessions++;
                            }

                            foreach (var membership in relatedMemberships)
                            {
                                membership.RemainingSessions++;
                            }

                            deletedSessionWithRegistrationNumbers.Add(deletedSession.SessionNo);
                        }

                        if (rq.SelectedScheduleDetailId == deletedSession.Id)
                        {
                            rs.IsSelectedSessionDeleted = true;
                            rs.Messages.Add("Buổi học này đã bị xóa sau khi cập nhật số buổi");
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

            await _dbContext.SaveChangesAsync();

            rs.Schedule = await _dbContext.Schedules.ProjectTo<ScheduleDTO>(_mappingConfig).FirstOrDefaultAsync(s => s.Id == currentSchedule.Id);
            if (rs.IsSelectedSessionUpdated && rs.SelectedSessionNumber.HasValue)
            {
                var updatedSession = await _dbContext.ScheduleDetails.FirstOrDefaultAsync(s => s.ScheduleId == currentSchedule.Id && s.SessionNo == rs.SelectedSessionNumber.Value);
                if (updatedSession != null)
                {
                    rs.UpdatedSessionId = updatedSession.Id;
                }
            }

            return rs;
        }

        public async Task<DeleteScheduleSessionRs> Delete(int id)
        {
            var rs = new DeleteScheduleSessionRs();

            var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id);
            if (schedule == null)
            {
                throw new Exception("Lịch học không tồn tại");
            }

            var userAndCountRegistrationMap = schedule.ScheduleDetails
                .SelectMany(x => x.Registrations)
                .Select(x => x.UserId)
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            rs.IsUserGetSessionBack = ReturnSessionBackToRegisteredUser(userAndCountRegistrationMap);

            _dbContext.Schedules.Remove(schedule);
            await _dbContext.SaveChangesAsync();

            rs.Success = true;
            return rs;
        }

        public async Task<DeleteScheduleSessionRs> DeleteSession(int scheduleDetailId)
        {
            var session = await _dbContext.ScheduleDetails.FirstOrDefaultAsync(x => x.Id == scheduleDetailId);
            if (session == null)
            {
                throw new Exception("Buổi học không tồn tại!");
            }

            if (session.SessionNo == 1)
            {
                return await this.Delete(session.ScheduleId);
            }

            var rs = new DeleteScheduleSessionRs();

            var schedule = await _dbContext.Schedules.FirstOrDefaultAsync(s => s.Id == session.ScheduleId);
            schedule.Sessions = session.SessionNo - 1;

            var deletedSessions = _dbContext.ScheduleDetails
                .Where(x => x.ScheduleId == session.ScheduleId && x.SessionNo >= session.SessionNo);

            var userAndCountRegistrationMap = await deletedSessions
                .SelectMany(x => x.Registrations)
                .Select(x => x.UserId)
                .GroupBy(x => x)
                .ToDictionaryAsync(x => x.Key, x => x.Count());

            rs.IsUserGetSessionBack = ReturnSessionBackToRegisteredUser(userAndCountRegistrationMap);

            _dbContext.ScheduleDetails.RemoveRange(deletedSessions);
            await _dbContext.SaveChangesAsync();

            rs.Success = true;
            return rs;
        }

        private bool ReturnSessionBackToRegisteredUser(Dictionary<int, int> userAndCountRegistrationMap)
        {
            if (userAndCountRegistrationMap.Count() == 0)
            {
                return false;
            }

            var registeredUserIds = userAndCountRegistrationMap.Keys;
            var memberPackages = _dbContext.Packages.Where(x => registeredUserIds.Contains(x.UserId) && x.IsActive);
            foreach (var memberPackage in memberPackages)
            {
                memberPackage.RemainingSessions += userAndCountRegistrationMap[memberPackage.UserId];
            }

            var memberships = _dbContext.Memberships.Where(x => registeredUserIds.Contains(x.UserId));
            foreach (var membership in memberships)
            {
                membership.RemainingSessions += userAndCountRegistrationMap[membership.UserId];
            }

            return true;
        }
    }
}
