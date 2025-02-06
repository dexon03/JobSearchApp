using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class CompanyService(AppDbContext db, IMapper mapper) : ICompanyService
{
    public Task<List<Company>> GetAllCompanies()
    {
        return db.Companies.ToListAsync();
    }

    public async Task<Company> GetCompanyById(int id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company == null)
        {
            throw new Exception("Company not found");
        }

        return company;
    }

    public async Task<Company> CreateCompany(CompanyCreateDto company)
    {
        var companyEntity = mapper.Map<Company>(company);
        var result = db.Companies.Add(companyEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Company> UpdateCompany(CompanyUpdateDto company)
    {
        var companyEntity = mapper.Map<Company>(company);
        var isExist = await db.Companies.AnyAsync(x => x.Id == companyEntity.Id);
        if (!isExist)
        {
            throw new Exception("Company that you trying to update, not exist");
        }

        var result = db.Update(companyEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteCompany(int id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company == null)
        {
            throw new Exception("Company not found");
        }

        db.Companies.Remove(company);
        await db.SaveChangesAsync();
    }
}