using AutoMapper;
using Helpers;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.SkillServiceTests;

public class UpdateSkillTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly SkillService _service;

    public UpdateSkillTests()
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
    public async Task UpdateSkill_WhenExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var updateDto = new SkillUpdateDto { Id = 1, Name = "Updated Skill" };
        var skillEntity = new Skill { Id = 1, Name = "Updated Skill" };
        var expectedDto = new SkillDto { Id = 1, Name = "Updated Skill" };

        _mapperMock.Setup(m => m.Map<Skill>(updateDto)).Returns(skillEntity);
        _mapperMock.Setup(m => m.Map<SkillDto>(skillEntity)).Returns(expectedDto);

        var skills = new List<Skill> { skillEntity }.AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(skills);
        _dbMock.Setup(d => d.Skills).Returns(mockSet.Object);

        _dbMock.Setup(d => d.Skills.Update(skillEntity));
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c => c.RemoveByTagAsync($"skill_{skillEntity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var result = await _service.UpdateSkill(updateDto);

        // Assert
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);

        _dbMock.Verify(d => d.Skills.Update(skillEntity), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"skill_{skillEntity.Id}", It.IsAny<FusionCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Skill {SkillId} updated. Cache invalidated.", skillEntity.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateSkill_Throws_WhenNotFound()
    {
        // Arrange
        var updateDto = new SkillUpdateDto { Id = 99, Name = "Non-existent" };
        var skillEntity = new Skill { Id = 99, Name = "Non-existent" };

        _mapperMock.Setup(m => m.Map<Skill>(updateDto)).Returns(skillEntity);

        var emptyList = new List<Skill>().AsQueryable();
        var mockSet = EfHelpers.CreateMockDbSet(emptyList);
        _dbMock.Setup(d => d.Skills).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.UpdateSkill(updateDto));
        Assert.Equal("Skill not found", ex.Message);
        _loggerMock.Verify(l => l.Warning("Attempt to update non-existing skill {SkillId}.", 99), Times.Once);
    }
}