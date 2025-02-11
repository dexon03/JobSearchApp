using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace JobSearchApp.Api.Endpoints;

public static class SkillsEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var skillGroup = group.MapGroup("/skill");

        skillGroup.MapGet("/", async (ISkillService skillService) =>
            {
                var result = await skillService.GetAllSkills();
                return Results.Ok(result);
            })
            .WithName("GetAllSkills")
            .WithOpenApi();

        skillGroup.MapGet("/{id}", async (int id, ISkillService skillService) =>
            {
                var skill = await skillService.GetSkillById(id);
                return Results.Ok(skill);
            })
            .WithName("GetSkillById")
            .WithOpenApi();

        skillGroup.MapPost("/", async (SkillCreateDto skill, ISkillService skillService) =>
            {
                var createdSkill = await skillService.CreateSkill(skill);
                return Results.Ok(createdSkill);
            })
            .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
            .WithName("CreateSkill")
            .WithOpenApi();

        skillGroup.MapPut("/", async (SkillUpdateDto skill, ISkillService skillService) =>
            {
                var updatedSkill = await skillService.UpdateSkill(skill);
                return Results.Ok(updatedSkill);
            })
            .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
            .WithName("UpdateSkill")
            .WithOpenApi();

        skillGroup.MapDelete("/{id}", async (int id, ISkillService skillService) =>
            {
                await skillService.DeleteSkill(id);
                return Results.Ok();
            })
            .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
            .WithName("DeleteSkill")
            .WithOpenApi();

        skillGroup.MapDelete("/deleteMany",
                async ([FromQuery] int[] skills, [FromServices] ISkillService skillService) =>
                {
                    await skillService.DeleteManySkills(skills);
                    return Results.Ok();
                })
            .RequireAuthorization(Role.Admin.ToString(), Role.Recruiter.ToString(), "CompanyOwner")
            .WithName("DeleteManySkills")
            .WithOpenApi();
    }
}