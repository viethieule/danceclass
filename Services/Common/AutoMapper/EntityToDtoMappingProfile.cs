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
            CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>();
            CreateMap<DataAccess.Entities.ScheduleDetail, ScheduleDetailDTO>()
                .ForMember(x => x.TotalRegistered, opt => opt.MapFrom(m => m.Registrations.Count));
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
