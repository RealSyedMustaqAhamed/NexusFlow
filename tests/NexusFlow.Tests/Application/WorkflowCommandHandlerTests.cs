using FluentAssertions;
using Moq;
using NexusFlow.Application.DTOs;
using NexusFlow.Application.Workflows.Commands.CreateWorkflow;
using NexusFlow.Application.Workflows.Commands.TriggerWorkflow;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Repositories;
using Xunit;

namespace NexusFlow.Tests.Application;

public class WorkflowCommandHandlerTests
{
    private readonly Mock<IWorkflowRepository> _workflowRepoMock;
    private readonly Mock<IExecutionRepository> _executionRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public WorkflowCommandHandlerTests()
    {
        _workflowRepoMock = new Mock<IWorkflowRepository>();
        _executionRepoMock = new Mock<IExecutionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task CreateWorkflow_ShouldPersistWorkflowAndReturnDto()
    {
        // Arrange
        var handler = new CreateWorkflowCommandHandler(_workflowRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateWorkflowCommand(
            "TelemetryPipeline",
            "Processes IoT metrics",
            "Syed",
            new List<CreateStepRequest>
            {
                new(1, "WebhookTrigger", StepType.Webhook, "{}", 3, 30)
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("TelemetryPipeline");
        result.Value.Steps.Should().HaveCount(1);
        _workflowRepoMock.Verify(r => r.AddAsync(It.IsAny<WorkflowDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TriggerWorkflow_WhenWorkflowNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        _workflowRepoMock.Setup(r => r.GetByIdAsync(workflowId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinition?)null);

        var handler = new TriggerWorkflowCommandHandler(_workflowRepoMock.Object, _executionRepoMock.Object, _unitOfWorkMock.Object);
        var command = new TriggerWorkflowCommand(workflowId, "Webhook", "{}");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _executionRepoMock.Verify(r => r.AddAsync(It.IsAny<WorkflowExecution>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TriggerWorkflow_WhenWorkflowIsActive_ShouldCreateExecution()
    {
        // Arrange
        var workflow = new WorkflowDefinition("ActiveWorkflow", "Desc", "Author");
        workflow.AddStep(1, "Step1", StepType.DataTransform, "{}");

        _workflowRepoMock.Setup(r => r.GetByIdAsync(workflow.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var handler = new TriggerWorkflowCommandHandler(_workflowRepoMock.Object, _executionRepoMock.Object, _unitOfWorkMock.Object);
        var command = new TriggerWorkflowCommand(workflow.Id, "Webhook", "{\"metric\": 42}");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WorkflowDefinitionId.Should().Be(workflow.Id);
        result.Value.Status.Should().Be(ExecutionStatus.Pending);
        result.Value.StepExecutions.Should().HaveCount(1);
        _executionRepoMock.Verify(r => r.AddAsync(It.IsAny<WorkflowExecution>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
