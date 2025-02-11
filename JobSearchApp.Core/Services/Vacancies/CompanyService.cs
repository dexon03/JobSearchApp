using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class CompanyService(AppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger) : ICompanyService
{
    private readonly ILogger _logger = logger.ForContext<CompanyService>();

    public async Task<List<Company>> GetAllCompanies()
    {
        var cacheKey = "all_companies";
        var cacheTag = "companies";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching all companies from DB...", cacheKey);
                var companies = await db.Companies.ToListAsync(ctx);
                _logger.Information("Fetched {Count} companies from DB.", companies.Count);
                return companies;
            },
            tags: [cacheTag]
        );
    }

    public async Task<Company> GetCompanyById(int id)
    {
        var cacheKey = $"company_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching company from DB...", cacheKey);
                var company = await db.Companies.FindAsync(new object[] { id }, ctx);
                if (company == null)
                {
                    throw new Exception("Company not found");
                }

                return company;
            },
            tags: [$"company_{id}"]
        );
    }

    public async Task<Company> CreateCompany(CompanyCreateDto company)
    {
        var companyEntity = mapper.Map<Company>(company);
        var result = db.Companies.Add(companyEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("companies");
        _logger.Information("New company created. Cache invalidated.");
        return result.Entity;
    }

    public async Task<Company> UpdateCompany(CompanyUpdateDto company)
    {
        var companyEntity = mapper.Map<Company>(company);
        var isExist = await db.Companies.AnyAsync(x => x.Id == companyEntity.Id);
        if (!isExist)
        {
            throw new Exception("Company that you are trying to update does not exist");
        }

        var result = db.Update(companyEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"company_{companyEntity.Id}");
        await hybridCache.RemoveByTagAsync("companies");

        _logger.Information("Company {CompanyId} updated. Cache invalidated.", companyEntity.Id);
        return result.Entity;
    }

    public async Task DeleteCompany(int id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company == null)
        {
            throw new Exception("Company not found");
        }

        db.Companies.Remove(company);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"company_{id}");
        await hybridCache.RemoveByTagAsync("companies");

        _logger.Information("Company {CompanyId} deleted. Cache invalidated.", id);
    }
}