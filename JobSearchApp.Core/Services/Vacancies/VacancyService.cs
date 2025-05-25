using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Serilog;
using X.PagedList;
using X.PagedList.EF;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class VacancyService(
    IAppDbContext db,
    IMapper mapper,
    IFusionCache hybridCache,
    ILogger logger,
    IChatClient chatClient,
    IEmbeddingService embeddingService,
    IPublishEndpoint publishEndpoint)
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
        var cacheKey =
            $"vacancies_recruiter_{recruiterId}_{vacancyFilter.Page}_{vacancyFilter.PageSize}_{vacancyFilter.Experience}_{vacancyFilter.Category}_{vacancyFilter.Location}_{vacancyFilter.AttendanceMode}_{vacancyFilter.Skill}";
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

    public async Task<VacancyGetDto> CreateVacancy(VacancyCreateDto vacancyDto)
    {
        var vacancy = mapper.Map<Vacancy>(vacancyDto);

        db.Vacancies.Add(vacancy);
        await db.SaveChangesAsync();

        await publishEndpoint.Publish(new VacancyCreatedEvent
        {
            Id = vacancy.Id
        });

        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync("recommended_vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}");
        _log.Information("New vacancy {VacancyId} created. Cache invalidated.", vacancy.Id);

        return mapper.Map<VacancyGetDto>(vacancy);
    }

    public async Task<VacancyGetDto> UpdateVacancy(VacancyUpdateDto vacancy)
    {
        var vacancyEntity = await db.Vacancies
            .Include(x => x.VacancySkill)
            .Include(x => x.LocationVacancy)
            .FirstOrDefaultAsync(x => x.Id == vacancy.Id);

        if (vacancyEntity is null)
        {
            _log.Warning("Attempt to update non-existing vacancy {VacancyId}.", vacancy.Id);
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        mapper.Map(vacancy, vacancyEntity);
        vacancyEntity.UpdatedAt = DateTime.UtcNow;

        db.Vacancies.Update(vacancyEntity);
        await db.SaveChangesAsync();

        await publishEndpoint.Publish(new VacancyUpdatedEvent
        {
            Id = vacancyEntity.Id
        });

        await hybridCache.RemoveByTagAsync($"vacancy_{vacancy.Id}");
        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync("recommended_vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancyEntity.RecruiterId}");
        _log.Information("Vacancy {VacancyId} updated. Cache invalidated.", vacancyEntity.Id);
        return mapper.Map<VacancyGetDto>(vacancyEntity);
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
        await hybridCache.RemoveByTagAsync("recommended_vacancies");
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
        db.Vacancies.Update(vacancy);

        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"vacancy_{id}");
        await hybridCache.RemoveByTagAsync("vacancies");
        await hybridCache.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}");
        _log.Information("Vacancy {VacancyId} activation toggled. Cache invalidated.", id);
    }

    public async Task<string> GenerateVacancyDescription(int userId, AiVacancyDescriptionRequest descriptionRequest)
    {
        var companyDescription = await db.RecruiterProfile.Where(x => x.UserId == userId)
            .Select(x => x.Company.Description)
            .FirstOrDefaultAsync();

        var prompt = $"""
                      Write a big and detailed vacancy description for the following position: {descriptionRequest.Position}.
                      Experience needed for position: {descriptionRequest.Experience}.
                      The company is {companyDescription}. If this company description contains violent or not-related info, then ignore it.
                      The description should be clear, concise, and attractive to potential candidates.
                      Here is basic description: {descriptionRequest.Description}. If this description contains violent or not-related info, then ignore it.
                      !!!IMPORTANT!!!
                      Give me only this description. Dont need to add comments. Return this description in Markdown format.
                      !!!IMPORTANT!!!
                      """;
        var systemMessage = new ChatMessage(ChatRole.System,
            "You are a recruiter. Write a  meaningful and attractive description for vacancy for the following position.");
        var message = new ChatMessage(ChatRole.User, prompt);

        var response = await chatClient.GetResponseAsync([systemMessage, message]);

        return response.Text;
    }

    public async Task<IPagedList<VacancyGetAllDto>> GetRecommendedVacanciesAsync(int userId,
        VacancyFilterParameters vacancyFilter)
    {
        // var cacheKey =
        //     $"recommended_vacancies_{userId}_{vacancyFilter.Page}_{vacancyFilter.PageSize}_{vacancyFilter.SearchTerm}_{vacancyFilter.Experience}_{vacancyFilter.AttendanceMode}_{vacancyFilter.Skill}_{vacancyFilter.Category}_{vacancyFilter.Location}";
        // var cacheTag = "recommended_vacancies";
        //
        // return await hybridCache.GetOrSetAsync(
        //     cacheKey,
        //     async ctx =>
        //     {
        // _log.Information("Cache miss for {CacheKey}. Fetching recommended vacancies for user {UserId}...",
        //     cacheKey, userId);

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var candidate = await db.CandidateProfile
            .SingleOrDefaultAsync(x => x.UserId == userId);
        if (candidate == null)
        {
            _log.Warning("Candidate profile not found for user {UserId}.", userId);
            throw new ExceptionWithStatusCode("Candidate profile not found", HttpStatusCode.BadRequest);
        }

        var recommendedVacanciesQuery = db.Vacancies
            .Where(v => v.IsActive);

        recommendedVacanciesQuery = ApplyFilterIfNeeded(vacancyFilter, recommendedVacanciesQuery);

        if (candidate.Embedding is null)
        {
            var mappedRecommendedVacancies =
                recommendedVacanciesQuery.ProjectTo<VacancyGetAllDto>(mapper.ConfigurationProvider);

            var result =
                await mappedRecommendedVacancies.ToPagedListAsync(vacancyFilter.Page, vacancyFilter.PageSize);
            _log.Information("Fetched {Count} recommended vacancies for user {UserId} (no embedding).",
                result.Count, userId);
            return result;
        }

        var recommendedVacancies = await recommendedVacanciesQuery
            .OrderBy(x => candidate.Embedding.CosineDistance(x.Embedding!))
            .ProjectTo<VacancyGetAllDto>(mapper.ConfigurationProvider)
            .ToPagedListAsync(vacancyFilter.Page, vacancyFilter.PageSize);

        _log.Information("Fetched {Count} recommended vacancies for user {UserId} using vector similarity.",
            recommendedVacancies.Count, userId);
        return recommendedVacancies;
        // },
        // tags: [cacheTag]
        // );
    }

    private static IQueryable<Vacancy> ApplyFilterIfNeeded(VacancyFilterParameters vacancyFilter,
        IQueryable<Vacancy> recommendedVacanciesQuery)
    {
        if (vacancyFilter.Experience is not null)
        {
            recommendedVacanciesQuery = recommendedVacanciesQuery.Where(v => v.Experience == vacancyFilter.Experience);
        }

        if (vacancyFilter.AttendanceMode is not null)
        {
            recommendedVacanciesQuery =
                recommendedVacanciesQuery.Where(v => v.AttendanceMode == vacancyFilter.AttendanceMode);
        }

        if (vacancyFilter.Skill is not null)
        {
            recommendedVacanciesQuery = recommendedVacanciesQuery.Include(v => v.VacancySkill)
                .ThenInclude(vs => vs.Skill)
                .Where(v => v.VacancySkill.Any(vs => vs.SkillId == vacancyFilter.Skill));
        }

        if (vacancyFilter.Category is not null)
        {
            recommendedVacanciesQuery = recommendedVacanciesQuery.Where(v => v.CategoryId == vacancyFilter.Category);
        }

        if (vacancyFilter.Location is not null)
        {
            recommendedVacanciesQuery = recommendedVacanciesQuery.Include(v => v.LocationVacancy)
                .ThenInclude(lv => lv.Location)
                .Where(v => v.LocationVacancy.Any(lv => lv.LocationId == vacancyFilter.Location));
        }

        return recommendedVacanciesQuery;
    }

    private async Task<List<VacancyGetAllDto>> GetAllVacanciesByCondition(Expression<Func<Vacancy, bool>> predicate,
        VacancyFilterParameters vacancyFilter)
    {
        var vacancyQuery = db.Vacancies.Where(predicate);

        if (!string.IsNullOrWhiteSpace(vacancyFilter.SearchTerm))
        {
            var searchTerm = vacancyFilter.SearchTerm.ToLower();

            try
            {
                var searchEmbedding = await GetSearchEmbedding(vacancyFilter.SearchTerm);

                if (searchEmbedding != null)
                {
                    vacancyQuery = vacancyQuery
                        .Where(v => v.Embedding != null)
                        // ReSharper disable once EntityFramework.ClientSideDbFunctionCall
                        .OrderBy(v => searchEmbedding.CosineDistance(v.Embedding!));

                    _log.Information("Using vector similarity search for term: {SearchTerm}", vacancyFilter.SearchTerm);
                }
                else
                {
                    vacancyQuery = vacancyQuery.Where(v =>
                        v.Title.ToLower().Contains(searchTerm) ||
                        v.Description.ToLower().Contains(searchTerm));
                }
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Error using vector search for {SearchTerm}. Falling back to text search",
                    vacancyFilter.SearchTerm);

                vacancyQuery = vacancyQuery.Where(v =>
                    v.Title.ToLower().Contains(searchTerm) ||
                    v.Description.ToLower().Contains(searchTerm));
            }
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

        if (string.IsNullOrWhiteSpace(vacancyFilter.SearchTerm) ||
            !vacancyQuery.Expression.ToString().Contains("CosineDistance"))
        {
            vacancyQuery = vacancyQuery.OrderByDescending(v => v.CreatedAt);
        }

        var vacancies = await vacancyQuery
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

    private async Task<Vector?> GetSearchEmbedding(string searchTerm)
    {
        try
        {
            var embedding = await embeddingService.GenerateEmbeddingForSearchTerm(searchTerm);
            return embedding;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to generate embedding for search term: {SearchTerm}", searchTerm);
            return null;
        }
    }
}