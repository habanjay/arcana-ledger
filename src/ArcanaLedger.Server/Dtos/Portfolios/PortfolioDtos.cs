using System.ComponentModel.DataAnnotations;

namespace ArcanaLedger.Server.Dtos.Portfolios;

/// <summary>Request body for <c>POST /api/v1/portfolios</c>.</summary>
public sealed record CreatePortfolioRequest
{
    [Required]
    [MaxLength(128)]
    public required string Name { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Base currency must be a 3-letter ISO 4217 code.")]
    public required string BaseCurrency { get; init; }

    public bool IsDefault { get; init; }
}

/// <summary>Request body for <c>PATCH /api/v1/portfolios/{portfolioId}</c>. All fields are optional.</summary>
public sealed record UpdatePortfolioRequest
{
    [MaxLength(128)]
    public string? Name { get; init; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Base currency must be a 3-letter ISO 4217 code.")]
    public string? BaseCurrency { get; init; }

    public bool? IsDefault { get; init; }
}

/// <summary>Response body representing a portfolio, mapped to the <c>portfolios</c> table.</summary>
public sealed record PortfolioResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string BaseCurrency,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
