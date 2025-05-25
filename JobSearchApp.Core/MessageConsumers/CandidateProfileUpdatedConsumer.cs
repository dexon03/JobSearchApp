using System.Diagnostics.CodeAnalysis;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.MessageContracts;
using MassTransit;
using Serilog;

namespace JobSearchApp.Core.MessageConsumers;
[ExcludeFromCodeCoverage]
public class CandidateProfileUpdatedConsumer : IConsumer<CandidateProfileUpdatedEvent>
{
    private readonly IEmbeddingService _embedding;
    private readonly ILogger _logger;

    public CandidateProfileUpdatedConsumer(IEmbeddingService embedding, ILogger logger)
    {
        _embedding = embedding;
        _logger = logger.ForContext<CandidateProfileUpdatedConsumer>();
    }

    public async Task Consume(ConsumeContext<CandidateProfileUpdatedEvent> context)
    {
        var profileId = context.Message.ProfileId;

        _logger.Information("Consuming CandidateProfileUpdatedEvent with id {ProfileId}", profileId);

        await _embedding.GenerateEmbeddingForCandidate(profileId);

        _logger.Information("CandidateProfileUpdatedEvent with id {ProfileId} consumed", profileId);
    }
}