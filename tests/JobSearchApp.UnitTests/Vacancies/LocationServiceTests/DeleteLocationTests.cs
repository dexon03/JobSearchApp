using AutoMapper;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.LocationServiceTests;

public class DeleteLocationTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly LocationService _service;

    public DeleteLocationTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();

        _loggerMock.Setup(l => l.ForContext<LocationService>()).Returns(_loggerMock.Object);

        _service = new LocationService(
            _dbMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task DeleteLocation_WhenFound_RemovesAndInvalidatesCache()
    {
        // Arrange
        var location = new Location { Id = 1, City = "Oslo", Country = "Norway" };
        var mockSet = new Mock<DbSet<Location>>();

        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(location);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync($"location_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteLocation(1);

        // Assert
        mockSet.Verify(m => m.Remove(location), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"location_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Location {LocationId} deleted. Cache invalidated.", 1), Times.Once);
    }

    [Fact]
    public async Task DeleteLocation_Throws_WhenNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Location>>();
        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(999)).ReturnsAsync((Location)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteLocation(999));
        Assert.Equal("Location not found", ex.Message);
        _loggerMock.Verify(l => l.Warning("Attempt to delete non-existing location {LocationId}.", 999), Times.Once);
    }
}