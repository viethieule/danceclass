using AutoMapper;
using Services.Class;
using Services.MemberPackage;
using Services.Members;
using Services.Package;
using Services.Registration;
using Services.Schedule;
using Services.Trainer;
using System.Linq;

namespace Services.Common.AutoMapper
{
    public class EntityToDtoMappingProfile : Profile
    {
        public EntityToDtoMappingProfile()
        {
            CreateMap<DataAccess.Entities.Schedule, ScheduleDTO>()
                .ForMember(x => x.ScheduleDetails, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Trainer, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Class, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.ScheduleDetail, ScheduleDetailDTO>()
                .ForMember(x => x.TotalRegistered, opt => opt.MapFrom(m => m.Registrations.Count()))
                .ForMember(x => x.Registrations, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Schedule, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.Trainer, TrainerDTO>()
                .ForMember(x => x.Schedules, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.Class, ClassDTO>()
                .ForMember(x => x.Schedules, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.Registration, RegistrationDTO>()
                .ForMember(x => x.ScheduleDetail, opt => opt.ExplicitExpansion())
                .ForMember(x => x.User, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.Package, PackageDTO>();

            CreateMap<DataAccess.Entities.ApplicationUser, MemberDTO>()
                .ForMember(x => x.MemberPackages, opt => opt.ExplicitExpansion())
                .ForMember(x => x.ActivePackage, opt => opt.MapFrom(x => x.MemberPackages.FirstOrDefault(m => m.IsActive)));

            CreateMap<DataAccess.Entities.MemberPackage, MemberPackageDTO>()
                .ForMember(x => x.User, opt => opt.ExplicitExpansion())
                .ForMember(x => x.Package, opt => opt.ExplicitExpansion());

            CreateMap<DataAccess.Entities.UserRole, UserRoleDTO>();

            CreateMap<DataAccess.Entities.Role, RoleDTO>();
        }
    }
}
