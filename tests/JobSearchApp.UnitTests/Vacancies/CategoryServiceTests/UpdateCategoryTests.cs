using AutoMapper;
using Helpers;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CategoryServiceTests;

public class UpdateCategoryTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryService _service;

    public UpdateCategoryTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();

        _loggerMock.Setup(l => l.ForContext<CategoryService>()).Returns(_loggerMock.Object);

        _service = new CategoryService(
            _dbMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task UpdateCategory_WhenExists_UpdatesAndReturnsCategory()
    {
        // Arrange
        var dto = new CategoryUpdateDto { Id = 1, Name = "Updated" };
        var category = new Category { Id = 1, Name = "Updated" };

        _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(category);

        // Setup the in-memory list as IQueryable
        var categories = new List<Category> { category }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(categories);
        _dbMock.Setup(d => d.Categories).Returns(mockSet.Object);

        _dbMock.Setup(d => d.Categories.Update(category));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.UpdateCategory(dto);

        // Assert
        Assert.Equal(category, result);
        _dbMock.Verify(d => d.Categories.Update(category), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"category_{category.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Category {CategoryId} updated. Cache invalidated.", category.Id),
            Times.Once);
    }


    [Fact]
    public async Task UpdateCategory_Throws_WhenCategoryDoesNotExist()
    {
        // Arrange
        var dto = new CategoryUpdateDto { Id = 99, Name = "Missing" };
        var mapped = new Category { Id = 99, Name = "Missing" };

        _mapperMock.Setup(m => m.Map<Category>(dto)).Returns(mapped);

        var emptyData = new List<Category>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(emptyData);
        _dbMock.Setup(d => d.Categories).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateCategory(dto));
        Assert.Equal("Category not found", ex.Message);
    }
}