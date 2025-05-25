using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllLocations();
    Task<LocationDto> GetLocationById(int id);
    Task<LocationDto> CreateLocation(LocationCreateDto location);
    Task<LocationDto> UpdateLocation(LocationUpdateDto location);
    Task DeleteLocation(int id);
    Task DeleteManyLocations(int[] locations);
}