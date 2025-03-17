using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Mapper;

public class VacancyProfile : Profile
{
    public VacancyProfile()
    {
        CreateMap<VacancyUpdateDto, Vacancy>()
            .ForMember(x => x.VacancySkill,
                opt =>
                    opt.MapFrom((source, dest) => source.Skills.Select(x => new VacancySkill
                    {
                        Vacancy = dest,
                        SkillId = x.Id
                    })))
            .ForMember(x => x.LocationVacancy,
                opt =>
                    opt.MapFrom((source, dest) => source.Locations.Select(x => new LocationVacancy
                    {
                        Vacancy = dest,
                        LocationId = x.Id
                    })));
        CreateMap<VacancyCreateDto, Vacancy>().ForMember(x => x.VacancySkill,
                opt =>
                    opt.MapFrom((source, dest) => source.Skills.Select(x => new VacancySkill
                    {
                        Vacancy = dest,
                        SkillId = x.Id
                    })))
            .ForMember(x => x.LocationVacancy,
                opt =>
                    opt.MapFrom((source, dest) => source.Locations.Select(x => new LocationVacancy
                    {
                        Vacancy = dest,
                        LocationId = x.Id
                    })));

        CreateMap<Vacancy, VacancyGetDto>()
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.LocationVacancy))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.VacancySkill));
            ;
        
        CreateMap<LocationVacancy, LocationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Location.Id))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Location.City))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Location.Country));

        CreateMap<VacancySkill, SkillDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Skill.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Skill.Name));

    }
}