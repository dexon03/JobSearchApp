using System.Linq.Expressions;
using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class VacancyService(AppDbContext db, IMapper mapper) : IVacanciesService
{
    public async Task<List<VacancyGetAllDto>> GetAllVacancies(VacancyFilterParameters vacancyFilter)
    {
        var vacancies = await GetAllVacanciesByCondition(v => v.IsActive == true, vacancyFilter);
        return vacancies;
    }

    public async Task<VacancyGetDto> GetVacancyById(int id)
    {
        var vacancy = await db.Vacancies
            .Where(v => v.Id == id)
            .Include(v => v.Category)
            .Include(v => v.Company)
            .Include(v => v.LocationVacancy)
            .ThenInclude(lv => lv.Location)
            .Include(v => v.VacancySkill)
            .ThenInclude(vs => vs.Skill)
            .Select(v => new VacancyGetDto
            {
                Id = v.Id,
                RecruiterId = v.RecruiterId,
                Title = v.Title,
                Salary = v.Salary,
                Experience = v.Experience,
                CreatedAt = v.CreatedAt,
                AttendanceMode = v.AttendanceMode,
                Description = v.Description,
                IsActive = v.IsActive,
                Category = v.Category,
                Locations = v.LocationVacancy
                    .Select(lv => new Location
                    {
                        Id = lv.Location.Id,
                        City = lv.Location.City,
                        Country = lv.Location.Country
                    })
                    .ToList(),
                Skills = v.VacancySkill
                    .Select(vs => new Skill
                    {
                        Id = vs.Skill.Id,
                        Name = vs.Skill.Name
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (vacancy == null)
        {
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        return vacancy;
    }

    public Task<List<VacancyGetAllDto>> GetVacanciesByRecruiterId(int recruiterId,
        VacancyFilterParameters vacancyFilter)
    {
        return GetAllVacanciesByCondition(v => v.RecruiterId == recruiterId, vacancyFilter);
    }

    //TODO: Add validation for category and company existence
    public async Task<Vacancy> CreateVacancy(VacancyCreateDto vacancyDto)
    {
        var vacancy = new Vacancy();
        var vacancyEntity = mapper.Map(vacancyDto, vacancy);
        vacancyEntity.CreatedAt = DateTime.Now;

        var result = db.Vacancies.Add(vacancyEntity);
        await db.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Vacancy> UpdateVacancy(VacancyUpdateDto vacancy)
    {
        var vacancyEntity = await db.Vacancies.FindAsync(vacancy.Id);
        if (vacancyEntity is null)
        {
            throw new ExceptionWithStatusCode("Vacancy that you trying to update, not exist",
                HttpStatusCode.BadRequest);
        }

        vacancyEntity = mapper.Map(vacancy, vacancyEntity);
        vacancyEntity.UpdatedAt = DateTime.Now;

        var result = db.Update(vacancyEntity);
        await db.SaveChangesAsync();
        return result.Entity;
    }

    public async Task DeleteVacancy(int id)
    {
        var vacancy = await db.Vacancies.FindAsync(id);
        if (vacancy == null)
        {
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        db.Vacancies.Remove(vacancy);
        await db.SaveChangesAsync();
    }

    public async Task ActivateDeactivateVacancy(int id)
    {
        var vacancy = await db.Vacancies.FindAsync(id);
        if (vacancy == null)
        {
            throw new ExceptionWithStatusCode("Vacancy not found", HttpStatusCode.BadRequest);
        }

        vacancy.IsActive = !vacancy.IsActive;
        vacancy.UpdatedAt = DateTime.Now;
        db.Update(vacancy);

        await db.SaveChangesAsync();
    }

    private async Task<List<VacancyGetAllDto>> GetAllVacanciesByCondition(Expression<Func<Vacancy, bool>> predicate,
        VacancyFilterParameters vacancyFilter)
    {
        var vacancyQuery = db.Vacancies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(vacancyFilter.SearchTerm))
        {
            var searchTerm = vacancyFilter.SearchTerm.ToLower();
            vacancyQuery = vacancyQuery.Where(v =>
                v.Title.ToLower().Contains(searchTerm) ||
                v.Description.ToLower().Contains(searchTerm));
        }

        if (vacancyFilter.Experience is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.Experience == vacancyFilter.Experience);
        }

        if (vacancyFilter.AttendanceMode is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.AttendanceMode == vacancyFilter.AttendanceMode);
        }

        if (vacancyFilter.Skill is not null)
        {
            vacancyQuery = vacancyQuery.Include(v => v.VacancySkill).ThenInclude(vs => vs.Skill)
                .Where(v => v.VacancySkill.Any(vs => vs.SkillId == vacancyFilter.Skill));
        }

        if (vacancyFilter.Category is not null)
        {
            vacancyQuery = vacancyQuery.Where(v => v.CategoryId == vacancyFilter.Category);
        }

        if (vacancyFilter.Location is not null)
        {
            vacancyQuery = vacancyQuery.Include(v => v.LocationVacancy).ThenInclude(lv => lv.Location)
                .Where(v => v.LocationVacancy.Any(lv => lv.LocationId == vacancyFilter.Location));
        }

        // TODO: Refactor this query
        var vacancies = await vacancyQuery
            .Where(predicate)
            .Include(v => v.Company)
            .Include(v => v.LocationVacancy)
            .ThenInclude(lv => lv.Location)
            .OrderBy(x => x.CreatedAt)
            .Skip((vacancyFilter.Page - 1) * vacancyFilter.PageSize)
            .Take(vacancyFilter.PageSize)
            .Select(v => new VacancyGetAllDto
            {
                Id = v.Id,
                Title = v.Title,
                Salary = v.Salary,
                Experience = v.Experience.ToString(),
                CreatedAt = v.CreatedAt,
                AttendanceMode = v.AttendanceMode.ToString(),
                Description = v.Description,
                CompanyName = v.Company.Name,
                IsActive = v.IsActive,
                Locations = v.LocationVacancy
                    .Select(lv => new LocationDto
                    {
                        City = lv.Location.City,
                        Country = lv.Location.Country
                    })
                    .ToList()
            })
            .ToListAsync();

        return vacancies;
    }
}