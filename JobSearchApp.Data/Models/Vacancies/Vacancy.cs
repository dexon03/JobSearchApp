using System.ComponentModel.DataAnnotations.Schema;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Profiles;
using Pgvector;

namespace JobSearchApp.Data.Models.Vacancies;

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public AttendanceMode AttendanceMode { get; set; }
    public Experience Experience { get; set; }
    public double Salary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int CategoryId { get; set; }
    public int CompanyId { get; set; }
    public int RecruiterId { get; set; }

    public RecruiterProfile Recruiter { get; set; }
    public Category? Category { get; set; }
    public Company? Company { get; set; }
    [Column(TypeName = "vector(768)")]
    public Vector? Embedding { get; set; }
    public virtual ICollection<LocationVacancy> LocationVacancy { get; set; } = [];
    public virtual ICollection<VacancySkill> VacancySkill { get; set; } = [];
    public virtual ICollection<VacancyUser> VacancyUsers { get; set; } = [];
}