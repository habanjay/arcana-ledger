using ArcanaLedger.Server.Dtos.Activity;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Read-only recent-activity lookups backing <c>/api/v1/portfolios/{portfolioId}/activity</c>,
/// mapped to the <c>portfolio_activity</c> table.
/// </summary>
public interface IPortfolioActivityService
{
    IReadOnlyList<PortfolioActivityResponse> List(Guid userId, Guid portfolioId, int limit, CancellationToken cancellationToken);
}
