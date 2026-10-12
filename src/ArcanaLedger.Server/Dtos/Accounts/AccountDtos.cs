using System.ComponentModel.DataAnnotations;

namespace ArcanaLedger.Server.Dtos.Accounts;

/// <summary>Request body for <c>POST /api/v1/portfolios/{portfolioId}/accounts</c>.</summary>
public sealed record CreateAccountRequest
{
    [Required]
    [MaxLength(128)]
    public required string Name { get; init; }

    [Required]
    [MaxLength(128)]
    public required string BrokerName { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO 4217 code.")]
    public required string Currency { get; init; }
}

/// <summary>
/// Request body for <c>PATCH /api/v1/portfolios/{portfolioId}/accounts/{accountId}</c>.
/// All fields are optional.
/// </summary>
public sealed record UpdateAccountRequest
{
    [MaxLength(128)]
    public string? Name { get; init; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO 4217 code.")]
    public string? Currency { get; init; }

    public bool? IsActive { get; init; }
}

/// <summary>Response body representing a brokerage account, mapped to <c>brokerage_accounts</c>.</summary>
public sealed record BrokerageAccountResponse(
    Guid Id,
    Guid PortfolioId,
    string Name,
    string BrokerName,
    string Currency,
    bool IsActive,
    DateTimeOffset CreatedAt);
