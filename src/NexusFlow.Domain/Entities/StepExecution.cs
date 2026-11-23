using NexusFlow.Domain.Common;
using NexusFlow.Domain.Enums;

namespace NexusFlow.Domain.Entities;

public class StepExecution : BaseEntity
{
    public Guid WorkflowExecutionId { get; private set; }
    public Guid WorkflowStepId { get; private set; }
    public ExecutionStatus Status { get; private set; } = ExecutionStatus.Pending;
    public int AttemptCount { get; private set; } = 1;
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? OutputDataJson { get; private set; }
    public string? ExecutionLogs { get; private set; }

    protected StepExecution() { }

    public StepExecution(Guid workflowExecutionId, Guid workflowStepId, int attemptCount)
    {
        WorkflowExecutionId = workflowExecutionId;
        WorkflowStepId = workflowStepId;
        AttemptCount = attemptCount;
        Status = ExecutionStatus.Pending;
    }

    public void Start()
    {
        Status = ExecutionStatus.Running;
        StartedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }

    public void Complete(string outputData, string? logs = null)
    {
        Status = ExecutionStatus.Completed;
        OutputDataJson = outputData;
        ExecutionLogs = logs;
        CompletedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }

    public void Fail(string errorLogs)
    {
        Status = ExecutionStatus.Failed;
        ExecutionLogs = errorLogs;
        CompletedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }
}
