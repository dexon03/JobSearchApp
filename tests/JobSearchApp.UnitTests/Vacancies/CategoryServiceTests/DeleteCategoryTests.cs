using AutoMapper;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CategoryServiceTests;

public class DeleteCategoryTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CategoryService _service;

    public DeleteCategoryTests()
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
    public async Task DeleteCategory_RemovesCategory_AndInvalidatesCache()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test" };
        var mockSet = new Mock<DbSet<Category>>();

        _dbMock.Setup(d => d.Categories).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(category);

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteCategory(1);

        // Assert
        mockSet.Verify(m => m.Remove(category), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("categories", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("category_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Category {CategoryId} deleted. Cache invalidated.", 1), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_Throws_WhenNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Category>>();
        _dbMock.Setup(d => d.Categories).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(999)).ReturnsAsync((Category)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteCategory(999));
        Assert.Equal("Category not found", ex.Message);
    }
}