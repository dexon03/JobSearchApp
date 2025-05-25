using AutoMapper;
using Helpers;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class DeleteProfileByUserIdTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly ProfileService _service;

    public DeleteProfileByUserIdTests()
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
    public async Task DeleteProfileByUserId_Deletes_CandidateProfile_WhenFound()
    {
        // Arrange
        var candidate = new CandidateProfile { Id = 1, UserId = 123 };
        var data = new List<CandidateProfile> { candidate }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(data);

        _dbMock.Setup(d => d.CandidateProfile).Returns(mockSet.Object);

        // Act
        await _service.DeleteProfileByUserId(123);

        // Assert
        mockSet.Verify(s => s.Remove(candidate), Times.Once);
    }

    [Fact]
    public async Task DeleteProfileByUserId_Deletes_RecruiterProfile_WhenCandidateNotFound()
    {
        // Arrange
        var recruiter = new RecruiterProfile { Id = 2, UserId = 456 };
        var emptyCandidateSet = EfHelpers.CreateMockDbSet(new List<CandidateProfile>().AsQueryable());
        var recruiterSet = EfHelpers.CreateMockDbSet(new List<RecruiterProfile> { recruiter }.AsQueryable());

        _dbMock.Setup(d => d.CandidateProfile).Returns(emptyCandidateSet.Object);
        _dbMock.Setup(d => d.RecruiterProfile).Returns(recruiterSet.Object);

        // Act
        await _service.DeleteProfileByUserId(456);

        // Assert
        recruiterSet.Verify(s => s.Remove(recruiter), Times.Once);
    }

    [Fact]
    public async Task DeleteProfileByUserId_DoesNothing_WhenNoProfileFound()
    {
        // Arrange
        var emptyCandidateSet = EfHelpers.CreateMockDbSet(new List<CandidateProfile>().AsQueryable());
        var emptyRecruiterSet = EfHelpers.CreateMockDbSet(new List<RecruiterProfile>().AsQueryable());

        _dbMock.Setup(d => d.CandidateProfile).Returns(emptyCandidateSet.Object);
        _dbMock.Setup(d => d.RecruiterProfile).Returns(emptyRecruiterSet.Object);

        // Act
        await _service.DeleteProfileByUserId(999);

        // Assert
        emptyCandidateSet.Verify(s => s.Remove(It.IsAny<CandidateProfile>()), Times.Never);
        emptyRecruiterSet.Verify(s => s.Remove(It.IsAny<RecruiterProfile>()), Times.Never);
    }
}