using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Data.Models.Vacancies;

public class LocationVacancy
{
    public int LocationId { get; set; }
    public int VacancyId { get; set; }
    public Location Location { get; set; } = null!;
    public Vacancy Vacancy { get; set; } = null!;
}