using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Snapshots;

/// <summary>
/// A daily portfolio valuation snapshot, mapped to <c>portfolio_snapshots</c>. Always read
/// from this cache; never recomputed from raw transactions per request.
/// </summary>
public sealed record PortfolioSnapshotResponse(
    DateOnly AsOfDate,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal TotalValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal CashValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal InvestedValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal DailyReturn,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal TimeWeightedReturn);
