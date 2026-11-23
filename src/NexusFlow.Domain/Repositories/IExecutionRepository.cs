using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Enums;

namespace NexusFlow.Domain.Repositories;

public interface IExecutionRepository
{
    Task<WorkflowExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowExecution>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowExecution>> GetPendingExecutionsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(WorkflowExecution execution, CancellationToken cancellationToken = default);
    void Update(WorkflowExecution execution);
}
