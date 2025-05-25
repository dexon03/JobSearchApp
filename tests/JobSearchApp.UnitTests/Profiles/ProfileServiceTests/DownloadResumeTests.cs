using System.Net;
using AutoMapper;
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

public class DownloadResumeTests
{
   private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly ProfileService _service;

    public DownloadResumeTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _pdfServiceMock = new Mock<IPdfService>();
        var mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger>();
        var chatClientMock = new Mock<IChatClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();

        loggerMock.Setup(l => l.ForContext<ProfileService>()).Returns(loggerMock.Object);

        _service = new ProfileService(
            _dbMock.Object,
            mapperMock.Object,
            chatClientMock.Object,
            loggerMock.Object,
            _pdfServiceMock.Object,
            publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task DownloadResume_ReturnsBytes_When_Initialized()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var expectedBytes = new byte[] { 1, 2, 3 };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(1)).ReturnsAsync(profile);
        _pdfServiceMock.Setup(p => p.CheckIfResumeFolderInitialised(profile)).Returns(true);
        _pdfServiceMock.Setup(p => p.DownloadPdf(profile)).ReturnsAsync(expectedBytes);

        // Act
        var result = await _service.DownloadResume(1);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task DownloadResume_ReturnsNull_When_NotInitialized()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 2 };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(2)).ReturnsAsync(profile);
        _pdfServiceMock.Setup(p => p.CheckIfResumeFolderInitialised(profile)).Returns(false);

        // Act
        var result = await _service.DownloadResume(2);

        // Assert
        Assert.Null(result);
        _pdfServiceMock.Verify(p => p.DownloadPdf(It.IsAny<CandidateProfile>()), Times.Never);
    }

    [Fact]
    public async Task DownloadResume_Throws_When_ProfileNotFound()
    {
        // Arrange
        _dbMock.Setup(d => d.CandidateProfile.FindAsync(404)).ReturnsAsync((CandidateProfile)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.DownloadResume(404));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Candidate profile not found", ex.Message);
    } 
}