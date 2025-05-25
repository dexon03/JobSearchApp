using AutoMapper;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.SkillServiceTests;

public class CreateSkillTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly SkillService _service;

    public CreateSkillTests()
    {
        _dbMock = new Mock<IAppDbContext>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<IFusionCache>();
        _loggerMock = new Mock<ILogger>();

        _loggerMock.Setup(l => l.ForContext<SkillService>()).Returns(_loggerMock.Object);

        _service = new SkillService(
            _dbMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateSkill_AddsSkill_AndReturnsDto()
    {
        // Arrange
        var createDto = new SkillCreateDto { Name = "C#" };
        var entity = new Skill { Id = 1, Name = "C#" };
        var dto = new SkillDto { Id = 1, Name = "C#" };

        _mapperMock.Setup(m => m.Map<Skill>(createDto)).Returns(entity);
        _mapperMock.Setup(m => m.Map<SkillDto>(entity)).Returns(dto);

        var mockSet = new Mock<DbSet<Skill>>();
        _dbMock.Setup(d => d.Skills).Returns(mockSet.Object);
        mockSet.Setup(s => s.Add(entity));

        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.CreateSkill(createDto);

        // Assert
        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Name, result.Name);

        mockSet.Verify(s => s.Add(entity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("New skill {SkillName} created. Cache invalidated.", "C#"), Times.Once);
    }
}