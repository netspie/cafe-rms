using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            DomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        var detail = exception switch
        {
            DomainException => exception.Message,
            UnauthorizedAccessException => exception.Message,
            DbUpdateConcurrencyException => "The record was modified by another user. Reload and try again.",
            _ => null
        };

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        }, ct);

        return true;
    }
}
