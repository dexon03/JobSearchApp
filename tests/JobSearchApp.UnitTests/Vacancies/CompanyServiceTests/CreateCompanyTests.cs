using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.CompanyServiceTests;

public class CreateCompanyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CompanyService _service;

    public CreateCompanyTests()
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
    public async Task CreateCompany_CreatesCompany_AndInvalidatesCache()
    {
        // Arrange
        var dto = new CompanyCreateDto
        {
            Name = "OpenAI",
            Description = "AI Research Company"
        };

        var entity = new Company
        {
            Id = 1,
            Name = dto.Name,
            Description = dto.Description
        };

        _mapperMock.Setup(m => m.Map<Company>(dto)).Returns(entity);

        var mockSet = new Mock<DbSet<Company>>();
        _dbMock.Setup(db => db.Companies).Returns(mockSet.Object);
        mockSet.Setup(s => s.Add(entity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock
            .Setup(c => c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);


        // Act
        var result = await _service.CreateCompany(dto);

        // Assert
        Assert.Equal(entity, result);
        mockSet.Verify(s => s.Add(entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c =>
                c.RemoveByTagAsync("companies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _loggerMock.Verify(l => l.Information("New company created. Cache invalidated."), Times.Once);
    }
}