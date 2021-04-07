using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using DataAccess.Enums;
using DataAccess.Interfaces;
using Microsoft.AspNet.Identity;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Services.Registration
{
    public interface IRegistrationService
    {
        Task<int> Cancel(CancelRegistrationRq rq);
        Task<CreateRegistrationRs> Create(CreateRegistrationRq rq);
        Task<List<RegistrationDTO>> GetByScheduleDetail(int scheduleDetailId);
        Task ConfirmAttendance(int registrationId);
        Task<GetRegistrationsRs> GetByUser(GetRegistrationsRq rq);
    }

    public class RegistrationService : BaseService, IRegistrationService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public const int MAX_MEMBERS_PER_SESSION = 20;

        public RegistrationService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<int> Cancel(CancelRegistrationRq rq)
        {
            DataAccess.Entities.Registration registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.Id == rq.RegistrationId);
            if (registration == null)
            {
                throw new Exception("Đăng ký không tồn tại!");
            }

            bool isMember = HttpContext.Current.User.IsInRole("Member");
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            if (registration.UserId.ToString() != currentUserId && isMember)
            {
                throw new Exception("Không đủ quyền hủy đăng ký!");
            }

            var scheduleDetail = registration.ScheduleDetail;
            if (scheduleDetail == null)
            {
                throw new Exception("Lịch học không tồn tại!");
            }

            DateTime dateAttending = scheduleDetail.Date.Add(scheduleDetail.Schedule.StartTime);
            if (isMember && (DateTime.Now >= dateAttending || (dateAttending - DateTime.Now).TotalHours < 1))
            {
                throw new Exception("Chỉ có thể hủy đăng ký ít nhất 1 tiếng trước khi tập!");
            }
            
            _dbContext.Registrations.Remove(registration);

            if (scheduleDetail.Schedule.IsPrivate)
            {
                // Get the latest private package
                // Assuming that when there are more than 2 private packages, the old ones are expired according to common use cases
                var privatePackage = await _dbContext.Packages
                    .Where(p => p.UserId == registration.UserId && p.IsPrivate && p.ExpiryDate >= DateTime.Now.Date)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                if (privatePackage == null)
                {
                    throw new Exception("Hội viên không có thẻ tập cá nhân hoặc đã hết hạn");
                }

                privatePackage.RemainingSessions++;

                LogLatestAction(new List<IFieldChangeLog> { privatePackage });
            }
            else
            {
                // Increase remaining sessions of the user
                var activePackage = _dbContext.Packages.FirstOrDefault(x => x.UserId == registration.UserId && x.IsActive);
                if (activePackage == null)
                {
                    throw new Exception("Học viên chưa đăng ký gói tập hoặc các gói đã hết hạn");
                }

                activePackage.RemainingSessions++;

                var membership = await _dbContext.Memberships.FirstOrDefaultAsync(x => x.UserId == registration.UserId);
                if (membership == null)
                {
                    throw new Exception("Học viên chưa đăng ký gói tập");
                }

                membership.RemainingSessions++;

                LogLatestAction(new List<IFieldChangeLog> { membership, activePackage });
            }

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<CreateRegistrationRs> Create(CreateRegistrationRq rq)
        {
            var session = await _dbContext.ScheduleDetails.FirstOrDefaultAsync(x => x.Id == rq.Registration.ScheduleDetailId);
            if (session == null)
            {
                throw new Exception("Buổi học không tồn tại");
            }

            int userId = rq.Registration.UserId;

            if (session.Schedule.IsPrivate)
            {
                // Get the latest private package
                // Assuming that when there are more than 2 private packages, the old ones are expired according to common use cases
                var privatePackage = await _dbContext.Packages
                    .Where(p => p.UserId == userId && p.IsPrivate && p.ExpiryDate >= DateTime.Now.Date && p.RemainingSessions != 0)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefaultAsync();

                if (privatePackage == null)
                {
                    throw new Exception("Hội viên không có thẻ tập cá nhân hoặc đã hết hạn");
                }

                privatePackage.RemainingSessions--;

                LogLatestAction(new List<IFieldChangeLog> { privatePackage });
            }
            else
            {
                var membership = await _dbContext.Memberships.FirstOrDefaultAsync(x => x.UserId == userId);
                if (membership == null)
                {
                    throw new Exception("Học viên chưa đăng ký gói tập");
                }

                if (membership.RemainingSessions <= 0)
                {
                    throw new Exception("Bạn đã dùng hết số buổi của gói tập hiện tại.");
                }

                if (membership.ExpiryDate < DateTime.Now.Date)
                {
                    throw new Exception("Gói tập của bạn đã hết hạn.");
                }

                // Decrease remaining sessions of the user
                membership.RemainingSessions--;

                // Keep track of what package is being used
                var activePackage = _dbContext.Packages.FirstOrDefault(x => x.UserId == userId && x.IsActive);
                if (activePackage == null)
                {
                    throw new Exception("Học viên chưa đăng ký gói tập hoặc có gì đó không đúng! Vui lòng liên hệ admin tại 0943619526");
                }

                DataAccess.Entities.Package nextActivePackage = null;
                if (activePackage.RemainingSessions > 0)
                {
                    activePackage.RemainingSessions--;
                }
                else
                {
                    nextActivePackage = _dbContext.Packages.FirstOrDefault(p => p.UserId == userId && p.Id > activePackage.Id && p.RemainingSessions > 0 && !p.IsPrivate);
                    if (nextActivePackage != null)
                    {
                        activePackage.IsActive = false;
                        nextActivePackage.IsActive = true;
                        nextActivePackage.RemainingSessions--;
                    }
                }

                var logs = new List<IFieldChangeLog> { membership, activePackage };
                if (nextActivePackage != null) logs.Add(nextActivePackage);
                LogLatestAction(logs);
            }

            // Add registration
            DataAccess.Entities.Registration registration = _mapper.Map<DataAccess.Entities.Registration>(rq.Registration);

            registration.Status = RegistrationStatus.Registered;
            registration.DateRegistered = DateTime.Now;

            _dbContext.Registrations.Add(registration);
            await _dbContext.SaveChangesAsync();

            var registrationDTO = await _dbContext.Registrations
                .ProjectTo<RegistrationDTO>(_mappingConfig, dest => dest.User)
                .FirstOrDefaultAsync(u => u.Id == registration.Id);

            CreateRegistrationRs rs = new CreateRegistrationRs()
            {
                Registration = registrationDTO
            };

            return rs;
        }

        public async Task<List<RegistrationDTO>> GetByScheduleDetail(int scheduleDetailId)
        {
            var registrations = await _dbContext.Registrations
                .Where(r => r.ScheduleDetailId == scheduleDetailId)
                .ProjectTo<RegistrationDTO>(_mappingConfig, dest => dest.User)
                .ToListAsync();

            return registrations;
        }

        public async Task ConfirmAttendance(int registrationId)
        {
            var registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.Id == registrationId);
            if (registration == null)
            {
                throw new Exception("Đăng ký không tồn tại");
            }

            registration.Status = RegistrationStatus.Attended;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<GetRegistrationsRs> GetByUser(GetRegistrationsRq rq)
        {
            var rs = new GetRegistrationsRs();

            bool isMember = HttpContext.Current.User.IsInRole("Member");
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            if (isMember && rq.UserId.ToString() != currentUserId)
            {
                return rs;
            }

            var registrations = await _dbContext.Registrations
                .Where(r => r.UserId == rq.UserId)
                .ProjectTo<RegistrationDTO>(_mappingConfig, dest => dest.ScheduleDetail.Schedule.Class, dest => dest.ScheduleDetail.Schedule.Trainer)
                .ToListAsync();

            rs.Registrations = registrations;
            return rs;
        }
    }
}
