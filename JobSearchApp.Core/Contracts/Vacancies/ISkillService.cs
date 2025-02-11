using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ISkillService
{
    Task<List<Skill>> GetAllSkills();
    Task<Skill> GetSkillById(int id);
    Task<Skill> CreateSkill(SkillCreateDto skill);
    Task<Skill> UpdateSkill(SkillUpdateDto skill);
    Task DeleteSkill(int id);
    Task DeleteManySkills(int[] skills);
}