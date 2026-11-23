using System.Text.Json;
using Microsoft.Extensions.Logging;
using NexusFlow.Application.Common.Interfaces;
using NexusFlow.Domain.Enums;

namespace NexusFlow.Infrastructure.Services;

public class AiTriageAgentService : IAiTriageAgentService
{
    private readonly ILogger<AiTriageAgentService> _logger;

    public AiTriageAgentService(ILogger<AiTriageAgentService> logger)
    {
        _logger = logger;
    }

    public Task<AiTriageResult> AnalyzeIncidentAsync(string incidentDescription, string errorLogs, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Agentic AI triage analyzing incident: {Description}", incidentDescription);

        var combinedText = $"{incidentDescription} {errorLogs}".ToLowerInvariant();

        IncidentSeverity severity;
        string rootCause;
        double confidence;
        string summary;
        string action;

        if (combinedText.Contains("deadlock") || combinedText.Contains("fatal") || combinedText.Contains("out of memory") || combinedText.Contains("service unavailable 503"))
        {
            severity = IncidentSeverity.Critical;
            rootCause = "Database Exhaustion / High-Severity Infrastructure Failure";
            confidence = 0.94;
            summary = "Critical operational disruption detected. Resource saturation or transactional deadlock blocked worker threads.";
            action = "Scale horizontal pods, terminate orphaned lock transactions, and trigger circuit breaker on dependent downstream services.";
        }
        else if (combinedText.Contains("timeout") || combinedText.Contains("gateway") || combinedText.Contains("504"))
        {
            severity = IncidentSeverity.High;
            rootCause = "Downstream API Latency / Network Timeout";
            confidence = 0.89;
            summary = "Downstream dependency exceeded execution SLA threshold. Requests accumulating in retry buffer.";
            action = "Activate fallback circuit breaker with Polly, flush transient queue, and verify downstream health endpoint.";
        }
        else if (combinedText.Contains("nullreference") || combinedText.Contains("validation") || combinedText.Contains("bad request 400"))
        {
            severity = IncidentSeverity.Medium;
            rootCause = "Schema Incompatibility / Payload Validation Failure";
            confidence = 0.92;
            summary = "Contract mismatch detected in incoming workflow payload. Missing mandatory properties.";
            action = "Quarantine malformed message to dead-letter queue, notify producer client, and verify API contract version.";
        }
        else
        {
            severity = IncidentSeverity.Low;
            rootCause = "Transient Application Warning";
            confidence = 0.78;
            summary = "Non-blocking diagnostic warning. System operational within standard error budget.";
            action = "Log for weekly telemetry review. No immediate manual intervention needed.";
        }

        var result = new AiTriageResult(severity, rootCause, confidence, summary, action);
        return Task.FromResult(result);
    }
}
