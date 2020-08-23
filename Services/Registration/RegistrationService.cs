using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using DataAccess.Enums;
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

            bool isAdmin = HttpContext.Current.User.IsInRole("Admin");
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            if (registration.UserId.ToString() != currentUserId && !isAdmin)
            {
                throw new Exception("Không đủ quyền hủy đăng ký!");
            }

            var scheduleDetail = registration.ScheduleDetail;
            if (scheduleDetail == null)
            {
                throw new Exception("Lịch học không tồn tại!");
            }

            DateTime dateAttending = scheduleDetail.Date.Add(scheduleDetail.Schedule.StartTime);
            if (!isAdmin && (DateTime.Now >= dateAttending || (dateAttending - DateTime.Now).TotalHours < 1))
            {
                throw new Exception("Chỉ có thể hủy đăng ký ít nhất 1 tiếng trước khi tập!");
            }
            
            _dbContext.Registrations.Remove(registration);

            // Increase remaining sessions of the user
            var memberPackage = _dbContext.MemberPackages.FirstOrDefault(x => x.UserId == registration.UserId && x.IsActive);
            if (memberPackage == null)
            {
                throw new Exception("Học viên chưa đăng ký gói tập nào!");
            }
            else if (memberPackage.RemainingSessions >= memberPackage.Package.NumberOfSessions)
            {
                throw new Exception("Không thể hủy. Bạn vẫn chưa đăng ký buổi học nào mà!");
            }

            memberPackage.RemainingSessions++;

            return await _dbContext.SaveChangesAsync();
        }

        public async Task<CreateRegistrationRs> Create(CreateRegistrationRq rq)
        {
            int userId = rq.Registration.UserId;
            // Decrease remaining sessions of the user
            var memberPackage = _dbContext.MemberPackages.FirstOrDefault(x => x.UserId == userId && x.IsActive);
            if (memberPackage == null)
            {
                throw new Exception("Học viên chưa đăng ký gói tập!");
            }

            if (memberPackage.RemainingSessions <= 0)
            {
                throw new Exception("Bạn đã dùng hết gói tập hiện tại. Vui lòng đăng ký gói mới!");
            }

            memberPackage.RemainingSessions--;

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

            bool isAdmin = HttpContext.Current.User.IsInRole("Admin");
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();

            if (rq.UserId.ToString() != currentUserId && !isAdmin)
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
