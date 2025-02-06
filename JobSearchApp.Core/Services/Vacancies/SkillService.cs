using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class SkillService(AppDbContext db, IMapper mapper) : ISkillService
{
    public Task<List<Skill>> GetAllSkills()
    {
        return db.Skills.ToListAsync();
    }

    public async Task<Skill> GetSkillById(int id)
    {
        var skill = await db.Skills.FindAsync(id);
        if (skill == null)
        {
            throw new Exception($"Skill with id {id} not found");
        }

        return skill;
    }

    public async Task<Skill> CreateSkill(SkillCreateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        var result = db.Skills.Add(skillEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Skill> UpdateSkill(SkillUpdateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        var isExists = await db.Skills.AnyAsync(x => x.Id == skillEntity.Id);
        if (!isExists)
        {
            throw new Exception("Skill not found");
        }

        var result = db.Update(skillEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteSkill(int id)
    {
        var skill = await db.Skills.FindAsync(id);
        if (skill == null)
        {
            throw new Exception($"Skill not found");
        }

        db.Skills.Remove(skill);
        await db.SaveChangesAsync();
    }

    public Task DeleteManySkills(Skill[] skills)
    {
        db.Skills.RemoveRange(skills);
        return db.SaveChangesAsync();
    }
}