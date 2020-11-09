using AutoMapper;

namespace Services.Common.AutoMapper
{
    public class EntityToEntityMappingProfile : Profile
    {
        public EntityToEntityMappingProfile()
        {
            CreateMap<DataAccess.Entities.Schedule, DataAccess.Entities.Schedule>()
                .ForMember(x => x.SessionsPerWeek, opt => opt.MapFrom(source => !string.IsNullOrEmpty(source.DaysPerWeek) ? source.DaysPerWeek.Length : default))
                .ForMember(x => x.ScheduleDetails, opt => opt.Ignore())
                .ForMember(x => x.Trainer, opt => opt.Ignore())
                .ForMember(x => x.TrainerId, opt => opt.Ignore())
                .ForMember(x => x.Class, opt => opt.Ignore())
                .ForMember(x => x.ClassId, opt => opt.Ignore())
                .ForMember(x => x.CreatedDate, opt => opt.Ignore())
                .ForMember(x => x.UpdatedDate, opt => opt.Ignore());
        }
    }
}
