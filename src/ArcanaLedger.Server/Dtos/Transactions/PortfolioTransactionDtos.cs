using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Transactions;

/// <summary>Request body for <c>POST /api/v1/portfolios/{portfolioId}/transactions</c>.</summary>
public sealed record CreatePortfolioTransactionRequest
{
    public Guid? AccountId { get; init; }

    [Required]
    [AllowedValues("deposit", "withdrawal", "fee", "adjustment", ErrorMessage =
        "Transaction type must be one of: deposit, withdrawal, fee, adjustment.")]
    public required string TransactionType { get; init; }

    [Required]
    public required decimal CashAmount { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO 4217 code.")]
    public required string Currency { get; init; }

    [Required]
    public required DateTimeOffset OccurredAt { get; init; }

    public string? Memo { get; init; }
}

/// <summary>Response body representing a ledger entry, mapped to <c>portfolio_transactions</c>.</summary>
public sealed record PortfolioTransactionResponse(
    Guid Id,
    Guid PortfolioId,
    Guid? AccountId,
    Guid? InstrumentId,
    Guid? TradeId,
    string TransactionType,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal CashAmount,
    string Currency,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? Quantity,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? UnitPrice,
    DateTimeOffset OccurredAt,
    string? Memo);
