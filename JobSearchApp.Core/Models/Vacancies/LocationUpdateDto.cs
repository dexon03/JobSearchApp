namespace JobSearchApp.Core.Models.Vacancies;

public class LocationUpdateDto
{
    public int Id { get; set; }
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
}