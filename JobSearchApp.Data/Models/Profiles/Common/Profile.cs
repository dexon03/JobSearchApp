namespace JobSearchApp.Data.Models.Profiles.Common;

public class Profile<T>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly DateBirth { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? PositionTitle { get; set; }
    public bool IsActive { get; set; } = true;
}

// public static class ProfileMapperExtension
// {
//     public static CandidateProfile MapCreateToCandidateProfile(this CandidateProfile profile,
//         ProfileCreateDto profileCreateDto)
//     {
//         return new CandidateProfile
//         {
//             UserId = profileCreateDto.UserId,
//             Name = profileCreateDto.Name,
//             Surname = profileCreateDto.Surname,
//             PositionTitle = profileCreateDto.PositionTitle,
//             Email = profileCreateDto.Email,
//             PhoneNumber = profileCreateDto.PhoneNumber,
//             WorkExperience = Experience.NoExperience
//         };
//     }
//
//     public static RecruiterProfile MapCreateToRecruiterProfile(this RecruiterProfile profile,
//         ProfileCreateDto profileCreateDto)
//     {
//         return new RecruiterProfile
//         {
//             UserId = profileCreateDto.UserId,
//             Name = profileCreateDto.Name,
//             Surname = profileCreateDto.Surname,
//             Email = profileCreateDto.Email,
//             PhoneNumber = profileCreateDto.PhoneNumber,
//         };
//     }
// }