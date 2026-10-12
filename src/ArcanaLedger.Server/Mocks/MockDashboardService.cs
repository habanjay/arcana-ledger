using ArcanaLedger.Server.Dtos.Dashboard;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IDashboardService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockDashboardService(MockDataStore store) : IDashboardService
{
    private const int ActivityLimit = 10;

    public DashboardSummaryResponse GetSummary(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var portfolio = store.GetOwnedPortfolio(userId, portfolioId);

        var latestSnapshot = store.Snapshots.Values
            .Where(s => s.PortfolioId == portfolioId)
            .OrderByDescending(s => s.AsOfDate)
            .Select(s => new DashboardSnapshotSummary(s.AsOfDate, s.TotalValue, s.CashValue, s.InvestedValue, s.DailyReturn))
            .FirstOrDefault();

        var watchlist = store.WatchlistItems.Values
            .Where(w => w.PortfolioId == portfolioId)
            .OrderBy(w => w.DisplayOrder)
            .Select(w =>
            {
                var instrument = store.Instruments[w.InstrumentId];
                store.Quotes.TryGetValue(w.InstrumentId, out var quote);
                return new DashboardWatchlistItem(
                    instrument.Symbol,
                    instrument.Name,
                    instrument.Sector,
                    quote?.LastPrice ?? 0m,
                    quote?.ChangePercent,
                    quote?.Tone ?? "neutral");
            })
            .ToList();

        var activity = store.Activity.Values
            .Where(a => a.PortfolioId == portfolioId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(ActivityLimit)
            .Select(a => new DashboardActivityItem(a.Title, a.Detail, a.OccurredAt))
            .ToList();

        return new DashboardSummaryResponse(
            new DashboardPortfolioSummary(portfolio.Id, portfolio.Name, portfolio.BaseCurrency),
            latestSnapshot,
            watchlist,
            activity);
    }
}
