using AutoMapper;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class CreateProfileTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ProfileService _service;

    public CreateProfileTests()
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
    public async Task CreateProfile_AddsCandidateProfile_WhenRoleIsCandidate()
    {
        // Arrange
        var dto = new ProfileCreateDto
        {
            UserId = 1,
            Name = "Alice",
            Surname = "Smith",
            Email = "alice@example.com",
            PhoneNumber = "123456",
            PositionTitle = "Junior Dev",
            Role = Role.Candidate
        };

        var mockSet = new Mock<DbSet<CandidateProfile>>();
        _dbMock.Setup(db => db.CandidateProfile).Returns(mockSet.Object);

        _dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.CreateProfile(dto);

        // Assert
        mockSet.Verify(s => s.Add(It.Is<CandidateProfile>(p =>
            p.UserId == dto.UserId &&
            p.Name == dto.Name &&
            p.WorkExperience == Experience.NoExperience)), Times.Once);

        _dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProfile_AddsRecruiterProfile_WhenRoleIsRecruiter()
    {
        // Arrange
        var dto = new ProfileCreateDto
        {
            UserId = 2,
            Name = "Bob",
            Surname = "Johnson",
            Email = "bob@example.com",
            PhoneNumber = "654321",
            PositionTitle = "HR", // will be ignored
            Role = Role.Recruiter
        };

        var mockSet = new Mock<DbSet<RecruiterProfile>>();
        _dbMock.Setup(db => db.RecruiterProfile).Returns(mockSet.Object);

        _dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.CreateProfile(dto);

        // Assert
        mockSet.Verify(s => s.Add(It.Is<RecruiterProfile>(p =>
            p.UserId == dto.UserId &&
            p.Name == dto.Name &&
            p.Email == dto.Email)), Times.Once);

        _dbMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}