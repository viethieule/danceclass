using AutoMapper;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Common
{
    public abstract class BaseService
    {
        protected readonly DanceClassDbContext _dbContext;
        protected readonly IMapper _mapper;

        public BaseService(DanceClassDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
    }
}
