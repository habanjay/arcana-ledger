using ArcanaLedger.Server.Dtos.Transactions;

namespace ArcanaLedger.Server.Services;

/// <summary>Filters accepted by <c>GET /api/v1/portfolios/{portfolioId}/transactions</c>.</summary>
public sealed record TransactionListFilter(string? TransactionType, DateTimeOffset? From, DateTimeOffset? To);

/// <summary>
/// Ledger operations backing <c>/api/v1/portfolios/{portfolioId}/transactions</c>, mapped
/// to the <c>portfolio_transactions</c> table.
/// </summary>
public interface IPortfolioTransactionService
{
    IReadOnlyList<PortfolioTransactionResponse> List(Guid userId, Guid portfolioId, TransactionListFilter filter, CancellationToken cancellationToken);

    PortfolioTransactionResponse Create(Guid userId, Guid portfolioId, CreatePortfolioTransactionRequest request, CancellationToken cancellationToken);
}
