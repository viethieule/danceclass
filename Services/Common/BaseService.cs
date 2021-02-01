using AutoMapper;
using DataAccess;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        protected void LogLatestAction(
            List<IFieldChangeLog> entities,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            foreach (var entity in entities)
            {
                string fileName = sourceFilePath.Split('\\').LastOrDefault() ?? string.Empty;
                entity.LatestAction = string.Format("{0}/{1}/Line {2}", memberName, fileName, sourceLineNumber);
            }
        }
    }
}
