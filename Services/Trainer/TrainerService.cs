using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Trainer
{
    public interface ITrainerService
    {
        Task<List<TrainerDTO>> GetAll();
    }

    public class TrainerService : BaseService, ITrainerService
    {
        private readonly IConfigurationProvider _mappingConfig;
        public TrainerService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<List<TrainerDTO>> GetAll()
        {
            return await _dbContext.Trainers
                .ProjectTo<TrainerDTO>(_mappingConfig)
                .ToListAsync();
        }
    }
}
