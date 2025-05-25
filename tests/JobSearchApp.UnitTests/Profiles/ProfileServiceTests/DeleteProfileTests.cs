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

public class DeleteProfileTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly ProfileService _service;

    public DeleteProfileTests()
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
    public async Task DeleteProfile_Deletes_CandidateProfile_WhenFound()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var data = new List<CandidateProfile> { profile }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);

        _dbMock.Setup(d => d.Set<CandidateProfile>()).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(profile);

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.DeleteProfile<CandidateProfile>(1);

        // Assert
        mockSet.Verify(s => s.Remove(profile), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProfile_Deletes_RecruiterProfile_WhenFound()
    {
        // Arrange
        var profile = new RecruiterProfile { Id = 2 };
        var data = new List<RecruiterProfile> { profile }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);

        _dbMock.Setup(d => d.Set<RecruiterProfile>()).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(profile);

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.DeleteProfile<RecruiterProfile>(2);

        // Assert
        mockSet.Verify(s => s.Remove(profile), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProfile_Throws_When_ProfileNotFound()
    {
        // Arrange
        var emptyData = new List<CandidateProfile>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(emptyData);

        _dbMock.Setup(d => d.Set<CandidateProfile>()).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.DeleteProfile<CandidateProfile>(99));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Profile not found", ex.Message);
    }
}