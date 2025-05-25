using JobSearchApp.Data.Models;

namespace JobSearchApp.Core.Models.Identity;

public record UpdateUserRequest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; }  = null!;
    public required Role Role { get; set; }
}