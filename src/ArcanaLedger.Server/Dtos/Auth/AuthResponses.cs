namespace ArcanaLedger.Server.Dtos.Auth;

/// <summary>Response body for <c>POST /api/v1/auth/register</c>.</summary>
public sealed record RegisteredUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAt);

/// <summary>Response body for <c>GET /api/v1/auth/me</c>.</summary>
public sealed record AuthenticatedUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>The user summary nested inside <see cref="LoginResponse"/>.</summary>
public sealed record LoginUserSummary(
    Guid Id,
    string Email,
    string DisplayName);

/// <summary>Response body for <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    LoginUserSummary User);
