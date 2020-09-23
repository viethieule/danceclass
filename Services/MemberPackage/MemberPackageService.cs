using AutoMapper;
using DataAccess;
using Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MemberPackage
{
    public interface IMemberPackageService
    {
        Task<CreateMemberPackageRs> Create(CreateMemberPackageRq rq);
    }

    public class MemberPackageService : BaseService, IMemberPackageService
    {
        private readonly IConfigurationProvider _mappingConfig;

        public MemberPackageService(DanceClassDbContext dbContext, IMapper mapper, IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _mappingConfig = mappingConfig;
        }

        public Task<CreateMemberPackageRs> Create(CreateMemberPackageRq rq)
        {
            throw new NotImplementedException();
        }
    }
}
