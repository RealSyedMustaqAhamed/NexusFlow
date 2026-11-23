using NexusFlow.Domain.Common;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Events;

namespace NexusFlow.Domain.Entities;

public class IncidentTriage : BaseEntity
{
    public Guid WorkflowExecutionId { get; private set; }
    public string IncidentKey { get; private set; } = string.Empty;
    public IncidentSeverity Severity { get; private set; }
    public string RootCauseCategory { get; private set; } = string.Empty;
    public double ConfidenceScore { get; private set; }
    public string AiSummary { get; private set; } = string.Empty;
    public string RecommendedAction { get; private set; } = string.Empty;
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    protected IncidentTriage() { }

    public IncidentTriage(
        Guid workflowExecutionId,
        string incidentKey,
        IncidentSeverity severity,
        string rootCauseCategory,
        double confidenceScore,
        string aiSummary,
        string recommendedAction)
    {
        WorkflowExecutionId = workflowExecutionId;
        IncidentKey = string.IsNullOrWhiteSpace(incidentKey) ? $"INC-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}" : incidentKey;
        Severity = severity;
        RootCauseCategory = rootCauseCategory;
        ConfidenceScore = Math.Clamp(confidenceScore, 0.0, 1.0);
        AiSummary = aiSummary;
        RecommendedAction = recommendedAction;
        IsResolved = false;

        AddDomainEvent(new IncidentTriageCreatedEvent(Id, workflowExecutionId, severity.ToString()));
    }

    public void MarkResolved()
    {
        IsResolved = true;
        ResolvedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }
}
