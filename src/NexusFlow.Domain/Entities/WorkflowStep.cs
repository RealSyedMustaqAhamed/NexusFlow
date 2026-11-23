using NexusFlow.Domain.Common;
using NexusFlow.Domain.Enums;

namespace NexusFlow.Domain.Entities;

public class WorkflowStep : BaseEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public int StepOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public StepType StepType { get; private set; }
    public string ConfigurationJson { get; private set; } = "{}";
    public int MaxRetries { get; private set; } = 3;
    public int TimeoutSeconds { get; private set; } = 30;

    protected WorkflowStep() { }

    public WorkflowStep(Guid workflowDefinitionId, int order, string name, StepType type, string configJson, int maxRetries, int timeoutSeconds)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        StepOrder = order;
        Name = name;
        StepType = type;
        ConfigurationJson = string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson;
        MaxRetries = Math.Max(0, maxRetries);
        TimeoutSeconds = Math.Max(1, timeoutSeconds);
    }
}
