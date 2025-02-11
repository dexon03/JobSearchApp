using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.Core.Services.Vacancies;

public class LocationService(AppDbContext db, IMapper mapper, IFusionCache hybridCache, ILogger logger)
    : ILocationService
{
    private readonly ILogger _log = logger.ForContext<LocationService>();

    public async Task<List<Location>> GetAllLocations()
    {
        var cacheKey = "all_locations";
        var cacheTag = "locations";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching all locations from DB...", cacheKey);
                var locations = await db.Locations.ToListAsync(ctx);
                _log.Information("Fetched {Count} locations from DB.", locations.Count);
                return locations;
            },
            tags: [cacheTag]
        );
    }

    public async Task<Location> GetLocationById(int id)
    {
        var cacheKey = $"location_{id}";

        return await hybridCache.GetOrSetAsync(
            cacheKey,
            async ctx =>
            {
                _log.Information("Cache miss for {CacheKey}. Fetching location {LocationId} from DB...", cacheKey, id);
                var location = await db.Locations.FindAsync(new object[] { id }, ctx);
                if (location == null)
                {
                    _log.Warning("Location {LocationId} not found.", id);
                    throw new Exception("Location not found");
                }

                return location;
            },
            tags: [$"location_{id}"]
        );
    }

    public async Task<Location> CreateLocation(LocationCreateDto location)
    {
        var locationEntity = mapper.Map<Location>(location);
        var isExists = await db.Locations
            .AnyAsync(x => x.City == location.City && x.Country == location.Country);
        if (isExists)
        {
            _log.Warning("Location {City}, {Country} already exists.", location.City, location.Country);
            throw new Exception("Location already exists");
        }

        var result = db.Locations.Add(locationEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("locations");
        _log.Information("New location {City}, {Country} created. Cache invalidated.", location.City, location.Country);
        return result.Entity;
    }

    public async Task<Location> UpdateLocation(LocationUpdateDto location)
    {
        var locationEntity = mapper.Map<Location>(location);
        var isExists = await db.Locations.AnyAsync(x => x.Id == location.Id);
        if (!isExists)
        {
            _log.Warning("Attempt to update non-existing location {LocationId}.", location.Id);
            throw new Exception("Location not found");
        }

        var result = db.Update(locationEntity);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"location_{locationEntity.Id}");
        await hybridCache.RemoveByTagAsync("locations");

        _log.Information("Location {LocationId} updated. Cache invalidated.", locationEntity.Id);
        return result.Entity;
    }

    public async Task DeleteLocation(int id)
    {
        var location = await db.Locations.FindAsync(id);
        if (location == null)
        {
            _log.Warning("Attempt to delete non-existing location {LocationId}.", id);
            throw new Exception("Location not found");
        }

        db.Locations.Remove(location);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync($"location_{id}");
        await hybridCache.RemoveByTagAsync("locations");

        _log.Information("Location {LocationId} deleted. Cache invalidated.", id);
    }

    public async Task DeleteManyLocations(Location[] locations)
    {
        db.Locations.RemoveRange(locations);
        await db.SaveChangesAsync();

        await hybridCache.RemoveByTagAsync("locations");
        _log.Information("Multiple locations deleted. Cache invalidated.");
    }
}