using AutoMapper;
using DataAccess;
using Microsoft.AspNet.Identity;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.Registration
{
    public interface IRegistrationService
    {
        Task<int> Cancel(int id);
        Task<CreateRegistrationRs> Create(CreateRegistrationRq rq);
    }

    public class RegistrationService : BaseService, IRegistrationService
    {
        public RegistrationService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<int> Cancel(int id)
        {
            DataAccess.Entities.Registration registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.Id == id);
            if (registration == null)
            {
                throw new Exception("Đăng ký không tồn tại!");
            }

            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            if (registration.UserId.ToString() != currentUserId && !HttpContext.Current.User.IsInRole("Admin"))
            {
                throw new Exception("Không đủ quyền hủy đăng ký!");
            }

            var scheduleDetail = registration.ScheduleDetail;
            if (scheduleDetail == null)
            {
                throw new Exception("Lịch học không tồn tại!");
            }

            DateTime dateAttending = scheduleDetail.Date.Add(scheduleDetail.Schedule.StartTime);
            if (DateTime.Now >= dateAttending || (dateAttending - DateTime.Now).TotalHours < 1)
            {
                throw new Exception("Chỉ có thể hủy đăng ký ít nhất 1 tiếng trước khi tập!");
            }
                
            _dbContext.Registrations.Remove(registration);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<CreateRegistrationRs> Create(CreateRegistrationRq rq)
        {
            DataAccess.Entities.Registration registration = _mapper.Map<DataAccess.Entities.Registration>(rq.Registration);
            var userId = HttpContext.Current.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Vui lòng đăng nhập để đăng ký lớp học");
            }

            registration.UserId = int.Parse(userId);
            registration.DateRegistered = DateTime.Now;

            _dbContext.Registrations.Add(registration);
            await _dbContext.SaveChangesAsync();

            CreateRegistrationRs rs = new CreateRegistrationRs()
            {
                Registration = _mapper.Map<RegistrationDTO>(registration)
            };

            return rs;
        }
    }
}
