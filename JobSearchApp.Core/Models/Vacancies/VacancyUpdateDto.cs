using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Vacancies;

public class VacancyUpdateDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Salary { get; set; }
    public Experience Experience { get; set; }
    public AttendanceMode AttendanceMode { get; set; }
    public List<LocationDto> Locations { get; set; } = null!;
    public List<SkillDto> Skills { get; set; } = null!;
}