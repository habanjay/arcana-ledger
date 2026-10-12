using ArcanaLedger.Server.Dtos.Snapshots;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Read-only snapshot lookups backing <c>/api/v1/portfolios/{portfolioId}/snapshots</c>,
/// mapped to the <c>portfolio_snapshots</c> table.
/// </summary>
public interface IPortfolioSnapshotService
{
    Task<IReadOnlyList<PortfolioSnapshotResponse>> List(Guid userId, Guid portfolioId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}
