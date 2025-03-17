using System.Security.Claims;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace JobSearchApp.Api.Endpoints;

public static class RoleEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var roleGroup = group.MapGroup("/role");

        // roleGroup.MapGet("/", async (RoleManager<Role> roleManager) =>
        //     {
        //         var roles = await roleManager.Roles.ToListAsync();
        //         return Results.Ok(roles);
        //     })
        //     .WithName("GetRoles")Companies
        //     .WithOpenApi();

        roleGroup.MapGet("/", async (ClaimsPrincipal claimsPrincipal, UserManager<User> userManager) =>
            {
                var user = await userManager.GetUserAsync(claimsPrincipal);
                if (user is null)
                {
                    return Results.BadRequest("User not found");
                }

                var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                return role is not null ? Results.Ok(role) : Results.BadRequest("Role not found");
            })
            .RequireAuthorization()
            .WithName("GetRole")
            .WithOpenApi();
    }
}