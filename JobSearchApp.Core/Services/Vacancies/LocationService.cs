using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class LocationService(AppDbContext db, IMapper mapper) : ILocationService
{
    public Task<List<Location>> GetAllLocations()
    {
        return db.Locations.ToListAsync();
    }

    public async Task<Location> GetLocationById(int id)
    {
        var location = await db.Locations.FindAsync(id);
        if (location == null)
        {
            throw new Exception($"Location not found");
        }

        return location;
    }

    public async Task<Location> CreateLocation(LocationCreateDto location)
    {
        var locationEntity = mapper.Map<Location>(location);
        var isExists = await db.Locations
            .AnyAsync(x => x.City == location.City && x.Country == location.Country);
        if (isExists)
        {
            throw new Exception("Location already exists");
        }

        var result = db.Locations.Add(locationEntity);
        await db.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Location> UpdateLocation(LocationUpdateDto location)
    {
        var locationEntity = mapper.Map<Location>(location);
        var isExists = await db.Locations.AnyAsync(x => x.Id == location.Id);
        if (!isExists)
        {
            throw new Exception($"Location not found");
        }

        var result = db.Update(locationEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteLocation(int id)
    {
        var location = await db.Locations.FindAsync(id);
        if (location == null)
        {
            throw new Exception($"Location not found");
        }

        db.Locations.Remove(location);
        await db.SaveChangesAsync();
    }

    public Task DeleteManyLocations(Location[] locations)
    {
        db.Locations.RemoveRange(locations);
        return db.SaveChangesAsync();
    }
}