using System.ComponentModel.DataAnnotations;

namespace ArcanaLedger.Server.Dtos.Auth;

/// <summary>Request body for <c>POST /api/v1/auth/login</c>.</summary>
public sealed record LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    [MaxLength(256)]
    public required string Password { get; init; }
}
