using NexusFlow.Domain.Enums;

namespace NexusFlow.Application.DTOs;

public record WorkflowStepDto(
    Guid Id,
    int StepOrder,
    string Name,
    StepType StepType,
    string ConfigurationJson,
    int MaxRetries,
    int TimeoutSeconds);

public record WorkflowDto(
    Guid Id,
    string Name,
    string Description,
    int Version,
    bool IsActive,
    string CreatedBy,
    DateTime CreatedAtUtc,
    IReadOnlyList<WorkflowStepDto> Steps);

public record StepExecutionDto(
    Guid Id,
    Guid WorkflowStepId,
    ExecutionStatus Status,
    int AttemptCount,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? OutputDataJson,
    string? ExecutionLogs);

public record WorkflowExecutionDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    ExecutionStatus Status,
    string TriggerSource,
    string InputPayloadJson,
    string? OutputPayloadJson,
    string? ErrorMessage,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    IReadOnlyList<StepExecutionDto> StepExecutions);

public record IncidentTriageDto(
    Guid Id,
    Guid WorkflowExecutionId,
    string IncidentKey,
    IncidentSeverity Severity,
    string RootCauseCategory,
    double ConfidenceScore,
    string AiSummary,
    string RecommendedAction,
    bool IsResolved,
    DateTime? ResolvedAtUtc,
    DateTime CreatedAtUtc);
