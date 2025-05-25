using AutoMapper;
using Helpers;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CompanyServiceTests;

public class UpdateCompanyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CompanyService _service;

    public UpdateCompanyTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();

        _loggerMock.Setup(l => l.ForContext<CompanyService>()).Returns(_loggerMock.Object);

        _service = new CompanyService(
            _dbMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task UpdateCompany_WhenExists_Updates_AndInvalidatesCache()
    {
        // Arrange
        var dto = new CompanyUpdateDto
        {
            Id = 1,
            Name = "Updated Company",
            Description = "Updated Desc"
        };

        var entity = new Company
        {
            Id = 1,
            Name = dto.Name,
            Description = dto.Description
        };

        _mapperMock.Setup(m => m.Map<Company>(dto)).Returns(entity);

        var companies = new List<Company> { entity }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(companies);
        _dbMock.Setup(d => d.Companies).Returns(mockSet.Object);

        _dbMock.Setup(d => d.Companies.Update(entity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveByTagAsync($"company_{entity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.UpdateCompany(dto);

        // Assert
        Assert.Equal(entity, result);
        _dbMock.Verify(d => d.Companies.Update(entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"company_{entity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Company {CompanyId} updated. Cache invalidated.", entity.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCompany_Throws_WhenNotExist()
    {
        // Arrange
        var dto = new CompanyUpdateDto { Id = 99, Name = "Missing", Description = "Missing" };
        var mapped = new Company { Id = 99, Name = "Missing", Description = "Missing" };

        _mapperMock.Setup(m => m.Map<Company>(dto)).Returns(mapped);

        var emptyList = new List<Company>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(emptyList);
        _dbMock.Setup(d => d.Companies).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateCompany(dto));
        Assert.Equal("Company that you are trying to update does not exist", ex.Message);
    }
}