using AutoMapper;

using BarFoo.Domain.Entities;
using BarFoo.Infrastructure.DTOs;

namespace BarFoo.Infrastructure.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApiKey, ApiKeyDto>()
            .ForMember(dest => dest.Objectives, opt => opt.MapFrom(src => src.ApiKeyObjectives.Select(ao => ao.Objective)));

        CreateMap<ApiKeyDto, ApiKey>()
            .ForMember(dest => dest.ApiKeyObjectives, opt => opt.Ignore()) // Handled manually in the service
            .ForMember(dest => dest.HasBeenSyncedOnce, opt => opt.MapFrom(src => src.HasBeenSyncedOnce))
            .ForMember(dest => dest.LastSyncTime, opt => opt.MapFrom(src => src.LastSyncTime));

        CreateMap<Objective, ObjectiveDto>()
            .ForMember(dest => dest.ApiKeyName, opt => opt.Ignore());

        CreateMap<ObjectiveDto, Objective>()
            .ForMember(dest => dest.ApiKeyObjectives, opt => opt.Ignore());

        CreateMap<ApiKeyObjective, ObjectiveDto>()
            .IncludeMembers(src => src.Objective)
            .ForMember(dest => dest.ApiKeyName, opt => opt.MapFrom(src => src.ApiKey.Name));
    }
}
