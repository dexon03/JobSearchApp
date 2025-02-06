using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ILocationService
{
    Task<List<Location>> GetAllLocations();
    Task<Location> GetLocationById(int id);
    Task<Location> CreateLocation(LocationCreateDto location);
    Task<Location> UpdateLocation(LocationUpdateDto location);
    Task DeleteLocation(int id);
    Task DeleteManyLocations(Location[] locations);
}