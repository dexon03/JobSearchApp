using System.Net;
using AutoMapper;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Exceptions;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Profiles;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Moq;
using Serilog;

namespace JobSearchApp.UnitTests.Profiles.ProfileServiceTests;

public class UpdateRecruiterProfileTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ProfileService _service;

    public UpdateRecruiterProfileTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        var loggerMock = new Mock<ILogger>();
        var chatClientMock = new Mock<IChatClient>();
        var pdfServiceMock = new Mock<IPdfService>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();

        loggerMock.Setup(l => l.ForContext<ProfileService>()).Returns(loggerMock.Object);

        _service = new ProfileService(
            _dbMock.Object,
            _mapperMock.Object,
            chatClientMock.Object,
            loggerMock.Object,
            pdfServiceMock.Object,
            publishEndpointMock.Object
        );
    }

    [Fact]
    public async Task UpdateRecruiterProfile_Updates_And_ReturnsDto()
    {
        // Arrange
        var profile = new RecruiterProfile
        {
            Id = 1,
            Name = "Old",
            Email = "old@example.com"
        };

        var dto = new RecruiterProfileUpdateDto
        {
            Id = 1,
            Name = "New",
            Surname = "New",
            Email = "new@example.com"
        };

        var mockSet = new Mock<DbSet<RecruiterProfile>>();
        _dbMock.Setup(d => d.RecruiterProfile.FindAsync(dto.Id))
               .ReturnsAsync(profile);

        _mapperMock.Setup(m => m.Map(dto, profile));
        _dbMock.Setup(d => d.RecruiterProfile.Update(profile));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var expectedDto = new GetRecruiterProfileDto { Id = 1, Name = "New", Email = "new@example.com" };
        _mapperMock.Setup(m => m.Map<GetRecruiterProfileDto>(profile)).Returns(expectedDto);

        // Act
        var result = await _service.UpdateRecruiterProfile(dto);

        // Assert
        Assert.Equal(expectedDto.Name, result.Name);
        Assert.Equal(expectedDto.Email, result.Email);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRecruiterProfile_Throws_When_NotFound()
    {
        // Arrange
        var dto = new RecruiterProfileUpdateDto { Id = 999, Surname = "NonExistent", Name = "NonExistent" };

        _dbMock.Setup(d => d.RecruiterProfile.FindAsync(dto.Id))
               .ReturnsAsync((RecruiterProfile)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ExceptionWithStatusCode>(() => _service.UpdateRecruiterProfile(dto));
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("not exist", ex.Message);
    }
}