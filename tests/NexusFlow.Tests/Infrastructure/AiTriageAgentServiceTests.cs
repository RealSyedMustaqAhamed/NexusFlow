using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NexusFlow.Domain.Enums;
using NexusFlow.Infrastructure.Services;
using Xunit;

namespace NexusFlow.Tests.Infrastructure;

public class AiTriageAgentServiceTests
{
    private readonly AiTriageAgentService _service;

    public AiTriageAgentServiceTests()
    {
        var loggerMock = new Mock<ILogger<AiTriageAgentService>>();
        _service = new AiTriageAgentService(loggerMock.Object);
    }

    [Fact]
    public async Task AnalyzeIncident_WithDeadlockKeyword_ShouldReturnCriticalSeverity()
    {
        // Arrange
        var desc = "Transaction was deadlocked on lock resources with another process";
        var logs = "SqlException: Error 1205. Deadlock victim";

        // Act
        var result = await _service.AnalyzeIncidentAsync(desc, logs);

        // Assert
        result.Severity.Should().Be(IncidentSeverity.Critical);
        result.ConfidenceScore.Should().BeGreaterThan(0.9);
        result.RootCauseCategory.Should().Contain("Database");
        result.RecommendedAction.Should().Contain("circuit breaker");
    }

    [Fact]
    public async Task AnalyzeIncident_WithTimeoutKeyword_ShouldReturnHighSeverity()
    {
        // Arrange
        var desc = "Gateway timeout connecting to payment gateway";
        var logs = "HttpClient 504 Gateway Timeout after 30000ms";

        // Act
        var result = await _service.AnalyzeIncidentAsync(desc, logs);

        // Assert
        result.Severity.Should().Be(IncidentSeverity.High);
        result.RootCauseCategory.Should().Contain("Latency");
        result.RecommendedAction.Should().Contain("Polly");
    }

    [Fact]
    public async Task AnalyzeIncident_WithValidationKeyword_ShouldReturnMediumSeverity()
    {
        // Arrange
        var desc = "Payload validation failure on customer order";
        var logs = "ValidationException: Missing mandatory field CustomerId";

        // Act
        var result = await _service.AnalyzeIncidentAsync(desc, logs);

        // Assert
        result.Severity.Should().Be(IncidentSeverity.Medium);
        result.RootCauseCategory.Should().Contain("Validation");
        result.RecommendedAction.Should().Contain("dead-letter");
    }
}
