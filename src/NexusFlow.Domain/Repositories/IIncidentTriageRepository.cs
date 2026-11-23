using NexusFlow.Domain.Entities;

namespace NexusFlow.Domain.Repositories;

public interface IIncidentTriageRepository
{
    Task<IncidentTriage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentTriage?> GetByExecutionIdAsync(Guid executionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncidentTriage>> GetRecentTriagesAsync(int limit = 20, CancellationToken cancellationToken = default);
    Task AddAsync(IncidentTriage triage, CancellationToken cancellationToken = default);
    void Update(IncidentTriage triage);
}
