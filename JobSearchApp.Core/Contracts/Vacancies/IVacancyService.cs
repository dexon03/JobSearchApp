using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface IVacancyService
{
    Task<List<VacancyGetAllDto>> GetAllVacancies(VacancyFilterParameters vacancyFilter);
    Task<VacancyGetDto> GetVacancyById(int id);
    Task<List<VacancyGetAllDto>> GetVacanciesByRecruiterId(int recruiterId, VacancyFilterParameters vacancyFilter);
    Task<VacancyGetDto> CreateVacancy(VacancyCreateDto vacancyDto);
    Task<VacancyGetDto> UpdateVacancy(VacancyUpdateDto vacancy);
    Task DeleteVacancy(int id);
    Task ActivateDeactivateVacancy(int id);
    Task<string> GenerateVacancyDescription(int userId, AiVacancyDescriptionRequest descriptionRequest);
}