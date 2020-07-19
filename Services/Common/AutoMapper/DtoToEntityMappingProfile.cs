using AutoMapper;
using Services.Members;
using Services.Package;
using Services.Registration;

namespace Services.Common.AutoMapper
{
    public class DtoToEntityMappingProfile : Profile
    {
        public DtoToEntityMappingProfile()
        {
            CreateMap<PackageDTO, DataAccess.Entities.Package>();
            CreateMap<MemberDTO, DataAccess.Entities.ApplicationUser>();
            CreateMap<RegistrationDTO, DataAccess.Entities.Registration>();
        }
    }
}
