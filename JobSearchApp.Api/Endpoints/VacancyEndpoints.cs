using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobSearchApp.Api.Endpoints;

public static class VacancyEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var vacancyGroup = group.MapGroup("/vacancy");

        vacancyGroup.MapGet("/{id}", async (int id, [FromServices] IVacancyService vacanciesService) =>
        {
            var vacancy = await vacanciesService.GetVacancyById(id);
            return Results.Ok(vacancy);
        })
        .WithName("GetVacancyById")
        .WithOpenApi();

        vacancyGroup.MapGet("", async ([AsParameters] VacancyFilterParameters vacancyFilter, [FromServices] IVacancyService vacanciesService) =>
        {
            return Results.Ok(await vacanciesService.GetAllVacancies(vacancyFilter));
        })
        .WithName("GetVacancies")
        .WithOpenApi();

        vacancyGroup.MapGet("/recruiterVacancies/{recruiterId}", async ([FromRoute]int recruiterId, [AsParameters] VacancyFilterParameters vacancyFilter, [FromServices] IVacancyService vacanciesService) =>
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
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin},{Role.Recruiter}" })
        .WithName("DeleteVacancy")
        .WithOpenApi();

        vacancyGroup.MapPost("/", async (VacancyCreateDto vacancy, IVacancyService vacanciesService) =>
        {
            var createdVacancy = await vacanciesService.CreateVacancy(vacancy);
            return Results.Ok(createdVacancy);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin},{Role.Recruiter}" })
        .WithName("CreateVacancy")
        .WithOpenApi();

        vacancyGroup.MapPut("/", async (VacancyUpdateDto vacancy, IVacancyService vacanciesService) =>
        {
            var updatedVacancy = await vacanciesService.UpdateVacancy(vacancy);
            return Results.Ok(updatedVacancy);
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin},{Role.Recruiter}" })
        .WithName("UpdateVacancy")
        .WithOpenApi();

        vacancyGroup.MapPut("/{id}/activate-deactivate", async (int id, IVacancyService vacanciesService) =>
        {
            await vacanciesService.ActivateDeactivateVacancy(id);
            return Results.Ok();
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin},{Role.Recruiter}" })
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
        // .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Recruiter}" })
        // .WithName("GetGeneratedVacancyDescription")
        // .WithOpenApi();
    }
}
