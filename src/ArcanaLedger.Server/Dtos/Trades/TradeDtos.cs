using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Trades;

/// <summary>Request body for <c>POST /api/v1/trades</c>.</summary>
public sealed record CreateTradeRequest
{
    [Required]
    public required Guid AccountId { get; init; }

    [Required]
    public required Guid InstrumentId { get; init; }

    public Guid? SetupId { get; init; }

    [Required]
    [AllowedValues("long", "short", ErrorMessage = "Order type must be one of: long, short.")]
    public required string OrderType { get; init; }

    [Required]
    public required DateOnly OpenedOn { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal EntryTarget { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? CutLoss { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TargetPrice { get; init; }

    public string? Notes { get; init; }
}

/// <summary>Request body for <c>PATCH /api/v1/trades/{tradeId}</c>. All fields are optional.</summary>
public sealed record UpdateTradeRequest
{
    [Range(0, double.MaxValue)]
    public decimal? EntryTarget { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? CutLoss { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? TargetPrice { get; init; }

    [AllowedValues("planned", "open", "closed", ErrorMessage = "Status must be one of: planned, open, closed.")]
    public string? Status { get; init; }

    public DateOnly? ClosedOn { get; init; }

    public string? Notes { get; init; }
}

/// <summary>The instrument summary nested inside <see cref="TradeResponse"/>.</summary>
public sealed record TradeInstrumentSummary(Guid Id, string Symbol);

/// <summary>The setup summary nested inside <see cref="TradeResponse"/>.</summary>
public sealed record TradeSetupSummary(Guid Id, string Code, string Name);

/// <summary>Response body representing a trade (logical position), mapped to the <c>trades</c> table.</summary>
public sealed record TradeResponse(
    Guid Id,
    Guid PortfolioId,
    Guid AccountId,
    TradeInstrumentSummary Instrument,
    TradeSetupSummary? Setup,
    string OrderType,
    string Status,
    DateOnly OpenedOn,
    DateOnly? ClosedOn,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal EntryTarget,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? CutLoss,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TargetPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? UnrealizedPnlAmount,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? UnrealizedPnlPercent,
    string PositionResult,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<TradeExecutionResponse>? Executions = null);

/// <summary>
/// The list-item shape returned by <c>GET /api/v1/trades</c>, matching the Trades page table.
/// </summary>
public sealed record TradeListItemResponse(
    Guid Id,
    string Symbol,
    string? Setup,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Shares,
    DateOnly Date,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal EntryPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? CutLoss,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? TargetPrice,
    string Result,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? Quote,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? Change);

/// <summary>Request body for <c>POST /api/v1/trades/{tradeId}/executions</c>.</summary>
public sealed record CreateTradeExecutionRequest
{
    [Required]
    [AllowedValues("buy", "sell", ErrorMessage = "Side must be one of: buy, sell.")]
    public required string Side { get; init; }

    [Required]
    [Range(0.00000001, double.MaxValue)]
    public required decimal Quantity { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal Price { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Commission { get; init; }

    [Required]
    public required DateTimeOffset ExecutedAt { get; init; }
}

/// <summary>Response body representing a fill/tranche, mapped to <c>trade_executions</c>.</summary>
public sealed record TradeExecutionResponse(
    Guid Id,
    Guid TradeId,
    string Side,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Quantity,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Price,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Commission,
    DateTimeOffset ExecutedAt);
