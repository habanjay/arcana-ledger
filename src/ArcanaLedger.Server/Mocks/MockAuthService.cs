using ArcanaLedger.Server.Dtos.Auth;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IAuthService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockAuthService(MockDataStore store, IJwtTokenService tokenService) : IAuthService
{
    public RegisteredUserResponse Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            if (store.Users.Values.Any(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw ApiException.Conflict(
                    "email_taken",
                    "Email is already registered.",
                    $"An account with email '{request.Email}' already exists.");
            }

            var now = DateTimeOffset.UtcNow;
            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = PasswordHasher.Hash(request.Password),
                CreatedAt = now,
                UpdatedAt = now,
            };
            store.Users[user.Id] = user;

            return new RegisteredUserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAt);
        }
    }

    public LoginResponse Login(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = store.Users.Values.FirstOrDefault(u => string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase));
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw ApiException.Unauthorized("Email or password is incorrect.");
        }

        var issued = tokenService.IssueToken(user.Id, user.Email);
        return new LoginResponse(issued.AccessToken, issued.ExpiresAt, new LoginUserSummary(user.Id, user.Email, user.DisplayName));
    }

    public void Logout(Guid userId, CancellationToken cancellationToken)
    {
        // The mock store does not track refresh tokens or server-side sessions, so there is
        // nothing to revoke; access tokens simply expire on their own.
    }

    public AuthenticatedUserResponse GetCurrentUser(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!store.Users.TryGetValue(userId, out var user))
        {
            throw ApiException.NotFound("User", userId);
        }

        return new AuthenticatedUserResponse(user.Id, user.Email, user.DisplayName, user.CreatedAt, user.UpdatedAt);
    }
}
