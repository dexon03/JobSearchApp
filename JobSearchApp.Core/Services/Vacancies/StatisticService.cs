using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Services.Vacancies;

public class StatisticService(AppDbContext db) : IStatisticService
{
    public async Task<IEnumerable<StatisticNode>> GetStatisticAsync(string? filterSkill)
    {
        DateTime lastYearStartDate = DateTime.Now.AddYears(-1);
        DateTime lastYearEndDate = DateTime.Now;
        var vacancies = await db.Vacancies
            .Include(v => v.VacancySkill)
            .ThenInclude(vs => vs.Skill)
            .Where(v => v.CreatedAt >= lastYearStartDate && v.CreatedAt <= lastYearEndDate &&
                        (filterSkill == null || v.VacancySkill.Any(vs => vs.Skill.Name == filterSkill)))
            .GroupBy(v => new { v.CreatedAt.Month, v.CreatedAt.Year })
            .Select(g => new
            {
                g.Key.Month,
                g.Key.Year,
                AverageSalary = g.Average(v => v.Salary)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync();
        var statistic = vacancies.Select(v => new StatisticNode
        {
            Salary = v.AverageSalary,
            Date = new DateOnly(year: v.Year, month: v.Month, day: 1)
        });

        return statistic;
    }

    public Task<List<StatisticNode>> GetMockedStatisticAsync()
    {
        var salaryRandomizer = new Random();
        var statistic = new List<StatisticNode>();
        for (int i = 0; i < 12; i++)
        {
            statistic.Add(new StatisticNode
            {
                Salary = salaryRandomizer.Next(1000, 1500),
                Date = new DateOnly(year: 2021, month: i + 1, day: 1)
            });
        }

        return Task.FromResult(statistic);
    }
}