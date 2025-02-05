using System.Text.Json.Serialization;

namespace JobSearchApp.Data.Models.Vacancies;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<VacancySkill> VacancySkill { get; set; } = [];
}