using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Shared helper for resolving the authenticated user id from the bearer token.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the authenticated user's id from the <c>sub</c> claim. Endpoints that call this
    /// are expected to already require authorization, so a missing/invalid claim indicates a
    /// misconfigured token rather than an anonymous caller.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subject is null || !Guid.TryParse(subject, out var userId))
        {
            throw ApiException.Unauthorized("The access token does not contain a valid subject claim.");
        }

        return userId;
    }
}
