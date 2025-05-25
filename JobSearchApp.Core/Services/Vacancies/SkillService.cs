using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class SkillService(IAppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger) : ISkillService
{
    private readonly ILogger _log = logger.ForContext<SkillService>();

    public async Task<List<SkillDto>> GetAllSkills()
    {
        var cacheKey = "all_skills";
        var cacheTag = "skills";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching all skills from DB...", cacheKey);
                var skills = await db.Skills.ProjectTo<SkillDto>(mapper.ConfigurationProvider).ToListAsync(ctx);
                _log.Information("Fetched {Count} skills from DB.", skills.Count);
                return skills;
            },
            tags: [cacheTag]
        );
    }

    public async Task<SkillDto> GetSkillById(int id)
    {
        var cacheKey = $"skill_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching skill {SkillId} from DB...", cacheKey, id);
                var skill = await db.Skills.FindAsync(id, ctx);
                if (skill == null)
                {
                    _log.Warning("Skill {SkillId} not found.", id);
                    throw new Exception($"Skill with id {id} not found");
                }

                var skillDto = mapper.Map<SkillDto>(skill);

                return skillDto;
            },
            tags: [$"skill_{id}"]
        );
    }

    public async Task<SkillDto> CreateSkill(SkillCreateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        db.Skills.Add(skillEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("skills");
        _log.Information("New skill {SkillName} created. Cache invalidated.", skill.Name);

        var skillDto = mapper.Map<SkillDto>(skillEntity);

        return skillDto;
    }

    public async Task<SkillDto> UpdateSkill(SkillUpdateDto skill)
    {
        var skillEntity = mapper.Map<Skill>(skill);
        var isExists = await db.Skills.AnyAsync(x => x.Id == skillEntity.Id);
        if (!isExists)
        {
            _log.Warning("Attempt to update non-existing skill {SkillId}.", skillEntity.Id);
            throw new Exception("Skill not found");
        }

        db.Skills.Update(skillEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"skill_{skillEntity.Id}");
        await hybridCache.RemoveByTagAsync("skills");

        _log.Information("Skill {SkillId} updated. Cache invalidated.", skillEntity.Id);

        var skillDto = mapper.Map<SkillDto>(skillEntity);

        return skillDto;
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

    public async Task DeleteManySkills(int[] skills)
    {
        await db.Skills.Where(x => skills.Contains(x.Id)).ExecuteDeleteAsync();

        await hybridCache.RemoveByTagAsync("skills");
        _log.Information("Multiple skills deleted. Cache invalidated.");
    }
}