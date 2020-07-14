using AutoMapper;
using Services.Class;
using Services.Common.AutoMapper.ApplicationUserResolver;
using Services.Members;
using Services.Package;
using Services.Schedule;
using Services.Registration;
using Services.Trainer;

namespace Services.Common.AutoMapper
{
    public class EntityToDtoMappingProfile : Profile
    {
        public EntityToDtoMappingProfile()
        {
            CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>();
            CreateMap<DataAccess.Entities.Trainer, TrainerDTO>();
            CreateMap<DataAccess.Entities.Class, ClassDTO>();
            CreateMap<DataAccess.Entities.Registration, RegistrationDTO>();
            CreateMap<DataAccess.Entities.Package, PackageDTO>();
            CreateMap<DataAccess.Entities.ApplicationUser, MemberDTO>()
                .ForMember(x => x.RoleNames, opt => opt.MapFrom<ApplicationUserRolesNameResolver>());
            CreateMap<DataAccess.Entities.UserRole, UserRoleDTO>();
            CreateMap<DataAccess.Entities.Role, RoleDTO>();
        }
    }
}
