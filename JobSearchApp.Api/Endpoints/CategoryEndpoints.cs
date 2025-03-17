using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.AspNetCore.Authorization;

namespace JobSearchApp.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var categoryGroup = group.MapGroup("/category");

        categoryGroup.MapGet("/",
                async (ICategoryService categoryService) =>
                {
                    return Results.Ok(await categoryService.GetAllCategories());
                })
            .WithName("GetAllCategories")
            .WithOpenApi();

        categoryGroup.MapGet("/{id}", async (int id, ICategoryService categoryService) =>
            {
                var category = await categoryService.GetCategoryById(id);
                return Results.Ok(category);
            })
            .WithName("GetCategoryById")
            .WithOpenApi();

        categoryGroup.MapPost("/", async (CategoryCreateDto category, ICategoryService categoryService) =>
            {
                var createdCategory = await categoryService.CreateCategory(category);
                return Results.Ok(createdCategory);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin}" })
            .WithName("CreateCategory")
            .WithOpenApi();

        categoryGroup.MapPut("/", async (CategoryUpdateDto category, ICategoryService categoryService) =>
            {
                var updatedCategory = await categoryService.UpdateCategory(category);
                return Results.Ok(updatedCategory);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin}" })
            .WithName("UpdateCategory")
            .WithOpenApi();

        categoryGroup.MapDelete("/{id}", async (int id, ICategoryService categoryService) =>
            {
                await categoryService.DeleteCategory(id);
                return Results.Ok();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin}" })
            .WithName("DeleteCategory")
            .WithOpenApi();

        categoryGroup.MapPut("/deleteMany", async (Category[] categories, ICategoryService categoryService) =>
            {
                await categoryService.DeleteMany(categories);
                return Results.Ok();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin}" })
            .WithName("DeleteManyCategories")
            .WithOpenApi();
    }
}
