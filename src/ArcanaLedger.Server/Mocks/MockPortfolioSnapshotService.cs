using ArcanaLedger.Server.Dtos.Snapshots;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IPortfolioSnapshotService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockPortfolioSnapshotService(MockDataStore store) : IPortfolioSnapshotService
{
    public IReadOnlyList<PortfolioSnapshotResponse> List(Guid userId, Guid portfolioId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        var query = store.Snapshots.Values.Where(s => s.PortfolioId == portfolioId);

        if (from is { } fromDate)
        {
            query = query.Where(s => s.AsOfDate >= fromDate);
        }

        if (to is { } toDate)
        {
            query = query.Where(s => s.AsOfDate <= toDate);
        }

        return query
            .OrderBy(s => s.AsOfDate)
            .Select(s => new PortfolioSnapshotResponse(s.AsOfDate, s.TotalValue, s.CashValue, s.InvestedValue, s.DailyReturn, s.TimeWeightedReturn))
            .ToList();
    }
}
