using ArcanaLedger.Server.Dtos.Activity;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IPortfolioActivityService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockPortfolioActivityService(MockDataStore store) : IPortfolioActivityService
{
    public IReadOnlyList<PortfolioActivityResponse> List(Guid userId, Guid portfolioId, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        return store.Activity.Values
            .Where(a => a.PortfolioId == portfolioId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(limit)
            .Select(a => new PortfolioActivityResponse(a.Id, a.Category, a.Title, a.Detail, a.OccurredAt))
            .ToList();
    }
}
