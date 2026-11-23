namespace NexusFlow.Domain.Enums;

public enum StepType
{
    Webhook = 1,
    DataTransform = 2,
    FilterRule = 3,
    AiAgentTriage = 4,
    EmailNotification = 5
}

public enum ExecutionStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Suspended = 5
}

public enum IncidentSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
