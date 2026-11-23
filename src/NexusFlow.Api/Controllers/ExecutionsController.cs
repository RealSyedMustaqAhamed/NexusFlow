using Microsoft.AspNetCore.Mvc;
using NexusFlow.Application.DTOs;
using NexusFlow.Application.Workflows.Queries.GetExecutionStatus;

namespace NexusFlow.Api.Controllers;

public class ExecutionsController : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExecutionStatus(Guid id)
    {
        var result = await Mediator.Send(new GetExecutionStatusQuery(id));
        return HandleResult(result);
    }
}
