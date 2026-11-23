using NexusFlow.Domain.Common;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Events;
using NexusFlow.Domain.Exceptions;

namespace NexusFlow.Domain.Entities;

public class WorkflowExecution : BaseEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public ExecutionStatus Status { get; private set; } = ExecutionStatus.Pending;
    public string TriggerSource { get; private set; } = "API";
    public string InputPayloadJson { get; private set; } = "{}";
    public string? OutputPayloadJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    private readonly List<StepExecution> _stepExecutions = new();
    public IReadOnlyCollection<StepExecution> StepExecutions => _stepExecutions.AsReadOnly();

    protected WorkflowExecution() { }

    public WorkflowExecution(Guid workflowDefinitionId, string triggerSource, string inputPayloadJson)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        TriggerSource = string.IsNullOrWhiteSpace(triggerSource) ? "Manual" : triggerSource.Trim();
        InputPayloadJson = string.IsNullOrWhiteSpace(inputPayloadJson) ? "{}" : inputPayloadJson;
        Status = ExecutionStatus.Pending;
    }

    public void Start()
    {
        if (Status != ExecutionStatus.Pending)
            throw new DomainException($"Cannot start execution in state {Status}.");

        Status = ExecutionStatus.Running;
        StartedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }

    public void Complete(string outputJson)
    {
        Status = ExecutionStatus.Completed;
        OutputPayloadJson = outputJson;
        CompletedAtUtc = DateTime.UtcNow;
        SetUpdated();
        AddDomainEvent(new WorkflowExecutionCompletedEvent(Id, WorkflowDefinitionId, true));
    }

    public void Fail(string errorMessage)
    {
        Status = ExecutionStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAtUtc = DateTime.UtcNow;
        SetUpdated();
        AddDomainEvent(new WorkflowExecutionCompletedEvent(Id, WorkflowDefinitionId, false));
    }

    public StepExecution AddStepExecution(Guid workflowStepId, int attempt = 1)
    {
        var stepExec = new StepExecution(Id, workflowStepId, attempt);
        _stepExecutions.Add(stepExec);
        return stepExec;
    }
}
