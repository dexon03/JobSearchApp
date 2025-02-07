using AutoMapper.Configuration.Annotations;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Profiles;
using Microsoft.AspNetCore.Http;

namespace JobSearchApp.Core.Models.Profiles;

public class CandidateProfileUpdateDto : ProfileUpdateDto<CandidateProfile>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly DateBirth { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? PositionTitle { get; set; }
    public bool IsActive { get; set; } = false;
    public Experience WorkExperience { get; set; }
    public double DesiredSalary { get; set; }
    public AttendanceMode Attendance { get; set; }
    [Ignore]
    public IFormFile? PdfResume { get; set; }
    public IEnumerable<SkillDto>? Skills { get; set; }
    public IEnumerable<LocationDto>? Locations { get; set; }
}
