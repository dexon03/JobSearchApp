using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Profiles;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Core.Models.Profiles;

public class ProfileCreateDto
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string PositionTitle { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public Role Role { get; set; }
}

public static class ProfileMapperExtension
{
    public static CandidateProfile MapCreateToCandidateProfile(this CandidateProfile profile,
        ProfileCreateDto profileCreateDto)
    {
        return new CandidateProfile
        {
            UserId = profileCreateDto.UserId,
            Name = profileCreateDto.Name,
            Surname = profileCreateDto.Surname,
            PositionTitle = profileCreateDto.PositionTitle,
            Email = profileCreateDto.Email,
            PhoneNumber = profileCreateDto.PhoneNumber,
            WorkExperience = Experience.NoExperience
        };
    }

    public static RecruiterProfile MapCreateToRecruiterProfile(this RecruiterProfile profile,
        ProfileCreateDto profileCreateDto)
    {
        return new RecruiterProfile
        {
            UserId = profileCreateDto.UserId,
            Name = profileCreateDto.Name,
            Surname = profileCreateDto.Surname,
            Email = profileCreateDto.Email,
            PhoneNumber = profileCreateDto.PhoneNumber,
        };
    }
}