namespace JobSearchApp.Core.Models.Vacancies;

public class CompanyUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}