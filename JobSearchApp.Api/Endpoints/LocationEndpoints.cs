using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace JobSearchApp.Api.Endpoints;

public static class LocationEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var locationGroup = group.MapGroup("/location").RequireAuthorization();

        locationGroup.MapGet("/", async (ILocationService locationService) =>
            {
                var locations = await locationService.GetAllLocations();
                return Results.Ok(locations);
            })
            .WithName("GetAllLocations")
            .WithOpenApi();

        locationGroup.MapGet("/{id}", async (int id, ILocationService locationService) =>
            {
                var location = await locationService.GetLocationById(id);
                return Results.Ok(location);
            })
            .WithName("GetLocationById")
            .WithOpenApi();

        locationGroup.MapPost("/", async (LocationCreateDto location, ILocationService locationService) =>
            {
                var createdLocation = await locationService.CreateLocation(location);
                return Results.Ok(createdLocation);
            })
            .RequireAuthorization(Role.Admin.ToString())
            .WithName("CreateLocation")
            .WithOpenApi();

        locationGroup.MapPut("/", async (LocationUpdateDto location, ILocationService locationService) =>
            {
                var updatedLocation = await locationService.UpdateLocation(location);
                return Results.Ok(updatedLocation);
            })
            .RequireAuthorization(Role.Admin.ToString())
            .WithName("UpdateLocation")
            .WithOpenApi();

        locationGroup.MapDelete("/{id}", async (int id, ILocationService locationService) =>
            {
                await locationService.DeleteLocation(id);
                return Results.Ok();
            })
            .RequireAuthorization(Role.Admin.ToString())
            .WithName("DeleteLocation")
            .WithOpenApi();

        locationGroup.MapDelete("/many",
                async ([FromBody] Location[] locations, [FromServices] ILocationService locationService) =>
                {
                    await locationService.DeleteManyLocations(locations);
                    return Results.Ok();
                })
            .RequireAuthorization(Role.Admin.ToString())
            .WithName("DeleteManyLocations")
            .WithOpenApi();
    }
}