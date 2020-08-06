using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataAccess;
using Services.Common;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Services.Class
{
    public interface IClassService
    {
        Task<List<ClassDTO>> GetAll();
    }

    public class ClassService : BaseService, IClassService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public ClassService(
            DanceClassDbContext dbContext,
            IMapper mapper,
            IConfigurationProvider mappingConfig
            ) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<List<ClassDTO>> GetAll()
        {
            return await _dbContext.Classes
                .ProjectTo<ClassDTO>(_mappingConfig)
                .ToListAsync();
        }
    }
}
