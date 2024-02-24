using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PixelApi.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var problemDetails = exception switch
        {
            ArgumentException => HandleArgumentException(exception),
            _ => HandleGenericException(exception)
        };

        httpContext.Response.StatusCode =
            problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails HandleGenericException(Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = exception.Message,
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error"
        };

        logger.LogError(
            exception,
            "An error occurred while processing the request. {Exception}",
            exception
        );

        return problemDetails;
    }

    private ProblemDetails HandleArgumentException(Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Detail = exception.Message,
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad request"
        };
        logger.LogWarning(
            exception,
            "An error occurred while processing the request. {Exception}",
            exception
        );

        return problemDetails;
    }
}
