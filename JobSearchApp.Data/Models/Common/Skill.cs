using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Data.Models.Common;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<VacancySkill> VacancySkill { get; set; } = [];
    public virtual ICollection<ProfileSkills> ProfileSkills { get; set; } = [];
}