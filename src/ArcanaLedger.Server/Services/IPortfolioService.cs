using ArcanaLedger.Server.Dtos.Portfolios;

namespace ArcanaLedger.Server.Services;

/// <summary>Portfolio CRUD operations backing <c>/api/v1/portfolios</c>, mapped to the <c>portfolios</c> table.</summary>
public interface IPortfolioService
{
    IReadOnlyList<PortfolioResponse> List(Guid userId, CancellationToken cancellationToken);

    PortfolioResponse Create(Guid userId, CreatePortfolioRequest request, CancellationToken cancellationToken);

    PortfolioResponse Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken);

    PortfolioResponse Update(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken cancellationToken);

    void Delete(Guid userId, Guid portfolioId, CancellationToken cancellationToken);

    /// <summary>Verifies the portfolio exists and belongs to <paramref name="userId"/>, throwing otherwise.</summary>
    void EnsureOwned(Guid userId, Guid portfolioId, CancellationToken cancellationToken);
}
