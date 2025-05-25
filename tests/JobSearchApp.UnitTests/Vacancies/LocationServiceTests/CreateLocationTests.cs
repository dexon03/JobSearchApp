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

public class CreateLocationTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly LocationService _service;

    public CreateLocationTests()
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
    public async Task CreateLocation_CreatesLocation_AndInvalidatesCache()
    {
        // Arrange
        var createDto = new LocationCreateDto { City = "Kyiv", Country = "Ukraine" };
        var entity = new Location { Id = 1, City = "Kyiv", Country = "Ukraine" };
        var dto = new LocationDto { Id = 1, City = "Kyiv", Country = "Ukraine" };

        _mapperMock.Setup(m => m.Map<Location>(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.Map<LocationDto>(entity)).Returns(dto);

        var locations = new List<Location>().AsQueryable(); // No matching location
        var mockSet = EfHelpers.CreateMockDbSet(locations);
        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);

        _dbMock.Setup(d => d.Locations.Add(entity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.CreateLocation(createDto);

        // Assert
        Assert.Equal(dto.City, result.City);
        Assert.Equal(dto.Country, result.Country);
        _dbMock.Verify(d => d.Locations.Add(entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("locations", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(
            l => l.Information("New location {City}, {Country} created. Cache invalidated.", "Kyiv", "Ukraine"),
            Times.Once);
    }

    [Fact]
    public async Task CreateLocation_Throws_WhenAlreadyExists()
    {
        // Arrange
        var createDto = new LocationCreateDto { City = "Paris", Country = "France" };
        var locations = new List<Location>
        {
            new Location { Id = 1, City = "Paris", Country = "France" }
        }.AsQueryable();

        var mockSet = EfHelpers.CreateMockDbSet(locations);
        _dbMock.Setup(d => d.Locations).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.CreateLocation(createDto));
        Assert.Equal("Location already exists", ex.Message);
        _loggerMock.Verify(l => l.Warning("Location {City}, {Country} already exists.", "Paris", "France"), Times.Once);
    }
}