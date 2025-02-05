namespace JobSearchApp.Data.Models.Vacancies;

public class Company
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    public required string Description { get; set; } = null!;
}