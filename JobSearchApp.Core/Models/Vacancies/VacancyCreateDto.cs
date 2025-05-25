using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Vacancies;

public class VacancyCreateDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Salary { get; set; }
    public AttendanceMode AttendanceMode { get; set; }
    public Experience Experience { get; set; }
    public bool IsActive { get; set; } = true;
    public int RecruiterId { get; set; }
    public int CategoryId { get; set; }
    public int CompanyId { get; set; }
    public List<LocationDto> Locations { get; set; } = [];
    public List<SkillDto> Skills { get; set; } = [];
}