using AutoMapper;

namespace Services.Common.AutoMapper
{
    public class EntityToEntityMappingProfile : Profile
    {
        public EntityToEntityMappingProfile()
        {
            CreateMap<DataAccess.Entities.Schedule, DataAccess.Entities.Schedule>()
                .ForMember(x => x.SessionsPerWeek, opt => opt.MapFrom(source => !string.IsNullOrEmpty(source.DaysPerWeek) ? source.DaysPerWeek.Length : default))
                .ForMember(x => x.ScheduleDetails, opt => opt.Ignore());
        }
    }
}
