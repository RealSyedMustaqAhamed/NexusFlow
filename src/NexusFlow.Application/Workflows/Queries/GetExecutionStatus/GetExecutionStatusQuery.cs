using MediatR;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Workflows.Queries.GetExecutionStatus;

public record GetExecutionStatusQuery(Guid ExecutionId) : IRequest<Result<WorkflowExecutionDto>>;

public class GetExecutionStatusQueryHandler : IRequestHandler<GetExecutionStatusQuery, Result<WorkflowExecutionDto>>
{
    private readonly IExecutionRepository _repository;

    public GetExecutionStatusQueryHandler(IExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WorkflowExecutionDto>> Handle(GetExecutionStatusQuery request, CancellationToken cancellationToken)
    {
        var execution = await _repository.GetByIdAsync(request.ExecutionId, cancellationToken);
        if (execution == null)
            return Result.Failure<WorkflowExecutionDto>($"Execution with ID '{request.ExecutionId}' was not found.");

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
