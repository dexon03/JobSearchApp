using JobSearchApp.Data.Models.Profiles;

namespace JobSearchApp.Core.Models.Profiles;

public class RecruiterProfileUpdateDto : ProfileUpdateDto<RecruiterProfile>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly DateBirth { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? PositionTitle { get; set; }
    public bool IsActive { get; set; } = false;
    public int? CompanyId { get; set; }
}