using MediatR;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Domain.Common;

namespace NexusFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(new ProblemDetails { Title = "Resource Not Found", Detail = result.Error, Status = StatusCodes.Status404NotFound });

        return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = result.Error, Status = StatusCodes.Status400BadRequest });
    }
}
