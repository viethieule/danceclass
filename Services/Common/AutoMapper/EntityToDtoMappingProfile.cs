using AutoMapper;
using Services.Class;
using Services.Common.AutoMapper.ApplicationUserResolver;
using Services.Members;
using Services.Package;
using Services.Schedule;
using Services.Registration;
using Services.Trainer;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace Services.Common.AutoMapper
{
    public class EntityToDtoMappingProfile : Profile
    {
        public EntityToDtoMappingProfile()
        {
            CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>()
                .ForMember(x => x.ScheduleDetails, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Class, opt => opt.ExplicitExpansion());
            CreateMap<DataAccess.Entities.ScheduleDetail, ScheduleDetailDTO>()
                .ForMember(x => x.TotalRegistered, opt => opt.MapFrom(m => m.Registrations.Count()))
                .ForMember(x => x.Registrations, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Schedule, opt => opt.ExplicitExpansion());
            CreateMap<DataAccess.Entities.Trainer, TrainerDTO>();
            CreateMap<DataAccess.Entities.Class, ClassDTO>();
            CreateMap<DataAccess.Entities.Registration, RegistrationDTO>()
                .ForMember(x => x.ScheduleDetail, opt => opt.ExplicitExpansion())
                .ForMember(x => x.User, opt => opt.ExplicitExpansion());
            CreateMap<DataAccess.Entities.Package, PackageDTO>();
            CreateMap<DataAccess.Entities.ApplicationUser, MemberDTO>()
                .ForMember(x => x.RoleNames, opt => opt.MapFrom<ApplicationUserRolesNameResolver>());
            CreateMap<DataAccess.Entities.UserRole, UserRoleDTO>();
            CreateMap<DataAccess.Entities.Role, RoleDTO>();
        }
    }
}
