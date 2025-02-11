using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface IVacancyService
{
    Task<List<VacancyGetAllDto>> GetAllVacancies(VacancyFilterParameters vacancyFilter);
    Task<VacancyGetDto> GetVacancyById(int id);
    Task<List<VacancyGetAllDto>> GetVacanciesByRecruiterId(int recruiterId,VacancyFilterParameters vacancyFilter);
    Task<Vacancy> CreateVacancy(VacancyCreateDto vacancyDto);
    Task<Vacancy> UpdateVacancy(VacancyUpdateDto vacancy);
    Task DeleteVacancy(int id);
    Task ActivateDeactivateVacancy(int id);
}