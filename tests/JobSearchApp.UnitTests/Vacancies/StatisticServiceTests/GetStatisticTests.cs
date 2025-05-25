using Helpers;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Vacancies;
using Moq;

namespace JobSearchApp.UnitTests.Vacancies.StatisticServiceTests;

public class GetStatisticTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly StatisticService _service;

    public GetStatisticTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _service = new StatisticService(_dbMock.Object);
    }

    [Fact]
    public async Task GetStatisticAsync_ReturnsGroupedAverages_ByMonthAndYear()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var data = new List<Vacancy>
        {
            new Vacancy
            {
                CreatedAt = now.AddMonths(-2),
                Salary = 3000,
                VacancySkill = new List<VacancySkill>
                {
                    new VacancySkill { Skill = new Skill { Name = "C#" } }
                }
            },
            new Vacancy
            {
                CreatedAt = now.AddMonths(-2),
                Salary = 5000,
                VacancySkill = new List<VacancySkill>
                {
                    new VacancySkill { Skill = new Skill { Name = "C#" } }
                }
            },
            new Vacancy
            {
                CreatedAt = now.AddMonths(-1),
                Salary = 4000,
                VacancySkill = new List<VacancySkill>
                {
                    new VacancySkill { Skill = new Skill { Name = "Python" } }
                }
            }
        }.AsQueryable();

        var mockSet = EfHelpers.CreateMockDbSet(data);
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetStatisticAsync(null)).ToList();

        // Assert
        Assert.Equal(2, result.Count);

        var ordered = result.OrderBy(r => r.Date).ToList();
        Assert.Equal(new DateOnly(now.AddMonths(-2).Year, now.AddMonths(-2).Month, 1), ordered[0].Date);
        Assert.Equal(4000, ordered[0].Salary); // Average of 3000 and 5000

        Assert.Equal(new DateOnly(now.AddMonths(-1).Year, now.AddMonths(-1).Month, 1), ordered[1].Date);
        Assert.Equal(4000, ordered[1].Salary);
    }

    [Fact]
    public async Task GetStatisticAsync_FiltersBySkillName_WhenProvided()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var data = new List<Vacancy>
        {
            new Vacancy
            {
                CreatedAt = now,
                Salary = 4000,
                VacancySkill = new List<VacancySkill>
                {
                    new VacancySkill { Skill = new Skill { Name = "C#" } }
                }
            },
            new Vacancy
            {
                CreatedAt = now,
                Salary = 5000,
                VacancySkill = new List<VacancySkill>
                {
                    new VacancySkill { Skill = new Skill { Name = "Java" } }
                }
            }
        }.AsQueryable();

        var mockSet = EfHelpers.CreateMockDbSet(data);
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);

        // Act
        var result = (await _service.GetStatisticAsync("C#")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(4000, result[0].Salary);
    }

    [Fact]
    public async Task GetStatisticAsync_ReturnsEmpty_WhenNoVacanciesMatch()
    {
        // Arrange
        var data = new List<Vacancy>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);

        // Act
        var result = await _service.GetStatisticAsync(null);

        // Assert
        Assert.Empty(result);
    }
}