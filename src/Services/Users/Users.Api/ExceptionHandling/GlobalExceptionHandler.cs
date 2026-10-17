using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Users.Application.Common;
using Users.Domain.Common;

namespace Users.Api.ExceptionHandling;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception);

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails CreateProblemDetails(Exception exception) => exception switch
    {
        ValidationException validation => new HttpValidationProblemDetails(
            new Dictionary<string, string[]>(validation.Errors))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid request"
        },
        DomainException => Problem(StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
        InvalidCredentialsException => Problem(StatusCodes.Status401Unauthorized, "Authentication failed", exception.Message),
        NotFoundException => Problem(StatusCodes.Status404NotFound, "Resource not found", exception.Message),
        ConflictException => Problem(StatusCodes.Status409Conflict, "Conflict", exception.Message),
        _ => Problem(StatusCodes.Status500InternalServerError, "An unexpected error occurred", detail: null)
    };

    private static ProblemDetails Problem(int status, string title, string? detail) =>
        new() { Status = status, Title = title, Detail = detail };
}