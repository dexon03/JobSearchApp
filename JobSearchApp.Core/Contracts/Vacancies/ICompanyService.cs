using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface ICompanyService
{
    Task<List<Company>> GetAllCompanies();
    Task<Company> GetCompanyById(int id);
    Task<Company> CreateCompany(CompanyCreateDto company);
    Task<Company> UpdateCompany(CompanyUpdateDto company);
    Task DeleteCompany(int id);
}