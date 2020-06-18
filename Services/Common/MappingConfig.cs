using AutoMapper;
using Services.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Common
{
    public class MappingConfig
    {
        private static IMapper _mapper;
        public static IMapper Mapper
        {
            get
            {
                return _mapper;
            }
        }

        public static void RegisterMappings()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
        }
    }
}
