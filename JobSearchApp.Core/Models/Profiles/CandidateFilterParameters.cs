using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Profiles;

public record CandidateFilterParameters
{
    public string? SearchTerm { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public Experience? Experience { get; set; }
    public AttendanceMode? AttendanceMode { get; set; }
    public int? Skill { get; set; }
    public int? Location { get; set; }
}