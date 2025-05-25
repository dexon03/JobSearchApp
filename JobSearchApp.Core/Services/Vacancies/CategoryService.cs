using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class CategoryService(IAppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger)
    : ICategoryService
{
    private readonly ILogger _logger = logger.ForContext<CategoryService>();
    public async Task<List<Category>> GetAllCategories()
    {
        var cacheKey = "all_categories";
        var cacheTag = "categories";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching all categories from DB...", cacheKey);
                var categories = await db.Categories.ToListAsync(ctx);
                _logger.Information("Fetched {Count} categories from DB.", categories.Count);
                return categories;
            },
            tags: [cacheTag]
        );
    }

    public async Task<Category> GetCategoryById(int id)
    {
        var cacheKey = $"category_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _logger.Information("Cache miss for {CacheKey}. Fetching category from DB...", cacheKey);
                var category = await db.Categories.FindAsync(new object[] { id }, ctx);
                if (category is null)
                {
                    throw new Exception("Category not found");
                }

                return category;
            },
            tags: [$"category_{id}"]
        );
    }

    public async Task<Category> CreateCategory(CategoryCreateDto category)
    {
        var categoryEntity = mapper.Map<Category>(category);
        db.Categories.Add(categoryEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("categories");
        _logger.Information("New category created. Cache invalidated.");
        return categoryEntity;
    }

    public async Task<Category> UpdateCategory(CategoryUpdateDto category)
    {
        var categoryEntity = mapper.Map<Category>(category);
        var isExists = await db.Categories.AnyAsync(x => x.Id == categoryEntity.Id);
        if (!isExists)
        {
            throw new Exception("Category not found");
        }

        db.Categories.Update(categoryEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"category_{categoryEntity.Id}");
        await hybridCache.RemoveByTagAsync("categories");

        _logger.Information("Category {CategoryId} updated. Cache invalidated.", categoryEntity.Id);
        return categoryEntity;
    }

    public async Task DeleteCategory(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"category_{id}");
        await hybridCache.RemoveByTagAsync("categories");

        _logger.Information("Category {CategoryId} deleted. Cache invalidated.", id);
    }

    public async Task DeleteMany(Category[] categories)
    {
        db.Categories.RemoveRange(categories);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("categories");
        _logger.Information("Multiple categories deleted. Cache invalidated.");
    }
}