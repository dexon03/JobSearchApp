using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Data.Models.Vacancies;

public class VacancySkill
{
    public int VacancyId { get; set; }
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public Vacancy Vacancy { get; set; } = null!;
}