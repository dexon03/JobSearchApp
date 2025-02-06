using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Vacancies;

public class VacancyUpdateDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public double Salary { get; set; }
    public Experience Experience { get; set; }
    public AttendanceMode AttendanceMode { get; set; }
    public List<LocationDto> Locations { get; set; }
    public List<SkillDto> Skills { get; set; }
}