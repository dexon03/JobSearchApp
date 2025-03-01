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

        CreateMap<CandidateProfile, GetCandidateProfileDto>()
            .ForMember(x => x.Locations,
                opt =>
                    opt.MapFrom((source, dest) => source.LocationProfiles.Select(x => new LocationDto
                    {
                        Id = x.Location.Id,
                        City = x.Location.City,
                        Country = x.Location.Country
                    })))
            .ForMember(x => x.Skills,
                opt =>
                    opt.MapFrom((source, dest) => source.ProfileSkills.Select(x => new SkillDto
                    {
                        Id = x.Skill.Id,
                        Name = x.Skill.Name
                    })));
        CreateMap<RecruiterProfile, GetRecruiterProfileDto>();
    }
}