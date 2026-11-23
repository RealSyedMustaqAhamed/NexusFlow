using FluentAssertions;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Events;
using NexusFlow.Domain.Exceptions;
using Xunit;

namespace NexusFlow.Tests.Domain;

public class WorkflowExecutionTests
{
    [Fact]
    public void Start_WhenPending_ShouldSetStatusToRunning()
    {
        // Arrange
        var execution = new WorkflowExecution(Guid.NewGuid(), "API", "{\"orderId\": 123}");

        // Act
        execution.Start();

        // Assert
        execution.Status.Should().Be(ExecutionStatus.Running);
        execution.StartedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ShouldThrowDomainException()
    {
        // Arrange
        var execution = new WorkflowExecution(Guid.NewGuid(), "API", "{\"orderId\": 123}");
        execution.Start();

        // Act
        var act = () => execution.Start();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot start execution in state Running*");
    }

    [Fact]
    public void Complete_ShouldSetStatusCompletedAndRaiseDomainEvent()
    {
        // Arrange
        var execution = new WorkflowExecution(Guid.NewGuid(), "Cron", "{}");
        execution.Start();

        // Act
        execution.Complete("{\"result\": \"success\"}");

        // Assert
        execution.Status.Should().Be(ExecutionStatus.Completed);
        execution.CompletedAtUtc.Should().NotBeNull();
        execution.OutputPayloadJson.Should().Be("{\"result\": \"success\"}");
        execution.DomainEvents.Should().ContainSingle(e => e is WorkflowExecutionCompletedEvent);
    }

    [Fact]
    public void Fail_ShouldSetStatusFailedAndRecordErrorMessage()
    {
        // Arrange
        var execution = new WorkflowExecution(Guid.NewGuid(), "Cron", "{}");
        execution.Start();

        // Act
        execution.Fail("Database timeout exception");

        // Assert
        execution.Status.Should().Be(ExecutionStatus.Failed);
        execution.ErrorMessage.Should().Be("Database timeout exception");
        execution.DomainEvents.Should().ContainSingle(e => e is WorkflowExecutionCompletedEvent);
    }
}
