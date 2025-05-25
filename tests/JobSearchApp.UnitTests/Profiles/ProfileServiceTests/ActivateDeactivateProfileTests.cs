using System.Net;
using AutoMapper;
using Helpers;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class ActivateDeactivateProfileTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly ProfileService _service;

    public ActivateDeactivateProfileTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger>();
        var chatClientMock = new Mock<IChatClient>();
        var pdfServiceMock = new Mock<IPdfService>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();

        loggerMock.Setup(l => l.ForContext<ProfileService>()).Returns(loggerMock.Object);

        _service = new ProfileService(
            _dbMock.Object,
            mapperMock.Object,
            chatClientMock.Object,
            loggerMock.Object,
            pdfServiceMock.Object,
            publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task ActivateDeactivateProfile_Toggles_IsActive_ForCandidate()
    {
        // Arrange
        var candidate = new CandidateProfile { Id = 1, IsActive = true };
        var data = new List<CandidateProfile> { candidate }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);

        _dbMock.Setup(d => d.Set<CandidateProfile>()).Returns(mockSet.Object);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.ActivateDeactivateProfile<CandidateProfile>(1);

        // Assert
        Assert.False(candidate.IsActive);
        mockSet.Verify(s => s.Update(candidate), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateDeactivateProfile_Toggles_IsActive_ForRecruiter()
    {
        // Arrange
        var recruiter = new RecruiterProfile { Id = 2, IsActive = false };
        var data = new List<RecruiterProfile> { recruiter }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);

        _dbMock.Setup(d => d.Set<RecruiterProfile>()).Returns(mockSet.Object);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.ActivateDeactivateProfile<RecruiterProfile>(2);

        // Assert
        Assert.True(recruiter.IsActive);
        mockSet.Verify(s => s.Update(recruiter), Times.Once);
    }

    [Fact]
    public async Task ActivateDeactivateProfile_Throws_When_NotFound()
    {
        // Arrange
        var data = new List<CandidateProfile>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);
        _dbMock.Setup(d => d.Set<CandidateProfile>()).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.ActivateDeactivateProfile<CandidateProfile>(99));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Profile not found", ex.Message);
    }
}