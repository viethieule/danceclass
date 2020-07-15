using AutoMapper;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Registration
{
    public interface IRegistrationService
    {
        Task Cancel(int id);
        Task<CreateRegistrationRs> CreateRegistration(int id);
        Task<GetRegistrationResponse> GetCurrentUserRegistrations();
    }

    public class RegistrationService : BaseService, IRegistrationService
    {
        public RegistrationService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task Cancel(int id)
        {
            DataAccess.Entities.Registration registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.Id == id);
            if (registration == null)
            {
                throw new Exception("Đăng ký không tồn tại!");
            }

            DataAccess.Entities.Schedule schedule = registration.Schedule;
            if (schedule == null)
            {
                throw new Exception("Lịch học không tồn tại!");
            }

            DateTime dateAttending = registration.DateAttending.Add(schedule.StartTime);
            if (DateTime.Now >= dateAttending || (dateAttending - DateTime.Now).TotalHours < 1)
            {
                throw new Exception("Chỉ có thể hủy đăng ký ít nhất 1 tiếng trước khi tập!");
            }
                
            _dbContext.Registrations.Remove(registration);
            await _dbContext.SaveChangesAsync();
        }

        public Task<CreateRegistrationRs> CreateRegistration(int id)
        {
            throw new NotImplementedException();
        }

        public Task<GetRegistrationResponse> GetCurrentUserRegistrations()
        {
            throw new NotImplementedException();
        }
    }
}
