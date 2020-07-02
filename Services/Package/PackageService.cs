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
    public class PackageService
    {
        public async Task<List<PackageDTO>> GetAll()
        {
            using (DanceClassDbContext db = new DanceClassDbContext())
            {
                List<DataAccess.Entities.Package> packages = await db.Packages.ToListAsync();
                return MappingConfig.Mapper.Map<List<PackageDTO>>(packages);
            }
        }
    }
}
