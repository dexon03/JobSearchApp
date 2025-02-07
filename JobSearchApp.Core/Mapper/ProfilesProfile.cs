using AutoMapper;
using JobSearchApp.Core.Models.Profiles;
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
        ;
    }
}