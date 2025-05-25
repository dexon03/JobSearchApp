using AutoMapper;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Vacancies;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.VacanciesServiceTests;

public class CreateServiceTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<IEmbeddingService> _embeddingServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly VacancyService _service;

    public CreateServiceTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();
        _chatClientMock = new Mock<IChatClient>();
        _embeddingServiceMock = new Mock<IEmbeddingService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _loggerMock.Setup(l => l.ForContext<VacancyService>()).Returns(_loggerMock.Object);

        _service = new VacancyService(
            _dbMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            _chatClientMock.Object,
            _embeddingServiceMock.Object,
            _publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task CreateVacancy_Creates_And_ReturnsMappedDto()
    {
        // Arrange
        var dto = new VacancyCreateDto
        {
            Title = "Senior Dev",
            Description = "Backend role",
            Salary = 8000,
            AttendanceMode = AttendanceMode.Remote,
            Experience = Experience.FourYears,
            RecruiterId = 42,
            CategoryId = 1,
            CompanyId = 1
        };

        var entity = new Vacancy
        {
            Id = 100,
            Title = dto.Title,
            Description = dto.Description,
            Salary = dto.Salary,
            RecruiterId = dto.RecruiterId
        };

        var resultDto = new VacancyGetDto { Id = 100, Title = "Senior Dev" };

        _mapperMock.Setup(m => m.Map<Vacancy>(dto)).Returns(entity);
        _mapperMock.Setup(m => m.Map<VacancyGetDto>(entity)).Returns(resultDto);

        var mockSet = new Mock<DbSet<Vacancy>>();
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);
        mockSet.Setup(s => s.Add(entity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _publishEndpointMock
            .Setup(p => p.Publish(It.Is<VacancyCreatedEvent>(e => e.Id == 100), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.CreateVacancy(dto);

        // Assert
        Assert.Equal(100, result.Id);
        Assert.Equal("Senior Dev", result.Title);

        mockSet.Verify(s => s.Add(entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(
            p => p.Publish(It.Is<VacancyCreatedEvent>(e => e.Id == 100), It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(
            c => c.RemoveByTagAsync("vacancies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("recommended_vacancies", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"vacancies_recruiter_{dto.RecruiterId}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(l => l.Information("New vacancy {VacancyId} created. Cache invalidated.", entity.Id),
            Times.Once);
    }
}