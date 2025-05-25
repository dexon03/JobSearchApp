using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.VacanciesServiceTests;

public class DeleteVacancyTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<IEmbeddingService> _embeddingServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly VacancyService _service;

    public DeleteVacancyTests()
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
    public async Task DeleteVacancy_WhenFound_RemovesAndInvalidatesCache()
    {
        // Arrange
        var vacancy = new Vacancy
        {
            Id = 10,
            Title = "To Delete",
            RecruiterId = 101
        };

        var mockSet = new Mock<DbSet<Vacancy>>();
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(10)).ReturnsAsync(vacancy);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<FusionCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteVacancy(10);

        // Assert
        mockSet.Verify(m => m.Remove(vacancy), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"vacancy_{vacancy.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("vacancies", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("recommended_vacancies", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"vacancies_recruiter_{vacancy.RecruiterId}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _loggerMock.Verify(l => l.Information("Vacancy {VacancyId} deleted. Cache invalidated.", vacancy.Id),
            Times.Once);
    }

    [Fact]
    public async Task DeleteVacancy_Throws_WhenNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Vacancy>>();
        _dbMock.Setup(d => d.Vacancies).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(404)).ReturnsAsync((Vacancy)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.DeleteVacancy(404));

        Assert.Equal("Vacancy not found", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);

        _loggerMock.Verify(l => l.Warning("Attempt to delete non-existing vacancy {VacancyId}.", 404), Times.Once);
    }
}