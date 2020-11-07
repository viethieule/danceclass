using AutoMapper;
using DataAccess;
using DataAccess.Entities;
using DataAccess.IdentityAccessor;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Receptionist
{
    public interface IReceptionistService
    {
        Task<CreateReceptionistRs> Create(CreateReceptionistRq rq);
    }

    public class ReceptionistService : UserService, IReceptionistService
    {
        public ReceptionistService(
            ApplicationUserManager userManager,
            DanceClassDbContext dbContext,
            IMapper mapper,
            IConfigurationProvider mappingConfig) : base(userManager, dbContext, mapper)
        {
            
        }

        public async Task<CreateReceptionistRs> Create(CreateReceptionistRq rq)
        {
            var receptionist = _mapper.Map<ApplicationUser>(rq.Receptionist);
            receptionist.UserName = await GenerateUserName(receptionist.FullName);
            receptionist.Birthdate = DateTime.Now;

            var result = await _userManager.CreateAsync(receptionist, DEFAULT_PASSWORD);
            if (!result.Succeeded)
            {
                throw new Exception("Không thể tạo. " + string.Join(" ", result.Errors));
            }

            var user = await _dbContext.Users.SingleAsync(x => x.UserName == receptionist.UserName);
            await _userManager.AddToRoleAsync(user.Id, "Receptionist");

            return new CreateReceptionistRs()
            {
                ReceptionistId = user.Id
            };
        }
    }
}
