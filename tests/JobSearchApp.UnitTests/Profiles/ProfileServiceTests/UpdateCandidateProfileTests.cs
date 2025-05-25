using System.Net;
using AutoMapper;
using Helpers;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class UpdateCandidateProfileTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ProfileService _service;

    public UpdateCandidateProfileTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _chatClientMock = new Mock<IChatClient>();
        _loggerMock = new Mock<ILogger>();
        _pdfServiceMock = new Mock<IPdfService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _loggerMock.Setup(l => l.ForContext<ProfileService>()).Returns(_loggerMock.Object);

        _service = new ProfileService(
            _dbMock.Object,
            _mapperMock.Object,
            _chatClientMock.Object,
            _loggerMock.Object,
            _pdfServiceMock.Object,
            _publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task UpdateCandidateProfile_Updates_And_ReturnsDto()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1, Name = "Old Name" };

        var profiles = new List<CandidateProfile> { profile }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(profiles);
        _dbMock.Setup(db => db.CandidateProfile).Returns(mockSet.Object);

        var updateDto = new CandidateProfileUpdateDto
        {
            Id = 1,
            Name = "New Name",
            Surname = "Doe"
        };

        _mapperMock.Setup(m => m.Map(updateDto, profile));
        _dbMock.Setup(d => d.CandidateProfile.Update(It.IsAny<CandidateProfile>()));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var expectedDto = new GetCandidateProfileDto { Id = 1, Name = "New Name" };
        _mapperMock.Setup(m => m.Map<GetCandidateProfileDto>(profile)).Returns(expectedDto);

        // Act
        var result = await _service.UpdateCandidateProfile(updateDto);

        // Assert
        Assert.Equal("New Name", result.Name);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<CandidateProfileUpdatedEvent>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateCandidateProfile_Throws_IfProfileNotFound()
    {
        // Arrange
        var empty = new List<CandidateProfile>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(empty);
        _dbMock.Setup(d => d.CandidateProfile).Returns(mockSet.Object);

        var dto = new CandidateProfileUpdateDto { Id = 99 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UpdateCandidateProfile(dto));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateCandidateProfile_Handles_PdfUpload_WhenProvided()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("resume.pdf");

        var profiles = new List<CandidateProfile> { profile }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(profiles);
        _dbMock.Setup(db => db.CandidateProfile).Returns(mockSet.Object);

        var dto = new CandidateProfileUpdateDto { Id = 1, PdfResume = mockFile.Object };

        _mapperMock.Setup(m => m.Map(dto, profile));
        _dbMock.Setup(d => d.CandidateProfile.Update(It.IsAny<CandidateProfile>()));

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _pdfServiceMock.Setup(p => p.CheckIfPdfExistsAndEqual(profile, mockFile.Object)).ReturnsAsync(false);
        _pdfServiceMock.Setup(p => p.UploadPdf(mockFile.Object, profile)).Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<GetCandidateProfileDto>(profile)).Returns(new GetCandidateProfileDto());

        // Act
        await _service.UpdateCandidateProfile(dto);

        // Assert
        _pdfServiceMock.Verify(p => p.UploadPdf(mockFile.Object, profile), Times.Once);
    }

    [Fact]
    public async Task UpdateCandidateProfile_Throws_When_Pdf_NotPdf()
    {
        // Arrange
        var profile = new CandidateProfile { Id = 1 };
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("not-a-pdf.txt");

        var profiles = new List<CandidateProfile> { profile }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(profiles);
        _dbMock.Setup(db => db.CandidateProfile).Returns(mockSet.Object);

        var dto = new CandidateProfileUpdateDto { Id = 1, PdfResume = mockFile.Object };

        _mapperMock.Setup(m => m.Map(dto, profile));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UpdateCandidateProfile(dto));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("File must be pdf", ex.Message);
    }
}