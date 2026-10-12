using ArcanaLedger.Server.Dtos.Auth;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Authentication and account operations backing <c>/api/v1/auth</c>, mapped to the
/// <c>app_users</c> table.
/// </summary>
public interface IAuthService
{
    Task<RegisteredUserResponse> Register(RegisterRequest request, CancellationToken cancellationToken);

    Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken);

    Task Logout(Guid userId, CancellationToken cancellationToken);

    Task<AuthenticatedUserResponse> GetCurrentUser(Guid userId, CancellationToken cancellationToken);
}
