using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Profiles;

namespace JobSearchApp.Core.Models.Profiles;

public class GetRecruiterProfileDto : ProfileDto<RecruiterProfile>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly DateBirth { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PositionTitle { get; set; }
    public bool IsActive { get; set; }
    public int UserId { get; set; }
    public Company? Company { get; set; }
}