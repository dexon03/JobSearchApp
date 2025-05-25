using AutoMapper;
using Helpers;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.LocationServiceTests;

public class UpdateLocationTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly LocationService _service;

    public UpdateLocationTests()
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
    public async Task UpdateLocation_WhenExists_Updates_AndReturnsDto()
    {
        // Arrange
        var updateDto = new LocationUpdateDto { Id = 1, City = "Berlin", Country = "Germany" };
        var entity = new Location { Id = 1, City = "Berlin", Country = "Germany" };
        var dto = new LocationDto { Id = 1, City = "Berlin", Country = "Germany" };

        _mapperMock.Setup(m => m.Map<Location>(updateDto)).Returns(entity);
        _mapperMock.Setup(m => m.Map<LocationDto>(entity)).Returns(dto);

        var locations = new List<Location> { entity }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(locations);
        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);

        _dbMock.Setup(d => d.Locations.Update(entity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveByTagAsync($"location_{entity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.UpdateLocation(updateDto);

        // Assert
        Assert.Equal(dto.City, result.City);
        Assert.Equal(dto.Country, result.Country);
        _dbMock.Verify(d => d.Locations.Update(entity), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"location_{entity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Location {LocationId} updated. Cache invalidated.", entity.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateLocation_Throws_WhenLocationNotFound()
    {
        // Arrange
        var updateDto = new LocationUpdateDto { Id = 42, City = "Rome", Country = "Italy" };
        var mapped = new Location { Id = 42, City = "Rome", Country = "Italy" };

        _mapperMock.Setup(m => m.Map<Location>(updateDto)).Returns(mapped);

        var mockSet = EfHelpers.CreateMockDbSet(new List<Location>().AsQueryable());
        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateLocation(updateDto));
        Assert.Equal("Location not found", ex.Message);
        _loggerMock.Verify(l => l.Warning("Attempt to update non-existing location {LocationId}.", 42), Times.Once);
    }
}