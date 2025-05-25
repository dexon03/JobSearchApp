using System.Diagnostics.CodeAnalysis;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.MessageContracts;
using MassTransit;
using Serilog;

namespace JobSearchApp.Core.MessageConsumers;

[ExcludeFromCodeCoverage]
public class VacancyCreatedConsumer : IConsumer<VacancyCreatedEvent>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger _logger;

    public VacancyCreatedConsumer(
        IEmbeddingService embeddingService,
        ILogger logger)
    {
        _embeddingService = embeddingService;
        _logger = logger.ForContext<VacancyCreatedConsumer>();
    }

    public async Task Consume(ConsumeContext<VacancyCreatedEvent> context)
    {
        var vacancyId = context.Message.Id;
        _logger.Information("Consuming VacancyCreatedEvent with id {VacancyId}", vacancyId);

        await _embeddingService.GenerateEmbeddingForVacancy(vacancyId);

        _logger.Information("VacancyCreatedEvent with id {VacancyId} consumed", vacancyId);
    }
}