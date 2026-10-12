using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Auth;
using ArcanaLedger.Server.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/auth</c>, the authentication and current-user endpoints.</summary>
public static class AuthApi
{
    /// <summary>Maps the public registration/login endpoints into an anonymous route group.</summary>
    public static void MapAnonymous(RouteGroupBuilder group)
    {
        group.MapPost("/register", Register)
            .RequireRateLimiting("sensitive")
            .WithName("RegisterUser")
            .WithSummary("Create an account.")
            .WithDescription("Creates a new app_users row from an email, display name, and password.")
            .Produces<RegisteredUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .RequireRateLimiting("sensitive")
            .WithName("Login")
            .WithSummary("Exchange credentials for a JWT.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    /// <summary>Maps the authenticated account endpoints into an authorization-required route group.</summary>
    public static void MapAuthorized(RouteGroupBuilder group)
    {
        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("End the client session; the bearer token remains valid until it expires.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/me", GetMe)
            .WithName("GetCurrentUser")
            .WithSummary("Return the authenticated user.")
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Register(RegisterRequest request, IAuthService authService, CancellationToken cancellationToken)
    {
        var user = await authService.Register(request, cancellationToken);
        return Results.Created("/api/v1/auth/me", user);
    }

    private static async Task<IResult> Login(LoginRequest request, IAuthService authService, CancellationToken cancellationToken)
        => Results.Ok(await authService.Login(request, cancellationToken));

    private static async Task<IResult> Logout(ClaimsPrincipal user, IAuthService authService, CancellationToken cancellationToken)
    {
        await authService.Logout(user.GetUserId(), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetMe(ClaimsPrincipal user, IAuthService authService, CancellationToken cancellationToken)
        => Results.Ok(await authService.GetCurrentUser(user.GetUserId(), cancellationToken));
}
