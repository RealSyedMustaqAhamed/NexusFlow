using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Application.DTOs;
using NexusFlow.Application.Workflows.Commands.CreateWorkflow;
using NexusFlow.Application.Workflows.Commands.TriggerWorkflow;
using NexusFlow.Application.Workflows.Queries.GetActiveWorkflows;
using NexusFlow.Application.Workflows.Queries.GetWorkflowById;

namespace NexusFlow.Api.Controllers;

public class WorkflowsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkflowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveWorkflows()
    {
        var result = await Mediator.Send(new GetActiveWorkflowsQuery());
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkflowById(Guid id)
    {
        var result = await Mediator.Send(new GetWorkflowByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(WorkflowDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetWorkflowById), new { id = result.Value.Id }, result.Value);
        }
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/trigger")]
    [ProducesResponseType(typeof(WorkflowExecutionDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerWorkflow(Guid id, [FromBody] TriggerRequest request)
    {
        var command = new TriggerWorkflowCommand(id, request.TriggerSource, request.PayloadJson);
        var result = await Mediator.Send(command);
        if (result.IsSuccess)
        {
            return Accepted($"/api/v1/executions/{result.Value.Id}", result.Value);
        }
        return HandleResult(result);
    }
}

public record TriggerRequest(string TriggerSource, string PayloadJson);
