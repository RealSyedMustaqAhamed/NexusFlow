using MediatR;
using NexusFlow.Application.Common.Interfaces;
using NexusFlow.Application.DTOs;
using NexusFlow.Domain.Common;
using NexusFlow.Domain.Entities;
using NexusFlow.Domain.Repositories;

namespace NexusFlow.Application.Incidents.Commands.TriageIncident;

public record TriageIncidentCommand(
    Guid ExecutionId,
    string IncidentKey,
    string IncidentDescription,
    string ErrorLogs) : IRequest<Result<IncidentTriageDto>>;

public class TriageIncidentCommandHandler : IRequestHandler<TriageIncidentCommand, Result<IncidentTriageDto>>
{
    private readonly IAiTriageAgentService _aiAgent;
    private readonly IIncidentTriageRepository _triageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TriageIncidentCommandHandler(
        IAiTriageAgentService aiAgent,
        IIncidentTriageRepository triageRepository,
        IUnitOfWork unitOfWork)
    {
        _aiAgent = aiAgent;
        _triageRepository = triageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IncidentTriageDto>> Handle(TriageIncidentCommand request, CancellationToken cancellationToken)
    {
        var aiAnalysis = await _aiAgent.AnalyzeIncidentAsync(request.IncidentDescription, request.ErrorLogs, cancellationToken);

        var triage = new IncidentTriage(
            request.ExecutionId,
            request.IncidentKey,
            aiAnalysis.Severity,
            aiAnalysis.RootCauseCategory,
            aiAnalysis.ConfidenceScore,
            aiAnalysis.Summary,
            aiAnalysis.RecommendedAction
        );

        await _triageRepository.AddAsync(triage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new IncidentTriageDto(
            triage.Id,
            triage.WorkflowExecutionId,
            triage.IncidentKey,
            triage.Severity,
            triage.RootCauseCategory,
            triage.ConfidenceScore,
            triage.AiSummary,
            triage.RecommendedAction,
            triage.IsResolved,
            triage.ResolvedAtUtc,
            triage.CreatedAtUtc
        );

        return Result.Success(dto);
    }
}
