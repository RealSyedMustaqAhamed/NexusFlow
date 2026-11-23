using FluentValidation;
using MediatR;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Enums;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Workflows.Commands.CreateWorkflow;

public record CreateStepRequest(int Order, string Name, StepType Type, string ConfigJson, int MaxRetries = 3, int TimeoutSeconds = 30);

public record CreateWorkflowCommand(
    string Name,
    string Description,
    string CreatedBy,
    List<CreateStepRequest> Steps) : IRequest<Result<WorkflowDto>>;

public class CreateWorkflowCommandValidator : AbstractValidator<CreateWorkflowCommand>
{
    public CreateWorkflowCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workflow name is required.")
            .MaximumLength(150).WithMessage("Workflow name must not exceed 150 characters.");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Creator username is required.");

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("Workflow must have at least one step.");
    }
}

public class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, Result<WorkflowDto>>
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkflowCommandHandler(IWorkflowRepository workflowRepository, IUnitOfWork unitOfWork)
    {
        _workflowRepository = workflowRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowDto>> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        var workflow = new WorkflowDefinition(request.Name, request.Description, request.CreatedBy);

        foreach (var s in request.Steps.OrderBy(x => x.Order))
        {
            workflow.AddStep(s.Order, s.Name, s.Type, s.ConfigJson, s.MaxRetries, s.TimeoutSeconds);
        }

        await _workflowRepository.AddAsync(workflow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new WorkflowDto(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Version,
            workflow.IsActive,
            workflow.CreatedBy,
            workflow.CreatedAtUtc,
            workflow.Steps.Select(s => new WorkflowStepDto(s.Id, s.StepOrder, s.Name, s.StepType, s.ConfigurationJson, s.MaxRetries, s.TimeoutSeconds)).ToList()
        );

        return Result.Success(dto);
    }
}
