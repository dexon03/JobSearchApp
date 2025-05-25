using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Vacancies;

public class AiVacancyDescriptionRequest
{
    public Experience Experience { get; set; }
    public string Position { get; set; }  = null!;
    public string? Description { get; set; }
}