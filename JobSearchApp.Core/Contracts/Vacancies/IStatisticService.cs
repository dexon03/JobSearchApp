using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Contracts.Vacancies;

public interface IStatisticService
{
    Task<IEnumerable<StatisticNode>> GetStatisticAsync(string? filterSkill);
    Task<List<StatisticNode>> GetMockedStatisticAsync();
}