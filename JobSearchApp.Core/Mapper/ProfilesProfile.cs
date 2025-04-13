using AutoMapper;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Profiles;

namespace JobSearchApp.Core.Mapper;

public class ProfilesProfile : Profile
{
    public ProfilesProfile()
    {
        CreateMap<RecruiterProfileUpdateDto, RecruiterProfile>();
        CreateMap<CandidateProfileUpdateDto, CandidateProfile>().ForMember(x => x.ProfileSkills,
                opt =>
                    opt.MapFrom((source, dest) => source.Skills.Select(x => new ProfileSkills
                    {
                        Profile = dest,
                        SkillId = x.Id
                    })))
            .ForMember(x => x.LocationProfiles,
                opt =>
                    opt.MapFrom((source, dest) => source.Locations.Select(x => new LocationProfile()
                    {
                        Profile = dest,
                        LocationId = x.Id
                    })));

        CreateMap<LocationProfile, LocationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Location.Id))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Location.City))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Location.Country))
            .ReverseMap();

        CreateMap<ProfileSkills, SkillDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Skill.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Skill.Name))
            .ReverseMap();


        CreateMap<CandidateProfile, GetCandidateProfileDto>()
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => src.LocationProfiles))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ProfileSkills));

        CreateMap<RecruiterProfile, GetRecruiterProfileDto>()
            .ForMember(x => x.Company, opt => opt.MapFrom(src => src.Company));
    }
}