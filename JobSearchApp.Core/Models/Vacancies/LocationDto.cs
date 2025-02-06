namespace JobSearchApp.Core.Models.Vacancies;

public record LocationDto
{
    public int Id { get; set; }
    public string Country { get; set; } = null!;
    public string City { get; set; } = null!;
};
