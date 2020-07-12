using AutoMapper;
using Services.Class;
using Services.Members;
using Services.Package;
using Services.Schedule;
using Services.ScheduleMember;
using Services.Trainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Common
{
    public class MappingConfig
    {
        public static IMapper Mapper { get; private set; }

        public static void RegisterMappings()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>();
                cfg.CreateMap<DataAccess.Entities.Trainer, TrainerDTO>();
                cfg.CreateMap<DataAccess.Entities.Class, ClassDTO>();
                cfg.CreateMap<DataAccess.Entities.ScheduleMember, ScheduleMemberDTO>();
                cfg.CreateMap<DataAccess.Entities.Package, PackageDTO>();
                cfg.CreateMap<DataAccess.Entities.ApplicationUser, MemberDTO>();
                cfg.CreateMap<DataAccess.Entities.UserRole, UserRoleDTO>();
                cfg.CreateMap<DataAccess.Entities.Role, RoleDTO>();
                    
                cfg.CreateMap<PackageDTO, DataAccess.Entities.Package>();
                cfg.CreateMap<MemberDTO, DataAccess.Entities.ApplicationUser>();
            });
            
            Mapper = mapperConfig.CreateMapper();
        }
    }
}
