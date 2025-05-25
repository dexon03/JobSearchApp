using AutoMapper;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CompanyServiceTests;

public class DeleteCompanyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CompanyService _service;

    public DeleteCompanyTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();
        var mapperMock = new Mock<IMapper>();

        _loggerMock.Setup(l => l.ForContext<CompanyService>()).Returns(_loggerMock.Object);

        _service = new CompanyService(
            _dbMock.Object,
            mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task DeleteCompany_WhenFound_RemovesAndInvalidatesCache()
    {
        // Arrange
        var company = new Company { Id = 1, Name = "Test Co", Description = "Desc" };
        var mockSet = new Mock<DbSet<Company>>();
        _dbMock.Setup(d => d.Companies).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(company);

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveByTagAsync($"company_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                  .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c => c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                  .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteCompany(1);

        // Assert
        mockSet.Verify(m => m.Remove(company), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByTagAsync("company_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(l => l.Information("Company {CompanyId} deleted. Cache invalidated.", 1), Times.Once);
    }

    [Fact]
    public async Task DeleteCompany_Throws_WhenNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Company>>();
        _dbMock.Setup(d => d.Companies).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(999)).ReturnsAsync((Company)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteCompany(999));
        Assert.Equal("Company not found", ex.Message);
    }
}