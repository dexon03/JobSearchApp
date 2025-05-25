using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Vacancies;
using Pgvector;

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
                        VacancyId = dest.Id,
                        SkillId = x.Id
                    })))
            .ForMember(x => x.LocationVacancy,
                opt =>
                    opt.MapFrom((source, dest) => source.Locations.Select(x => new LocationVacancy
                    {
                        VacancyId = dest.Id,
                        LocationId = x.Id
                    })))
            .ForMember(x => x.Embedding, opt => opt.MapFrom<Vector?>(x => null));
        CreateMap<VacancyCreateDto, Vacancy>()
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
                    })))
            .ForMember(x => x.CreatedAt, opt => opt.MapFrom(x => DateTime.UtcNow));

        CreateMap<Vacancy, VacancyGetDto>()
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.LocationVacancy))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.VacancySkill));

        CreateMap<LocationVacancy, LocationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Location.Id))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Location.City))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Location.Country))
            .ReverseMap();

        CreateMap<VacancySkill, SkillDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Skill.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Skill.Name))
            .ReverseMap();

        CreateMap<Vacancy, VacancyGetAllDto>()
            .ForMember(x => x.CompanyName, opt => opt.MapFrom(x => x.Company.Name))
            .ForMember(x => x.Experience, opt => opt.MapFrom(x => x.Experience.ToString()))
            .ForMember(x => x.AttendanceMode, opt => opt.MapFrom(x => x.AttendanceMode.ToString()))
            .ForMember(x => x.Locations, opt => opt.MapFrom(x => x.LocationVacancy.Select(t => new LocationDto
            {
                Id = t.Location.Id,
                City = t.Location.City,
                Country = t.Location.Country
            })));
    }
}