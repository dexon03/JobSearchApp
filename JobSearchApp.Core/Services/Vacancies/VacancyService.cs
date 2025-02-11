using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class VacancyService(AppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger)
    : IVacancyService
{
    private readonly ILogger _log = logger.ForContext<VacancyService>();

    public async Task<List<VacancyGetAllDto>> GetAllVacancies(VacancyFilterParameters vacancyFilter)
    {
        var cacheKey =
            $"all_vacancies_{vacancyFilter.Page}_{vacancyFilter.PageSize}_{vacancyFilter.SearchTerm}_{vacancyFilter.Experience}_{vacancyFilter.AttendanceMode}_{vacancyFilter.Skill}_{vacancyFilter.Category}_{vacancyFilter.Location}";
        var cacheTag = "vacancies";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching all active vacancies from DB...", cacheKey);
                var vacancies = await GetAllVacanciesByCondition(v => v.IsActive, vacancyFilter);
                _log.Information("Fetched {Count} vacancies from DB.", vacancies.Count);
                return vacancies;
            },
            tags: [cacheTag]
        );
    }

    public async Task<VacancyGetDto> GetVacancyById(int id)
    {
        var cacheKey = $"vacancy_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching vacancy {VacancyId} from DB...", cacheKey, id);
                var vacancy = await db.Vacancies
                    .Where(v => v.Id == id)
                    .Include(v => v.Category)
                    .Include(v => v.Company)
                    .Include(v => v.LocationVacancy).ThenInclude(lv => lv.Location)
                    .Include(v => v.VacancySkill).ThenInclude(vs => vs.Skill)
                    .Select(v => mapper.Map<VacancyGetDto>(v))
                    .FirstOrDefaultAsync(ctx);

                if (vacancy == null)
                {
                    _log.Warning("Vacancy {VacancyId} not found.", id);
                    throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
                }

                return vacancy;
            },
            tags: [$"vacancy_{id}"]
        );
    }

    public async Task<List<VacancyGetAllDto>> GetVacanciesByRecruiterId(int recruiterId,
        VacancyFilterParameters vacancyFilter)
    {
        var cacheKey = $"vacancies_recruiter_{recruiterId}_{vacancyFilter.Page}_{vacancyFilter.PageSize}";
        var cacheTag = $"vacancies_recruiter_{recruiterId}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching vacancies for recruiter {RecruiterId}...",
                    cacheKey, recruiterId);
                var vacancies = await GetAllVacanciesByCondition(v => v.RecruiterId == recruiterId, vacancyFilter);
                _log.Information("Fetched {Count} vacancies for recruiter {RecruiterId}.", vacancies.Count,
                    recruiterId);
                return vacancies;
            },
            tags: [cacheTag]
        );
    }

    public async Task<Vacancy> CreateVacancy(VacancyCreateDto vacancyDto)
    {
        var vacancy = mapper.Map<Vacancy>(vacancyDto);
        vacancy.CreatedAt = DateTime.UtcNow;

        var result = db.Vacancies.Add(vacancy);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}");
        _log.Information("New vacancy {VacancyId} created. Cache invalidated.", vacancy.Id);
        return result.Entity;
    }

    public async Task<Vacancy> UpdateVacancy(VacancyUpdateDto vacancy)
    {
        var vacancyEntity = await db.Vacancies.FindAsync(vacancy.Id);
        if (vacancyEntity is null)
        {
            _log.Warning("Attempt to update non-existing vacancy {VacancyId}.", vacancy.Id);
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        vacancyEntity = mapper.Map(vacancy, vacancyEntity);
        vacancyEntity.UpdatedAt = DateTime.UtcNow;

        var result = db.Update(vacancyEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"vacancy_{vacancy.Id}");
        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancyEntity.RecruiterId}");
        _log.Information("Vacancy {VacancyId} updated. Cache invalidated.", vacancyEntity.Id);
        return result.Entity;
    }

    public async Task DeleteVacancy(int id)
    {
        var vacancy = await db.Vacancies.FindAsync(id);
        if (vacancy == null)
        {
            _log.Warning("Attempt to delete non-existing vacancy {VacancyId}.", id);
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        db.Vacancies.Remove(vacancy);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"vacancy_{id}");
        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}");
        _log.Information("Vacancy {VacancyId} deleted. Cache invalidated.", id);
    }

    public async Task ActivateDeactivateVacancy(int id)
    {
        var vacancy = await db.Vacancies.FindAsync(id);
        if (vacancy == null)
        {
            _log.Warning("Attempt to toggle activation for non-existing vacancy {VacancyId}.", id);
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        vacancy.IsActive = !vacancy.IsActive;
        vacancy.UpdatedAt = DateTime.UtcNow;
        db.Update(vacancy);

        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"vacancy_{id}");
        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}");
        _log.Information("Vacancy {VacancyId} activation toggled. Cache invalidated.", id);
    }

    private async Task<List<VacancyGetAllDto>> GetAllVacanciesByCondition(Expression<Func<Vacancy, bool>> predicate,
        VacancyFilterParameters vacancyFilter)
    {
        var vacancyQuery = db.Vacancies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(vacancyFilter.SearchTerm))
        {
            var searchTerm = vacancyFilter.SearchTerm.ToLower();
            vacancyQuery = vacancyQuery.Where(v =>
                v.Title.ToLower().Contains(searchTerm) ||
                v.Description.ToLower().Contains(searchTerm));
        }

        if (vacancyFilter.Experience is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.Experience == vacancyFilter.Experience);
        }

        if (vacancyFilter.AttendanceMode is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.AttendanceMode == vacancyFilter.AttendanceMode);
        }

        if (vacancyFilter.Skill is not null)
        {
            vacancyQuery = vacancyQuery.Include(v => v.VacancySkill).ThenInclude(vs => vs.Skill)
                .Where(v => v.VacancySkill.Any(vs => vs.SkillId == vacancyFilter.Skill));
        }

        if (vacancyFilter.Category is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.CategoryId == vacancyFilter.Category);
        }

        if (vacancyFilter.Location is not null)
        {
            vacancyQuery = vacancyQuery.Include(v => v.LocationVacancy).ThenInclude(lv => lv.Location)
                .Where(v => v.LocationVacancy.Any(lv => lv.LocationId == vacancyFilter.Location));
        }

        var vacancies = await vacancyQuery
            .Where(predicate)
            .OrderBy(v => v.CreatedAt)
            .Select(v => new VacancyGetAllDto
            {
                Id = v.Id,
                Title = v.Title,
                Salary = v.Salary,
                Experience = v.Experience.ToString(),
                CreatedAt = v.CreatedAt,
                AttendanceMode = v.AttendanceMode.ToString(),
                Description = v.Description,
                CompanyName = v.Company.Name,
                IsActive = v.IsActive,
                Locations = v.LocationVacancy
                    .Select(lv => new LocationDto
                    {
                        City = lv.Location.City,
                        Country = lv.Location.Country
                    })
                    .Distinct()
                    .ToList()
            })
            .Skip((vacancyFilter.Page - 1) * vacancyFilter.PageSize)
            .Take(vacancyFilter.PageSize)
            .AsNoTracking()
            .ToListAsync();


        return vacancies;
    }
}