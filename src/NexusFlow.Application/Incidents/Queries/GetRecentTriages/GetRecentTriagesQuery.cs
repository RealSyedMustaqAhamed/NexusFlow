using MediatR;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Incidents.Queries.GetRecentTriages;

public record GetRecentTriagesQuery(int Limit = 20) : IRequest<Result<IReadOnlyList<IncidentTriageDto>>>;

public class GetRecentTriagesQueryHandler : IRequestHandler<GetRecentTriagesQuery, Result<IReadOnlyList<IncidentTriageDto>>>
{
    private readonly IIncidentTriageRepository _repository;

    public GetRecentTriagesQueryHandler(IIncidentTriageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<IncidentTriageDto>>> Handle(GetRecentTriagesQuery request, CancellationToken cancellationToken)
    {
        var triages = await _repository.GetRecentTriagesAsync(request.Limit, cancellationToken);
        var dtos = triages.Select(t => new IncidentTriageDto(
            t.Id,
            t.WorkflowExecutionId,
            t.IncidentKey,
            t.Severity,
            t.RootCauseCategory,
            t.ConfidenceScore,
            t.AiSummary,
            t.RecommendedAction,
            t.IsResolved,
            t.ResolvedAtUtc,
            t.CreatedAtUtc
        )).ToList();

        return Result.Success<IReadOnlyList<IncidentTriageDto>>(dtos);
    }
}
