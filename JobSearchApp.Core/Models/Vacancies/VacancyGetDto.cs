using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Models.Vacancies;

public record VacancyGetDto
{
    public int Id { get; set; }
    public int RecruiterId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public AttendanceMode? AttendanceMode { get; set; }
    public Experience? Experience { get; set; }
    public double? Salary { get; set; }
    public bool IsActive { get; set; }
    public Company Company { get; set; } = null!;
    public Category? Category { get; set; } = null!;
    public IEnumerable<LocationDto>? Locations { get; set; }
    public IEnumerable<SkillDto>? Skills { get; set; }
};