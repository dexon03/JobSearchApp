using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class UploadResumeTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly ProfileService _service;

    public UploadResumeTests()
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
    public async Task UploadResume_Uploads_When_ValidPdf()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("resume.pdf");

        var dto = new ResumeUploadDto { CandidateId = 1, Resume = file.Object };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(1)).ReturnsAsync(profile);
        _pdfServiceMock.Setup(p => p.CheckIfPdfExistsAndEqual(profile, file.Object)).ReturnsAsync(false);
        _pdfServiceMock.Setup(p => p.UploadPdf(file.Object, profile)).Returns(Task.CompletedTask);

        // Act
        await _service.UploadResume(dto);

        // Assert
        _pdfServiceMock.Verify(p => p.UploadPdf(file.Object, profile), Times.Once);
    }

    [Fact]
    public async Task UploadResume_Throws_When_ProfileNotFound()
    {
        // Arrange
        var dto = new ResumeUploadDto
        {
            CandidateId = 99,
            Resume = new Mock<IFormFile>().Object
        };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(99)).ReturnsAsync((CandidateProfile)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UploadResume(dto));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Candidate profile not found", ex.Message);
    }

    [Fact]
    public async Task UploadResume_Throws_When_NotPdf()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("resume.docx");

        var dto = new ResumeUploadDto { CandidateId = 1, Resume = file.Object };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(1)).ReturnsAsync(profile);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UploadResume(dto));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("File must be pdf", ex.Message);
    }

    [Fact]
    public async Task UploadResume_Skips_When_FileIsEqual()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("resume.pdf");

        var dto = new ResumeUploadDto { CandidateId = 1, Resume = file.Object };

        _dbMock.Setup(d => d.CandidateProfile.FindAsync(1)).ReturnsAsync(profile);
        _pdfServiceMock.Setup(p => p.CheckIfPdfExistsAndEqual(profile, file.Object)).ReturnsAsync(true);

        // Act
        await _service.UploadResume(dto);

        // Assert
        _pdfServiceMock.Verify(p => p.UploadPdf(It.IsAny<IFormFile>(), It.IsAny<CandidateProfile>()), Times.Never);
    }
}