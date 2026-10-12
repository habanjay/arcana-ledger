using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Instruments;

/// <summary>The latest quote for an instrument, mapped to <c>instrument_quotes</c>.</summary>
public sealed record QuoteResponse(
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal LastPrice,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? PreviousClose,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? ChangeAmount,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? ChangePercent,
    string Tone,
    DateTimeOffset QuotedAt);

/// <summary>
/// Response body representing a tradable instrument and its latest quote, mapped to
/// <c>instruments</c>. Read-only; instruments and quotes are maintained by a market-data
/// sync job.
/// </summary>
public sealed record InstrumentResponse(
    Guid Id,
    string Symbol,
    string Name,
    string? Sector,
    string AssetType,
    string Currency,
    bool IsActive,
    QuoteResponse? Quote);
