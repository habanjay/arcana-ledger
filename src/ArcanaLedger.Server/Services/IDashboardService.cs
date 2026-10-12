using ArcanaLedger.Server.Dtos.Dashboard;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Aggregates the latest snapshot, watchlist, and activity for the Dashboard page, backing
/// <c>GET /api/v1/portfolios/{portfolioId}/dashboard</c>.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummary(Guid userId, Guid portfolioId, CancellationToken cancellationToken);
}
