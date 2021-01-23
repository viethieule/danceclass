using AutoMapper;
using Services.Package;
using Services.Members;
using Services.Registration;
using Services.Schedule;
using Services.Membership;
using Services.Branch;
using Services.Utils;

namespace Services.Common.AutoMapper
{
    public class DtoToEntityMappingProfile : Profile
    {
        public DtoToEntityMappingProfile()
        {
            CreateMap<PackageDTO, DataAccess.Entities.Package>();
            CreateMap<MemberDTO, DataAccess.Entities.ApplicationUser>()
                .ForMember(x => x.NormalizedFullName, opt => opt.MapFrom(source => source.FullName.NormalizeVietnameseDiacritics()));
            CreateMap<RegistrationDTO, DataAccess.Entities.Registration>();
            CreateMap<ScheduleDTO, DataAccess.Entities.Schedule>()
                .ForMember(x => x.SessionsPerWeek, opt => opt.MapFrom(source => !string.IsNullOrEmpty(source.DaysPerWeek) ? source.DaysPerWeek.Length : default));
            CreateMap<BranchDTO, DataAccess.Entities.Branch>();
        }
    }
}
