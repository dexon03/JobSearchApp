using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class CategoryService(AppDbContext db, IMapper mapper) : ICategoryService
{
    public Task<List<Category>> GetAllCategories()
    {
        return db.Categories.ToListAsync();
    }

    public async Task<Category> GetCategoryById(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null)
        {
            throw new Exception("Category not found");
        }

        return category;
    }

    public async Task<Category> CreateCategory(CategoryCreateDto category)
    {
        var categoryEntity = mapper.Map<Category>(category);
        var result = db.Categories.Add(categoryEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Category> UpdateCategory(CategoryUpdateDto category)
    {
        var categoryEntity = mapper.Map<Category>(category);
        var isExists = await db.Categories.AnyAsync(x => x.Id == categoryEntity.Id);
        if (!isExists)
        {
            throw new Exception("Category not found");
        }

        var result = db.Update(categoryEntity);
        await db.SaveChangesAsync();
        return result.Entity;
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
    }

    public async Task DeleteMany(Category[] categories)
    {
        db.Categories.RemoveRange(categories);
        await db.SaveChangesAsync();
    }
}