using MediatR;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Workflows.Queries.GetWorkflowById;

public record GetWorkflowByIdQuery(Guid Id) : IRequest<Result<WorkflowDto>>;

public class GetWorkflowByIdQueryHandler : IRequestHandler<GetWorkflowByIdQuery, Result<WorkflowDto>>
{
    private readonly IWorkflowRepository _repository;

    public GetWorkflowByIdQueryHandler(IWorkflowRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkflowDto>> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (workflow == null)
            return Result.Failure<WorkflowDto>($"Workflow with ID '{request.Id}' was not found.");

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
