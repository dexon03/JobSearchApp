using Pgvector;

namespace JobSearchApp.Core.Contracts.Common;

public interface IEmbeddingService
{
    Task GenerateEmbeddingForVacancy(int id);
    Task GenerateEmbeddingForCandidate(int id);
    Task<Vector> GenerateEmbeddingForSearchTerm(string searchTerm);
}