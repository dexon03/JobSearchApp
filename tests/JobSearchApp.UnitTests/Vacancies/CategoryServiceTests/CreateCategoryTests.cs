using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CategoryServiceTests;

public class CreateCategoryTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryService _service;

    public CreateCategoryTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();

        _loggerMock.Setup(l => l.ForContext<CategoryService>()).Returns(_loggerMock.Object);

        _service = new CategoryService(_dbMock.Object, _mapperMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateCategory_CreatesCategory_AndInvalidatesCache()
    {
        // Arrange
        var dto = new CategoryCreateDto { Name = "Tech" };
        var category = new Category { Id = 1, Name = "Tech" };

        _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(category);

        var mockSet = new Mock<DbSet<Category>>();
        _dbMock.Setup(db => db.Categories).Returns(mockSet.Object);
        mockSet.Setup(m => m.Add(It.IsAny<Category>()));


        _dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.CreateCategory(dto);

        // Assert
        Assert.Equal(category, result);
        mockSet.Verify(s => s.Add(category), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("New category created. Cache invalidated."), Times.Once);
    }
}