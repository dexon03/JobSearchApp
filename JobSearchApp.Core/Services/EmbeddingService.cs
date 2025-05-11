using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Data;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Pgvector;
using Serilog;

namespace JobSearchApp.Core.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly AppDbContext _dbContext;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ILogger _logger;

    public EmbeddingService(
        AppDbContext dbContext,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        ILogger logger)
    {
        _dbContext = dbContext;
        _embeddingGenerator = embeddingGenerator;
        _logger = logger;
    }
    public async Task GenerateEmbeddingForVacancy(int id)
    {
        var vacancy = await _dbContext.Vacancies
            .Include(x => x.VacancySkill).ThenInclude(x => x.Skill)
            .Include(x => x.LocationVacancy).ThenInclude(x => x.Location)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vacancy is null)
        {
            _logger.Error("Vacancy with id {VacancyId} not found", id);
            return;
        }

        _logger.Information("Generating embedding for vacancy {VacancyId}", vacancy.Id);

        await GenerateEmbedding(vacancy);

        _dbContext.Update(vacancy);
        await _dbContext.SaveChangesAsync();
        _logger.Information("Embedding generated for vacancy {VacancyId}", vacancy.Id);
    }
    
    private async Task GenerateEmbedding(Vacancy vacancy)
    {
        var skills = vacancy.VacancySkill.Select(x => x.Skill.Name);
        var locations = vacancy.LocationVacancy.Select(x => $"{x.Location.Country}_{x.Location.City}");
        string prompt = $"""
                         Title: {vacancy.Title}
                         Description: {vacancy.Description}

                         Experience: {vacancy.Experience}
                         AttendanceMode: {vacancy.AttendanceMode}
                         Salary: {vacancy.Salary}

                         Skills: {skills}
                         Locations: {locations}
                         """;

        var embedding = await _embeddingGenerator.GenerateAsync([prompt]);
        vacancy.Embedding = new Vector(embedding.Single().Vector);
    }
}