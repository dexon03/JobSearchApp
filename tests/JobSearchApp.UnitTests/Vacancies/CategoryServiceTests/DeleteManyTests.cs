using AutoMapper;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CategoryServiceTests;

public class DeleteManyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryService _service;

    public DeleteManyTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();
        var mapperMock = new Mock<IMapper>();

        _loggerMock.Setup(l => l.ForContext<CategoryService>()).Returns(_loggerMock.Object);

        _service = new CategoryService(
            _dbMock.Object,
            mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task DeleteMany_RemovesCategories_AndInvalidatesCache()
    {
        // Arrange
        var categories = new[]
        {
            new Category { Id = 1, Name = "Tech" },
            new Category { Id = 2, Name = "HR" }
        };

        _dbMock.Setup(d => d.Categories.RemoveRange(categories));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteMany(categories);

        // Assert
        _dbMock.Verify(d => d.Categories.RemoveRange(categories), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Multiple categories deleted. Cache invalidated."), Times.Once);
    }
}