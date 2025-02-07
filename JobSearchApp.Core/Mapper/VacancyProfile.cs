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
    }
}