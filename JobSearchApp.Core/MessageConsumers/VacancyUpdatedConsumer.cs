using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.MessageContracts;
using MassTransit;
using Serilog;

namespace JobSearchApp.Core.MessageConsumers;

public class VacancyUpdatedConsumer : IConsumer<VacancyUpdatedEvent>
{
    private readonly ILogger _logger;
    private readonly IEmbeddingService _embeddingService;

    public VacancyUpdatedConsumer(ILogger logger, IEmbeddingService embeddingService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
    }

    public async Task Consume(ConsumeContext<VacancyUpdatedEvent> context)
    {
        var vacancyId = context.Message.Id;
        _logger.Information("Consuming VacancyUpdatedEvent with id {VacancyId}", vacancyId);

        await _embeddingService.GenerateEmbeddingForVacancy(vacancyId);

        _logger.Information("VacancyUpdatedEvent with id {VacancyId} consumed", vacancyId);
    }
}