using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Common;

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

    public User Recruiter { get; set; }
    public Category? Category { get; set; }
    public Company? Company { get; set; }
    public float[] Embedding { get; set; }
    public virtual ICollection<LocationVacancy> LocationVacancy { get; set; } = [];
    public virtual ICollection<VacancySkill> VacancySkill { get; set; } = [];
    public virtual ICollection<VacancyUser> VacancyUsers { get; set; } = [];
}