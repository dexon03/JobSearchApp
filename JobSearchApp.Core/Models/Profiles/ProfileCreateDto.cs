using JobSearchApp.Data.Models;

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