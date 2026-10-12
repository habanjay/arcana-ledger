using System.ComponentModel.DataAnnotations;

namespace ArcanaLedger.Server.Dtos.Auth;

/// <summary>Request body for <c>POST /api/v1/auth/register</c>.</summary>
public sealed record RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MaxLength(128)]
    public required string DisplayName { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(256)]
    public required string Password { get; init; }
}
