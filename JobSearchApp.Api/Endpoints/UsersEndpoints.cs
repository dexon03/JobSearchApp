using JobSearchApp.Core.Models.Identity;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Api.Endpoints;

public static class UsersEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var userGroup = group.MapGroup("/users").RequireAuthorization(Role.Admin.ToString());

        userGroup.MapGet("/", async (int page, int pageSize, UserManager<User> userManager) =>
            {
                var users = await userManager.Users
                    .OrderBy(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PhoneNumber = u.PhoneNumber,
                        Role = u.UserRole.First().Role.Name == "Admin" ? Role.Admin :
                            u.UserRole.First().Role.Name == "Recruiter" ? Role.Recruiter :
                            u.UserRole.First().Role.Name == "Candidate" ? Role.Candidate :
                            null
                    })
                    .ToListAsync();

                return Results.Ok(users);
            })
            .WithName("GetUsers")
            .WithOpenApi();

        userGroup.MapGet("/{id}", async (int id, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id.ToString());
                if (user is null) return Results.NotFound();

                var claims = await userManager.GetClaimsAsync(user);
                var result = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = claims.FirstOrDefault(c => c.Type == "Role")?.Value switch
                    {
                        "Admin" => Role.Admin,
                        "Recruiter" => Role.Recruiter,
                        "Candidate" => Role.Candidate,
                        _ => null
                    }
                };

                return Results.Ok(result);
            })
            .WithName("GetUser")
            .WithOpenApi();

        userGroup.MapPut("/", async (UpdateUserRequest request, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(request.Id.ToString());
                if (user is null) return Results.NotFound();

                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;

                var existingClaims = await userManager.GetClaimsAsync(user);
                await userManager.RemoveClaimsAsync(user, existingClaims);

                await userManager.AddClaimsAsync(user, new[]
                {
                    new System.Security.Claims.Claim("FirstName", request.FirstName),
                    new System.Security.Claims.Claim("LastName", request.LastName),
                    new System.Security.Claims.Claim("Role", request.Role.ToString())
                });

                await userManager.UpdateAsync(user);
                return Results.Ok();
            })
            .WithName("UpdateUser")
            .WithOpenApi();

        userGroup.MapDelete("/{id}", async (int id, UserManager<User> userManager) =>
            {
                var user = await userManager.FindByIdAsync(id.ToString());
                if (user is null) return Results.NotFound();

                await userManager.DeleteAsync(user);
                return Results.Ok();
            })
            .WithName("DeleteUser")
            .WithOpenApi();
    }
}