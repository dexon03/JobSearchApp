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