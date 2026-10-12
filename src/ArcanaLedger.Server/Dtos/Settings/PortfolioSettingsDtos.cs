using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Settings;

/// <summary>
/// Request body for <c>PUT /api/v1/portfolios/{portfolioId}/settings</c>. Replaces the
/// portfolio's entire configuration (upsert).
/// </summary>
public sealed record UpsertPortfolioSettingsRequest
{
    [Required]
    [Range(0, double.MaxValue)]
    public required decimal Capital { get; init; }

    [Required]
    [Range(0, 1)]
    public required double VarRate { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal CashValue { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal ReserveValue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? PositionEntryPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? PositionExitPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? PositionShares { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? PositionCashValue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TrancheEntryPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TrancheExitPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TrancheShares { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TrancheCashValue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? SwingEntryPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? SwingExitPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? SwingShares { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? SwingCashValue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? BreakoutPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? PatternLowPrice { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? SwingTargetPrice { get; init; }

    [Range(0, 1)]
    public double? MomentumRate { get; init; }
}

/// <summary>
/// Response body representing a portfolio's configuration, mapped to <c>portfolio_settings</c>.
/// Echoes the request plus the generated allocation-percent columns.
/// </summary>
public sealed record PortfolioSettingsResponse(
    Guid PortfolioId,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Capital,
    double VarRate,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal CashValue,
    double CashAllocationPercent,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal ReserveValue,
    double ReserveAllocationPercent,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PositionEntryPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PositionExitPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PositionShares,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PositionCashValue,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TrancheEntryPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TrancheExitPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TrancheShares,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TrancheCashValue,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? SwingEntryPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? SwingExitPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? SwingShares,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? SwingCashValue,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? BreakoutPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PatternLowPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? SwingTargetPrice,
    double? MomentumRate,
    DateTimeOffset UpdatedAt);
