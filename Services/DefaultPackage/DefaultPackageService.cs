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

namespace Services.DefaultPackage
{
    public interface IDefaultPackageService
    {
        Task<List<DefaultPackageDTO>> GetAll(bool isDefault);
    }

    public class DefaultPackageService : BaseService, IDefaultPackageService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public DefaultPackageService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<List<DefaultPackageDTO>> GetAll(bool isDefault)
        {
            return await _dbContext.DefaultPackages.ProjectTo<DefaultPackageDTO>(_mappingConfig).ToListAsync();
        }
    }
}
