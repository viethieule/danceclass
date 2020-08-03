using AutoMapper;
using Services.Members;
using Services.Package;
using Services.Registration;
using Services.Schedule;

namespace Services.Common.AutoMapper
{
    public class DtoToEntityMappingProfile : Profile
    {
        public DtoToEntityMappingProfile()
        {
            CreateMap<PackageDTO, DataAccess.Entities.Package>();
            CreateMap<MemberDTO, DataAccess.Entities.ApplicationUser>();
            CreateMap<RegistrationDTO, DataAccess.Entities.Registration>();
            CreateMap<ScheduleDTO, DataAccess.Entities.Schedule>()
                .ForMember(x => x.SessionsPerWeek, opt => opt.MapFrom(source => !string.IsNullOrEmpty(source.DaysPerWeek) ? source.DaysPerWeek.Length : default));
        }
    }
}
