using AutoMapper;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Common;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

namespace JobSearchApp.UnitTests.Vacancies.SkillServiceTests;

public class DeleteSkillTests
{
    private readonly Mock<IAppDbContext> _dbMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IFusionCache> _cacheMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly SkillService _service;

    public DeleteSkillTests()
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
    public async Task DeleteSkill_WhenFound_Removes_AndInvalidatesCache()
    {
        // Arrange
        var skill = new Skill { Id = 1, Name = "C#" };
        var mockSet = new Mock<DbSet<Skill>>();

        _dbMock.Setup(d => d.Skills).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(skill);
        _dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _cacheMock.Setup(c =>
                c.RemoveByTagAsync($"skill_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        _cacheMock.Setup(c =>
                c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await _service.DeleteSkill(1);

        // Assert
        mockSet.Verify(m => m.Remove(skill), Times.Once);
        _dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync($"skill_1", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveByTagAsync("skills", It.IsAny<FusionCacheEntryOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _loggerMock.Verify(l => l.Information("Skill {SkillId} deleted. Cache invalidated.", 1), Times.Once);
    }

    [Fact]
    public async Task DeleteSkill_Throws_WhenNotFound()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Skill>>();
        _dbMock.Setup(d => d.Skills).Returns(mockSet.Object);
        mockSet.Setup(m => m.FindAsync(999)).ReturnsAsync((Skill)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteSkill(999));
        Assert.Equal("Skill not found", ex.Message);
        _loggerMock.Verify(l => l.Warning("Attempt to delete non-existing skill {SkillId}.", 999), Times.Once);
    }
}