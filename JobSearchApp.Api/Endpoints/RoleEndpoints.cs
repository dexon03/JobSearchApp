using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Api.Endpoints;

public static class RoleEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var roleGroup = group.MapGroup("/role");

        // Allow Anonymous Access
        roleGroup.WithMetadata(new AllowAnonymousAttribute());

        roleGroup.MapGet("/", async (RoleManager<Role> roleManager) =>
            {
                var roles = await roleManager.Roles.ToListAsync();
                return Results.Ok(roles);
            })
            .WithName("GetRoles")
            .WithOpenApi();

        roleGroup.MapGet("/{id}", async (int id, RoleManager<Role> roleManager) =>
            {
                var role = await roleManager.FindByIdAsync(id.ToString());
                return role is not null ? Results.Ok(role) : Results.BadRequest("Role not found");
            })
            .WithName("GetRole")
            .WithOpenApi();
    }
}