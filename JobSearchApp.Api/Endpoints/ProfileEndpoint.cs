using System.Security.Claims;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Api.Endpoints;

public static class ProfileEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var profileGroup = group.MapGroup("/profile").RequireAuthorization();

        profileGroup.MapGet("/{role}",
                async (Role role, ClaimsPrincipal claims, IProfileService profileService,
                    UserManager<User> userManager) =>
            {
                var userId = int.Parse(userManager.GetUserId(claims) ?? throw new InvalidOperationException());
                return role switch
                {
                    Role.Candidate => Results.Ok(await profileService.GetCandidateProfileByUserId(userId)),
                    Role.Recruiter => Results.Ok(await profileService.GetRecruiterProfileByUserId(userId)),
                    _ => Results.BadRequest()
                };
            })
            .WithName("GetUserProfile")
            .WithOpenApi();

        profileGroup.MapGet("/recruiter/{profileId}",
                async (int profileId, IProfileService profileService) =>
                {
                    return Results.Ok(await profileService.GetRecruiterProfile(profileId));
                })
            .WithName("GetRecruiterProfile")
            .WithOpenApi();

        profileGroup.MapGet("/candidate/{profileId}",
                async (int profileId, IProfileService profileService) => Results.Ok((object?)await profileService.GetCandidateProfile(profileId)))
            .WithName("GetCandidateProfile")
            .WithOpenApi();

        profileGroup.MapGet("/candidates",
                async ([AsParameters] CandidateFilterParameters filter,
                    [FromServices] IProfileService profileService) =>
                {
                    return Results.Ok(await profileService.GetAllCandidatesProfiles(filter));
                })
            .WithName("GetCandidatesProfiles")
            .WithOpenApi();

        profileGroup.MapPut("/candidate",
                async (CandidateProfileUpdateDto profile, IProfileService profileService) =>
                    Results.Ok((object?)await profileService.UpdateCandidateProfile(profile)))
            .WithName("UpdateCandidateProfile")
            .WithOpenApi();

        profileGroup.MapPut("/uploadResume", async ([FromForm]ResumeUploadDto resume, IProfileService profileService) =>
            {
                await profileService.UploadResume(resume);
                return Results.Ok();
            })
            .WithName("UploadResume")
            .WithOpenApi()
            .DisableAntiforgery();

        profileGroup.MapGet("/downloadResume/{profileId}", async (int profileId, IProfileService profileService) =>
            {
                var result = await profileService.DownloadResume(profileId);
                return result is null ? Results.Ok() : Results.File(result, "application/pdf");
            })
            .WithName("DownloadResume")
            .WithOpenApi();

        profileGroup.MapPut("/recruiter",
                async (RecruiterProfileUpdateDto profile, IProfileService profileService) =>
                {
                    return Results.Ok(await profileService.UpdateRecruiterProfile(profile));
                })
            .WithName("UpdateRecruiterProfile")
            .WithOpenApi();
    }
}