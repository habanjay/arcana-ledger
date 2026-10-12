using ArcanaLedger.Server.Dtos.Common;
using ArcanaLedger.Server.Services;
using Microsoft.AspNetCore.Diagnostics;

namespace ArcanaLedger.Server.Middleware;

/// <summary>
/// Centralized exception handler that maps <see cref="ApiException"/> instances (and any
/// other unhandled exception) to the <see cref="ApiErrorResponse"/> envelope. Status-code
/// pages (e.g. 404 for unmatched routes, 401/403 from authentication/authorization
/// middleware) reuse the same envelope via <c>UseStatusCodePages</c> in <c>Program.cs</c>.
/// </summary>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, response) = exception switch
        {
            ApiException apiException => (
                (int)apiException.StatusCode,
                new ApiErrorResponse(
                    Type: $"https://arcanaledger.dev/errors/{apiException.Code}",
                    Title: apiException.Title,
                    Status: (int)apiException.StatusCode,
                    Detail: apiException.Detail,
                    Code: apiException.Code,
                    Errors: apiException.Errors)),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    Type: "https://arcanaledger.dev/errors/internal",
                    Title: "An unexpected error occurred.",
                    Status: StatusCodes.Status500InternalServerError,
                    Code: "internal_error"))
        };

        if (response.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "API exception {Code} while processing {Method} {Path}",
                response.Code, httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
