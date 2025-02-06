namespace JobSearchApp.Core.Models.Vacancies;

public class GenerateVacancyDescription
{
    public string Title { get; set; } = null!;
    public string VacancyShortDescription { get; set; } = null!;
    public string CompanyDescription { get; set; }
}