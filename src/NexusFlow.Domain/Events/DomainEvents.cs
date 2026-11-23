using NexusFlow.Domain.Common;

namespace NexusFlow.Domain.Events;

public sealed record WorkflowExecutionCompletedEvent(Guid ExecutionId, Guid WorkflowId, bool Succeeded) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record IncidentTriageCreatedEvent(Guid TriageId, Guid ExecutionId, string Severity) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
