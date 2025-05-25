using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using Moq;

namespace JobSearchApp.UnitTests.Vacancies.StatisticServiceTests;

public class GetMockedStatisticTests
{
    [Fact]
    public async Task GetMockedStatisticAsync_Returns12MonthlyPointsIn2021()
    {
        // Arrange
        var service = new StatisticService(Mock.Of<IAppDbContext>());

        // Act
        var result = await service.GetMockedStatisticAsync();

        // Assert
        Assert.Equal(12, result.Count);

        foreach (var node in result)
        {
            Assert.InRange(node.Date.Month, 1, 12);
            Assert.Equal(2021, node.Date.Year);
            Assert.InRange(node.Salary, 1000, 1500);
        }

        // Optional: Ensure months are unique
        var months = result.Select(r => r.Date.Month).Distinct().ToList();
        Assert.Equal(12, months.Count);
    }
}