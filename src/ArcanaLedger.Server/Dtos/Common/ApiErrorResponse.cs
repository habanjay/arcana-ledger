namespace ArcanaLedger.Server.Dtos.Common;

/// <summary>
/// RFC 9457 Problem Details envelope used for every API error response, including
/// <see cref="Services.ApiException"/> failures and otherwise-unhandled status-code pages.
/// </summary>
/// <param name="Type">A URI identifying the error category.</param>
/// <param name="Title">A short, human-readable summary of the problem.</param>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Detail">An optional, human-readable explanation specific to this occurrence.</param>
/// <param name="Code">An optional machine-readable error code for programmatic handling.</param>
/// <param name="Errors">Optional per-field validation error messages.</param>
public sealed record ApiErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail = null,
    string? Code = null,
    IDictionary<string, string[]>? Errors = null);
