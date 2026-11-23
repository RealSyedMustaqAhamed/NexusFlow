using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Domain.Exceptions;

namespace NexusFlow.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception encountered: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Domain Business Rule Violation", ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failure encountered.");
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Validation Failure", errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server exception.");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred while processing the request.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(json);
    }
}
