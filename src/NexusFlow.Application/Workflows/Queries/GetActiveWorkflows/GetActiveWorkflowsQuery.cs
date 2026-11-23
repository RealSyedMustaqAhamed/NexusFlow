using MediatR;
using NexusFlow.Application.Common.Interfaces;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Workflows.Queries.GetActiveWorkflows;

public record GetActiveWorkflowsQuery : IRequest<Result<IReadOnlyList<WorkflowDto>>>;

public class GetActiveWorkflowsQueryHandler : IRequestHandler<GetActiveWorkflowsQuery, Result<IReadOnlyList<WorkflowDto>>>
{
    private readonly IWorkflowRepository _repository;
    private readonly ICacheService _cacheService;
    private const string CacheKey = "workflows:active";

    public GetActiveWorkflowsQueryHandler(IWorkflowRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Result<IReadOnlyList<WorkflowDto>>> Handle(GetActiveWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<IReadOnlyList<WorkflowDto>>(CacheKey, cancellationToken);
        if (cached != null)
            return Result.Success(cached);

        var workflows = await _repository.GetAllActiveAsync(cancellationToken);
        var dtos = workflows.Select(w => new WorkflowDto(
            w.Id,
            w.Name,
            w.Description,
            w.Version,
            w.IsActive,
            w.CreatedBy,
            w.CreatedAtUtc,
            w.Steps.Select(s => new WorkflowStepDto(s.Id, s.StepOrder, s.Name, s.StepType, s.ConfigurationJson, s.MaxRetries, s.TimeoutSeconds)).ToList()
        )).ToList();

        await _cacheService.SetAsync(CacheKey, (IReadOnlyList<WorkflowDto>)dtos, TimeSpan.FromMinutes(2), cancellationToken);

        return Result.Success<IReadOnlyList<WorkflowDto>>(dtos);
    }
}
