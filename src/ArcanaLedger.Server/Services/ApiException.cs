using System.Net;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// An expected API failure that carries a specific HTTP status code, a machine-readable
/// error code, and an optional set of per-field validation errors. Caught centrally by
/// <see cref="Middleware.ApiExceptionHandler"/> and mapped to the <c>ApiErrorResponse</c>
/// envelope documented in <c>docs/api.md</c>.
/// </summary>
public sealed class ApiException : Exception
{
    public ApiException(
        HttpStatusCode statusCode,
        string code,
        string title,
        string? detail = null,
        IDictionary<string, string[]>? errors = null)
        : base(detail ?? title)
    {
        StatusCode = statusCode;
        Code = code;
        Title = title;
        Detail = detail;
        Errors = errors;
    }

    public HttpStatusCode StatusCode { get; }

    public string Code { get; }

    public string Title { get; }

    public string? Detail { get; }

    public IDictionary<string, string[]>? Errors { get; }

    public static ApiException NotFound(string resource, object id)
        => new(HttpStatusCode.NotFound, "not_found", $"{resource} not found.", $"No {resource} exists with id '{id}'.");

    public static ApiException Forbidden(string detail = "You do not have access to this resource.")
        => new(HttpStatusCode.Forbidden, "forbidden", "Access to the resource is forbidden.", detail);

    public static ApiException Conflict(string code, string title, string? detail = null)
        => new(HttpStatusCode.Conflict, code, title, detail);

    public static ApiException Unauthorized(string detail = "Invalid credentials.")
        => new(HttpStatusCode.Unauthorized, "unauthorized", "Authentication failed.", detail);

    public static ApiException Validation(IDictionary<string, string[]> errors)
        => new(HttpStatusCode.BadRequest, "validation_error", "One or more validation errors occurred.", errors: errors);
}
