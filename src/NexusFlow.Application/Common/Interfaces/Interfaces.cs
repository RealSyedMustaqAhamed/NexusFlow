using NexusFlow.Domain.Enums;

namespace NexusFlow.Application.Common.Interfaces;

public record AiTriageResult(
    IncidentSeverity Severity,
    string RootCauseCategory,
    double ConfidenceScore,
    string Summary,
    string RecommendedAction);

public interface IAiTriageAgentService
{
    Task<AiTriageResult> AnalyzeIncidentAsync(string incidentDescription, string errorLogs, CancellationToken cancellationToken = default);
}

public interface IWebhookDispatcher
{
    Task<bool> DispatchAsync(string targetUrl, string payloadJson, CancellationToken cancellationToken = default);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
