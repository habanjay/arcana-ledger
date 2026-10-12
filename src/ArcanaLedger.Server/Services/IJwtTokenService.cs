namespace ArcanaLedger.Server.Services;

/// <summary>Result of issuing a signed access token for an authenticated user.</summary>
public readonly record struct IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Issues signed bearer access tokens for authenticated users. Kept independent of the
/// mock user store so a real credential/identity provider can replace it later.
/// </summary>
public interface IJwtTokenService
{
    IssuedToken IssueToken(Guid userId, string email);
}
