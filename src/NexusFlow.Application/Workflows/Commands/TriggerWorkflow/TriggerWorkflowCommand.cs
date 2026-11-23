using FluentValidation;
using MediatR;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Workflows.Commands.TriggerWorkflow;

public record TriggerWorkflowCommand(
    Guid WorkflowId,
    string TriggerSource,
    string PayloadJson) : IRequest<Result<WorkflowExecutionDto>>;

public class TriggerWorkflowCommandValidator : AbstractValidator<TriggerWorkflowCommand>
{
    public TriggerWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty().WithMessage("Workflow ID is required.");
        RuleFor(x => x.TriggerSource).NotEmpty().WithMessage("Trigger source is required.");
    }
}

public class TriggerWorkflowCommandHandler : IRequestHandler<TriggerWorkflowCommand, Result<WorkflowExecutionDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IExecutionRepository _executionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TriggerWorkflowCommandHandler(
        IWorkflowRepository workflowRepository,
        IExecutionRepository executionRepository,
        IUnitOfWork unitOfWork)
    {
        _workflowRepository = workflowRepository;
        _executionRepository = executionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowExecutionDto>> Handle(TriggerWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
        if (workflow == null)
            return Result.Failure<WorkflowExecutionDto>($"Workflow with ID '{request.WorkflowId}' was not found.");

        if (!workflow.IsActive)
            return Result.Failure<WorkflowExecutionDto>("Cannot trigger an inactive workflow.");

        var execution = new WorkflowExecution(workflow.Id, request.TriggerSource, request.PayloadJson);

        foreach (var step in workflow.Steps.OrderBy(s => s.StepOrder))
        {
            execution.AddStepExecution(step.Id, attempt: 1);
        }

        await _executionRepository.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new WorkflowExecutionDto(
            execution.Id,
            execution.WorkflowDefinitionId,
            execution.Status,
            execution.TriggerSource,
            execution.InputPayloadJson,
            execution.OutputPayloadJson,
            execution.ErrorMessage,
            execution.StartedAtUtc,
            execution.CompletedAtUtc,
            execution.StepExecutions.Select(s => new StepExecutionDto(
                s.Id, s.WorkflowStepId, s.Status, s.AttemptCount, s.StartedAtUtc, s.CompletedAtUtc, s.OutputDataJson, s.ExecutionLogs)).ToList()
        );

        return Result.Success(dto);
    }
}
