using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Vacancies;

public class VacancyFilterParameters
{
    public string? SearchTerm { get; set; } = "";
    public int Page { get; set; }
    public int PageSize { get; set; }
    public Experience? Experience { get; set; } = null;
    public AttendanceMode? AttendanceMode { get; set; } = null;
    public int? Skill { get; set; } = null;
    public int? Category { get; set; } = null;
    public int? Location { get; set; } = null;
}