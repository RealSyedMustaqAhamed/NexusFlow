using NexusFlow.Domain.Entities;

namespace NexusFlow.Domain.Repositories;

public interface IWorkflowRepository
{
    Task<WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowDefinition>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default);
    void Update(WorkflowDefinition workflow);
}
