using AutoMapper;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Package
{
    public interface IPackageService
    {
        Task<List<PackageDTO>> GetAll(bool isDefault);
    }

    public class PackageService : BaseService, IPackageService
    {
        public PackageService(DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<List<PackageDTO>> GetAll(bool isDefault)
        {
            List<DataAccess.Entities.Package> packages = await _dbContext.Packages.Where(p => p.IsDefault == isDefault).ToListAsync();
            return _mapper.Map<List<PackageDTO>>(packages);
        }
    }
}
