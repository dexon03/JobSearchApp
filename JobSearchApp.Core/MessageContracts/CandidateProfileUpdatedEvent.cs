using System.Diagnostics.CodeAnalysis;

namespace JobSearchApp.Core.MessageContracts;

[ExcludeFromCodeCoverage]
public class CandidateProfileUpdatedEvent
{
    public int ProfileId { get; set; }
}