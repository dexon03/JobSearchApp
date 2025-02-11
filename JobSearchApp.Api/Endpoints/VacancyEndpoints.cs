using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Api.Endpoints;

using JobSearchApp.Data.Enums;

public static class VacancyEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var vacancyGroup = group.MapGroup("/vacancy");

        vacancyGroup.MapGet("/{id}", async (int id, IVacancyService vacanciesService) =>
        {
            var vacancy = await vacanciesService.GetVacancyById(id);
            return Results.Ok(vacancy);
        })
        .WithName("GetVacancyById")
        .WithOpenApi();

        vacancyGroup.MapGet("/", async (VacancyFilterParameters vacancyFilter, IVacancyService vacanciesService) =>
        {
            return Results.Ok(await vacanciesService.GetAllVacancies(vacancyFilter));
        })
        .WithName("GetVacancies")
        .WithOpenApi();

        vacancyGroup.MapGet("/recruiterVacancies/{recruiterId}", async (int recruiterId, VacancyFilterParameters vacancyFilter, IVacancyService vacanciesService) =>
        {
            return Results.Ok(await vacanciesService.GetVacanciesByRecruiterId(recruiterId, vacancyFilter));
        })
        .WithName("GetRecruiterVacancies")
        .WithOpenApi();

        vacancyGroup.MapDelete("/{id}", async (int id, IVacancyService vacanciesService) =>
        {
            await vacanciesService.DeleteVacancy(id);
            return Results.Ok();
        })
        .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
        .WithName("DeleteVacancy")
        .WithOpenApi();

        vacancyGroup.MapPost("/", async (VacancyCreateDto vacancy, IVacancyService vacanciesService) =>
        {
            var createdVacancy = await vacanciesService.CreateVacancy(vacancy);
            return Results.Ok(createdVacancy);
        })
        .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
        .WithName("CreateVacancy")
        .WithOpenApi();

        vacancyGroup.MapPut("/", async (VacancyUpdateDto vacancy, IVacancyService vacanciesService) =>
        {
            var updatedVacancy = await vacanciesService.UpdateVacancy(vacancy);
            return Results.Ok(updatedVacancy);
        })
        .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
        .WithName("UpdateVacancy")
        .WithOpenApi();

        vacancyGroup.MapPut("/{id}/activate-deactivate", async (int id, IVacancyService vacanciesService) =>
        {
            await vacanciesService.ActivateDeactivateVacancy(id);
            return Results.Ok();
        })
        .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
        .WithName("ActivateDeactivateVacancy")
        .WithOpenApi();

        // vacancyGroup.MapPost("/getDescription", async (GenerateVacancyDescription request, IOllamaApiClient ollamaApiClient) =>
        // {
        //     string result = string.Empty;
        //     var prompt = "Generate description for vacancy in English. Max word count is 1000. " +
        //                  "!!!Important " +
        //                  "Return only vacancy description " +
        //                  "!!! The vacancy is for a " +
        //                  request.Title + " at " + request.CompanyDescription + ". The job description is " +
        //                  request.VacancyShortDescription;
        //
        //     await foreach (var stream in ollamaApiClient.GenerateAsync(prompt))
        //         result += stream.Response;
        //
        //     return Results.Ok(new { Description = result });
        // })
        // .RequireAuthorization(Role.Recruiter.ToString())
        // .WithName("GetGeneratedVacancyDescription")
        // .WithOpenApi();
    }
}
