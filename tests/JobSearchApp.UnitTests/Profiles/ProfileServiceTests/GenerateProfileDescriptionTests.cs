using System.Net;
using AutoMapper;
using Helpers;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class GenerateProfileDescriptionTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IChatClient> _chatClientMock;
    private readonly Mock<IPdfService> _pdfServiceMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly ProfileService _service;

    public GenerateProfileDescriptionTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _chatClientMock = new Mock<IChatClient>();
        _pdfServiceMock = new Mock<IPdfService>();
        _loggerMock = new Mock<ILogger>();
        var mapperMock = new Mock<IMapper>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();

        _loggerMock.Setup(l => l.ForContext<ProfileService>()).Returns(_loggerMock.Object);

        _service = new ProfileService(
            _dbMock.Object,
            mapperMock.Object,
            _chatClientMock.Object,
            _loggerMock.Object,
            _pdfServiceMock.Object,
            publishEndpointMock.Object
        );
    }

    // [Fact]
    // public async Task GenerateProfileDescription_ReturnsChatResponse()
    // {
    //     // Arrange
    //     var userId = 1;
    //     var profile = new CandidateProfile
    //     {
    //         UserId = userId,
    //         Description = "Current profile description",
    //         ProfileSkills = new List<ProfileSkills>
    //         {
    //             new() { Skill = new Skill { Name = "C#" } },
    //             new() { Skill = new Skill { Name = "ASP.NET" } }
    //         }
    //     };
    //
    //     var request = new AiDescriptionRequest
    //     {
    //         Description = "User provided summary",
    //         Experience = Experience.ThreeToFiveYears,
    //         PositionTitle = "Backend Developer"
    //     };
    //
    //     var data = new List<CandidateProfile> { profile }.AsQueryable();
    //     var mockSet = EfHelpers.CreateMockDbSet(data);
    //     _dbMock.Setup(d => d.CandidateProfile).Returns(mockSet.Object);
    //
    //     _pdfServiceMock.Setup(p => p.DownloadPdf(profile)).ReturnsAsync(new byte[] { 1, 2, 3 });
    //
    //     _chatClientMock.Setup(c => c.GetResponseAsync(It.IsAny<ChatMessage>()))
    //         .ReturnsAsync(new ChatResponse { Text = "AI generated content" });
    //
    //     // Act
    //     var result = await _service.GenerateProfileDescription(userId, request);
    //
    //     // Assert
    //     Assert.Equal("AI generated content", result);
    //     _chatClientMock.Verify(c => c.GetResponseAsync(It.Is<ChatMessage>(msg =>
    //         msg.Contents.OfType<DataContent>().Any())), Times.Once);
    // }

    // [Fact]
    // public async Task GenerateProfileDescription_UsesFallbackDescription_IfEmptyInRequest()
    // {
    //     // Arrange
    //     var userId = 2;
    //     var profile = new CandidateProfile
    //     {
    //         UserId = userId,
    //         Description = "Fallback from profile",
    //         ProfileSkills = new List<ProfileSkills>()
    //     };
    //
    //     var request = new AiDescriptionRequest
    //     {
    //         Description = null,
    //         Experience = Experience.OneToThreeYears,
    //         PositionTitle = "QA Tester"
    //     };
    //
    //     var mockSet = EfHelpers.CreateMockDbSet(new[] { profile }.AsQueryable());
    //     _dbMock.Setup(d => d.CandidateProfile).Returns(mockSet.Object);
    //
    //     _pdfServiceMock.Setup(p => p.DownloadPdf(profile)).ReturnsAsync(Array.Empty<byte>());
    //     _chatClientMock.Setup(c => c.GetResponseAsync(It.IsAny<ChatMessage>()))
    //         .ReturnsAsync(new ChatResponse { Text = "Generated via fallback" });
    //
    //     // Act
    //     var result = await _service.GenerateProfileDescription(userId, request);
    //
    //     // Assert
    //     Assert.Equal("Generated via fallback", result);
    // }

    [Fact]
    public async Task GenerateProfileDescription_Throws_WhenProfileNotFound()
    {
        // Arrange
        var emptySet = EfHelpers.CreateMockDbSet(Enumerable.Empty<CandidateProfile>().AsQueryable());
        _dbMock.Setup(d => d.CandidateProfile).Returns(emptySet.Object);

        var request = new AiDescriptionRequest
        {
            PositionTitle = "Dev",
            Experience = Experience.NoExperience,
            Description = null
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() =>
            _service.GenerateProfileDescription(99, request));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Profile not found", ex.Message);
    }

    // [Fact]
    // public async Task GenerateProfileDescription_SkipsPdf_IfDownloadFails()
    // {
    //     // Arrange
    //     var profile = new CandidateProfile
    //     {
    //         UserId = 10,
    //         Description = "test",
    //         ProfileSkills = new List<ProfileSkills>()
    //     };
    //
    //     var mockSet = EfHelpers.CreateMockDbSet(new[] { profile }.AsQueryable());
    //     _dbMock.Setup(d => d.CandidateProfile).Returns(mockSet.Object);
    //
    //     _pdfServiceMock.Setup(p => p.DownloadPdf(profile)).ThrowsAsync(new IOException("Disk error"));
    //
    //     _chatClientMock.Setup(c => c.GetResponseAsync(It.IsAny<ChatMessage>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
    //         .ReturnsAsync(new ChatResponse { Text = "Safe fallback" });
    //
    //     // Act
    //     var result = await _service.GenerateProfileDescription(10, new AiDescriptionRequest
    //     {
    //         Description = null,
    //         Experience = Experience.NoExperience,
    //         PositionTitle = "Intern"
    //     });
    //
    //     // Assert
    //     Assert.Equal("Safe fallback", result);
    //     _loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("pdf")), It.IsAny<object[]>()), Times.Once);
    // }
}