using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ICategoryService
{
    Task<List<Category>> GetAllCategories();
    Task<Category> GetCategoryById(int id);
    Task<Category> CreateCategory(CategoryCreateDto category);
    Task<Category> UpdateCategory(CategoryUpdateDto category);
    Task DeleteCategory(int id);
    Task DeleteMany (Category[] categories);
}