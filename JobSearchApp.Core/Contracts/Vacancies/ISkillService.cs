using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ISkillService
{
    Task<List<SkillDto>> GetAllSkills();
    Task<SkillDto> GetSkillById(int id);
    Task<SkillDto> CreateSkill(SkillCreateDto skill);
    Task<SkillDto> UpdateSkill(SkillUpdateDto skill);
    Task DeleteSkill(int id);
    Task DeleteManySkills(int[] skills);
}