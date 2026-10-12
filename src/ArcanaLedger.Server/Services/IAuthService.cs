using ArcanaLedger.Server.Dtos.Auth;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Authentication and account operations backing <c>/api/v1/auth</c>, mapped to the
/// <c>app_users</c> table.
/// </summary>
public interface IAuthService
{
    RegisteredUserResponse Register(RegisterRequest request, CancellationToken cancellationToken);

    LoginResponse Login(LoginRequest request, CancellationToken cancellationToken);

    void Logout(Guid userId, CancellationToken cancellationToken);

    AuthenticatedUserResponse GetCurrentUser(Guid userId, CancellationToken cancellationToken);
}
