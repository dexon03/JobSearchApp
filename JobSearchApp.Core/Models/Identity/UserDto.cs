using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Identity;

public record UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public Role? Role { get; set; }
};