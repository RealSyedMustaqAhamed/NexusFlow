using FluentAssertions;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Exceptions;
using Xunit;

namespace NexusFlow.Tests.Domain;

public class WorkflowDefinitionTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInstantiateWorkflow()
    {
        // Act
        var workflow = new WorkflowDefinition("Order-Processing", "Handles payment and dispatch", "Developer");

        // Assert
        workflow.Should().NotBeNull();
        workflow.Name.Should().Be("Order-Processing");
        workflow.Description.Should().Be("Handles payment and dispatch");
        workflow.CreatedBy.Should().Be("Developer");
        workflow.IsActive.Should().BeTrue();
        workflow.Version.Should().Be(1);
        workflow.Steps.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        // Act
        var act = () => new WorkflowDefinition(invalidName!, "Description", "Author");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Workflow name cannot be empty.");
    }

    [Fact]
    public void AddStep_WithValidParameters_ShouldAddStepToWorkflow()
    {
        // Arrange
        var workflow = new WorkflowDefinition("DataSync", "Syncs ERP data", "System");

        // Act
        var step = workflow.AddStep(1, "FetchData", StepType.Webhook, "{\"url\": \"https://erp.internal/api\"}", retries: 2, timeoutSec: 15);

        // Assert
        workflow.Steps.Should().HaveCount(1);
        step.Name.Should().Be("FetchData");
        step.StepOrder.Should().Be(1);
        step.StepType.Should().Be(StepType.Webhook);
        step.MaxRetries.Should().Be(2);
        step.TimeoutSeconds.Should().Be(15);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var workflow = new WorkflowDefinition("DataSync", "Syncs ERP data", "System");

        // Act
        workflow.Deactivate();

        // Assert
        workflow.IsActive.Should().BeFalse();
        workflow.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void IncrementVersion_ShouldIncreaseVersionNumber()
    {
        // Arrange
        var workflow = new WorkflowDefinition("DataSync", "Syncs ERP data", "System");

        // Act
        workflow.IncrementVersion();

        // Assert
        workflow.Version.Should().Be(2);
    }
}
