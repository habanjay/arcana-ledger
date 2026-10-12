using ArcanaLedger.Server.Dtos.Settings;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Portfolio configuration operations backing <c>/api/v1/portfolios/{portfolioId}/settings</c>,
/// mapped to the <c>portfolio_settings</c> table (one row per portfolio).
/// </summary>
public interface IPortfolioSettingsService
{
    PortfolioSettingsResponse Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken);

    PortfolioSettingsResponse Upsert(Guid userId, Guid portfolioId, UpsertPortfolioSettingsRequest request, CancellationToken cancellationToken);
}
