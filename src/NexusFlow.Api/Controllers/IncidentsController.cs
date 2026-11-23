using Microsoft.AspNetCore.Mvc;
using NexusFlow.Application.DTOs;
using NexusFlow.Application.Incidents.Commands.TriageIncident;
using NexusFlow.Application.Incidents.Queries.GetRecentTriages;

namespace NexusFlow.Api.Controllers;

public class IncidentsController : ApiControllerBase
{
    [HttpPost("triage")]
    [ProducesResponseType(typeof(IncidentTriageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriageIncident([FromBody] TriageIncidentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("recent")]
    [ProducesResponseType(typeof(IReadOnlyList<IncidentTriageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentTriages([FromQuery] int limit = 20)
    {
        var result = await Mediator.Send(new GetRecentTriagesQuery(limit));
        return HandleResult(result);
    }
}
