using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Identity;

public class RegisterDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public Role Role { get; set; }
}