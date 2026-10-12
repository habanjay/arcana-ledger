using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Dashboard;

/// <summary>The portfolio summary nested inside <see cref="DashboardSummaryResponse"/>.</summary>
public sealed record DashboardPortfolioSummary(Guid Id, string Name, string BaseCurrency);

/// <summary>The latest-snapshot summary nested inside <see cref="DashboardSummaryResponse"/>.</summary>
public sealed record DashboardSnapshotSummary(
    DateOnly AsOfDate,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal TotalValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal CashValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal InvestedValue,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal DailyReturn);

/// <summary>A watchlist entry as shown on the dashboard widget.</summary>
public sealed record DashboardWatchlistItem(
    string Symbol,
    string Name,
    string? Sector,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Price,
    [property: JsonConverter(typeof(NullableMoneyJsonConverter))] decimal? Change,
    string Tone);

/// <summary>A recent activity entry as shown on the dashboard widget.</summary>
public sealed record DashboardActivityItem(string Title, string Detail, DateTimeOffset Time);

/// <summary>
/// Convenience aggregation of the latest snapshot, watchlist, and activity for the
/// Dashboard page, backing <c>GET /api/v1/portfolios/{portfolioId}/dashboard</c>.
/// </summary>
public sealed record DashboardSummaryResponse(
    DashboardPortfolioSummary Portfolio,
    DashboardSnapshotSummary? LatestSnapshot,
    IReadOnlyList<DashboardWatchlistItem> Watchlist,
    IReadOnlyList<DashboardActivityItem> Activity);
