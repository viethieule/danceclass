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

namespace Services.Branch
{
    public interface IBranchService
    {
        Task<GetAllBranchRs> GetAll();
    }

    public class BranchService : BaseService, IBranchService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public BranchService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public async Task<GetAllBranchRs> GetAll()
        {
            var branches = await _dbContext.Branches.ProjectTo<BranchDTO>(_mappingConfig).ToListAsync();

            return new GetAllBranchRs
            {
                Branches = branches
            };
        }
    }
}
