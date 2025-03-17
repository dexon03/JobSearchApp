using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using Microsoft.AspNetCore.Authorization;

namespace JobSearchApp.Api.Endpoints;

public static class CompanyEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var companyGroup = group.MapGroup("/company");

        companyGroup.MapGet("/", async (ICompanyService companyService) =>
            {
                var companies = await companyService.GetAllCompanies();
                return Results.Ok(companies);
            })
            .WithName("GetAllCompanies")
            .WithOpenApi();

        companyGroup.MapGet("/{id}", async (int id, ICompanyService companyService) =>
            {
                var company = await companyService.GetCompanyById(id);
                return Results.Ok(company);
            })
            .WithName("GetCompanyById")
            .WithOpenApi();

        companyGroup.MapPost("/", async (CompanyCreateDto company, ICompanyService companyService) =>
            {
                var createdCompany = await companyService.CreateCompany(company);
                return Results.Ok(createdCompany);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Recruiter},{Role.Admin}" })
            .WithName("CreateCompany")
            .WithOpenApi();

        companyGroup.MapPut("/", async (CompanyUpdateDto company, ICompanyService companyService) =>
            {
                var updatedCompany = await companyService.UpdateCompany(company);
                return Results.Ok(updatedCompany);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Recruiter},{Role.Admin}" })
            .WithName("UpdateCompany")
            .WithOpenApi();

        companyGroup.MapDelete("/{id}", async (int id, ICompanyService companyService) =>
            {
                await companyService.DeleteCompany(id);
                return Results.Ok();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = $"{Role.Admin}" })
            .WithName("DeleteCompany")
            .WithOpenApi();
    }
}