using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Watchlist;

/// <summary>Request body for <c>POST /api/v1/portfolios/{portfolioId}/watchlist</c>.</summary>
public sealed record AddWatchlistItemRequest
{
    [Required]
    public required Guid InstrumentId { get; init; }

    public int DisplayOrder { get; init; }
}

/// <summary>
/// Request body for <c>PATCH /api/v1/portfolios/{portfolioId}/watchlist/{instrumentId}</c>.
/// </summary>
public sealed record UpdateWatchlistItemRequest
{
    [Required]
    public required int DisplayOrder { get; init; }
}

/// <summary>The instrument summary nested inside <see cref="WatchlistItemResponse"/>.</summary>
public sealed record WatchlistInstrumentSummary(
    Guid Id,
    string Symbol,
    string Name,
    string? Sector);

/// <summary>The quote summary nested inside <see cref="WatchlistItemResponse"/>.</summary>
public sealed record WatchlistQuoteSummary(
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal LastPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? ChangeAmount,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? ChangePercent,
    string Tone);

/// <summary>Response body representing a watchlist entry, mapped to <c>watchlist_items</c>.</summary>
public sealed record WatchlistItemResponse(
    Guid PortfolioId,
    WatchlistInstrumentSummary Instrument,
    WatchlistQuoteSummary? Quote,
    int DisplayOrder,
    DateTimeOffset AddedAt);
