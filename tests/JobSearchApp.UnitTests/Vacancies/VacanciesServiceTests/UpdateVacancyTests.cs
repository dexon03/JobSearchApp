using System.Net;
using AutoMapper;
using Helpers;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using MassTransit;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.VacanciesServiceTests;

public class UpdateVacancyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<IEmbeddingService> _embeddingServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly VacancyService _service;

    public UpdateVacancyTests()
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
    public async Task UpdateVacancy_WhenExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var updateDto = new VacancyUpdateDto
        {
            Id = 1,
            Title = "Updated Vacancy"
        };

        var entity = new Vacancy
        {
            Id = 1,
            Title = "Old Title",
            RecruiterId = 101,
            VacancySkill = new List<VacancySkill>(),
            LocationVacancy = new List<LocationVacancy>()
        };

        var updatedDto = new VacancyGetDto { Id = 1, Title = "Updated Vacancy" };

        var vacancies = new List<Vacancy> { entity }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(vacancies);
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);

        _mapperMock.Setup(m => m.Map(updateDto, entity));
        _mapperMock.Setup(m => m.Map<VacancyGetDto>(entity)).Returns(updatedDto);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _publishEndpointMock.Setup(p =>
                p.Publish(It.Is<VacancyUpdatedEvent>(e => e.Id == entity.Id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.UpdateVacancy(updateDto);

        // Assert
        Assert.Equal(updatedDto.Id, result.Id);
        Assert.Equal(updatedDto.Title, result.Title);

        _mapperMock.Verify(m => m.Map(updateDto, entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(
            p => p.Publish(It.Is<VacancyUpdatedEvent>(e => e.Id == entity.Id), It.IsAny<CancellationToken>()),
            Times.Once);

        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"vacancy_{entity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("vacancies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("recommended_vacancies", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"vacancies_recruiter_{entity.RecruiterId}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(l => l.Information("Vacancy {VacancyId} updated. Cache invalidated.", entity.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateVacancy_Throws_WhenNotFound()
    {
        // Arrange
        var updateDto = new VacancyUpdateDto { Id = 999 };
        var mockSet = EfHelpers.CreateMockDbSet(new List<Vacancy>().AsQueryable());
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UpdateVacancy(updateDto));
        Assert.Equal("Vacancy not found", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        _loggerMock.Verify(l => l.Warning("Attempt to update non-existing vacancy {VacancyId}.", 999), Times.Once);
    }
}