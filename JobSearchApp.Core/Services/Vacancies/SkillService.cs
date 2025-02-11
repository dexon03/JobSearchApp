using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class SkillService(AppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger) : ISkillService
{
    private readonly ILogger _log = logger.ForContext<SkillService>();

    public async Task<List<Skill>> GetAllSkills()
    {
        var cacheKey = "all_skills";
        var cacheTag = "skills";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching all skills from DB...", cacheKey);
                var skills = await db.Skills.ToListAsync(ctx);
                _log.Information("Fetched {Count} skills from DB.", skills.Count);
                return skills;
            },
            tags: [cacheTag]
        );
    }

    public async Task<Skill> GetSkillById(int id)
    {
        var cacheKey = $"skill_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching skill {SkillId} from DB...", cacheKey, id);
                var skill = await db.Skills.FindAsync(new object[] { id }, ctx);
                if (skill == null)
                {
                    _log.Warning("Skill {SkillId} not found.", id);
                    throw new Exception($"Skill with id {id} not found");
                }

                return skill;
            },
            tags: [$"skill_{id}"]
        );
    }

    public async Task<Skill> CreateSkill(SkillCreateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        var result = db.Skills.Add(skillEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("skills");
        _log.Information("New skill {SkillName} created. Cache invalidated.", skill.Name);
        return result.Entity;
    }

    public async Task<Skill> UpdateSkill(SkillUpdateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        var isExists = await db.Skills.AnyAsync(x => x.Id == skillEntity.Id);
        if (!isExists)
        {
            _log.Warning("Attempt to update non-existing skill {SkillId}.", skillEntity.Id);
            throw new Exception("Skill not found");
        }

        var result = db.Update(skillEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"skill_{skillEntity.Id}");
        await hybridCache.RemoveByTagAsync("skills");

        _log.Information("Skill {SkillId} updated. Cache invalidated.", skillEntity.Id);
        return result.Entity;
    }

    public async Task DeleteSkill(int id)
    {
        var skill = await db.Skills.FindAsync(id);
        if (skill == null)
        {
            _log.Warning("Attempt to delete non-existing skill {SkillId}.", id);
            throw new Exception($"Skill not found");
        }

        db.Skills.Remove(skill);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"skill_{id}");
        await hybridCache.RemoveByTagAsync("skills");

        _log.Information("Skill {SkillId} deleted. Cache invalidated.", id);
    }

    public async Task DeleteManySkills(Skill[] skills)
    {
        db.Skills.RemoveRange(skills);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("skills");
        _log.Information("Multiple skills deleted. Cache invalidated.");
    }
}