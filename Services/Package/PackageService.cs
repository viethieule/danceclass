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

    public class PackageService : IPackageService
    {
        public async Task<List<PackageDTO>> GetAll(bool isDefault)
        {
            using (DanceClassDbContext db = new DanceClassDbContext())
            {
                List<DataAccess.Entities.Package> packages = await db.Packages.Where(p => p.IsDefault == isDefault).ToListAsync();
                return MappingConfig.Mapper.Map<List<PackageDTO>>(packages);
            }
        }
    }
}
