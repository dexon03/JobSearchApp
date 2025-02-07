using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Mapper;

public class CommonProfile : Profile
{
    public CommonProfile()
    {
        CreateMap<CompanyCreateDto, Company>();
        CreateMap<CompanyUpdateDto, Company>();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();
        CreateMap<LocationCreateDto, Location>();
        CreateMap<LocationUpdateDto, Location>();
        CreateMap<SkillCreateDto, Skill>();
        CreateMap<SkillUpdateDto, Skill>();
    }
}