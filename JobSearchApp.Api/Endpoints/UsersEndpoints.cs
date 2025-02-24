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
                        Role = u.UserRoles.First().Role
                    })
                    .ToListAsync();
                var totalCount = await userManager.Users.CountAsync();

                return Results.Ok(new UsersResponse
                {
                    Items = users.ToArray(),
                    TotalCount = totalCount
                });
            })
            .WithName("GetUsers")
            .WithOpenApi();

        userGroup.MapGet("/{id}", async (int id, UserManager<User> userManager)=>
            {
                var user = await userManager.FindByIdAsync(id.ToString());
                if (user is null) return Results.NotFound();

                var result = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role =  user.UserRoles.First().Role
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