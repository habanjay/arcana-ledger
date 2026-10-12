using ArcanaLedger.Server.Dtos.Transactions;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IPortfolioTransactionService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockPortfolioTransactionService(MockDataStore store) : IPortfolioTransactionService
{
    public IReadOnlyList<PortfolioTransactionResponse> List(Guid userId, Guid portfolioId, TransactionListFilter filter, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        var query = store.Transactions.Values.Where(t => t.PortfolioId == portfolioId);

        if (!string.IsNullOrWhiteSpace(filter.TransactionType))
        {
            query = query.Where(t => string.Equals(t.TransactionType, filter.TransactionType, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.From is { } from)
        {
            query = query.Where(t => t.OccurredAt >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(t => t.OccurredAt <= to);
        }

        return query.OrderByDescending(t => t.OccurredAt).Select(ToResponse).ToList();
    }

    public PortfolioTransactionResponse Create(Guid userId, Guid portfolioId, CreatePortfolioTransactionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, portfolioId);

            if (request.AccountId is { } accountId
                && (!store.Accounts.TryGetValue(accountId, out var account) || account.PortfolioId != portfolioId))
            {
                throw ApiException.NotFound("Account", accountId);
            }

            var transaction = new TransactionEntity
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                AccountId = request.AccountId,
                TransactionType = request.TransactionType,
                CashAmount = request.CashAmount,
                Currency = request.Currency,
                OccurredAt = request.OccurredAt,
                Memo = request.Memo,
            };
            store.Transactions[transaction.Id] = transaction;
            return ToResponse(transaction);
        }
    }

    private static PortfolioTransactionResponse ToResponse(TransactionEntity transaction)
        => new(
            transaction.Id,
            transaction.PortfolioId,
            transaction.AccountId,
            transaction.InstrumentId,
            transaction.TradeId,
            transaction.TransactionType,
            transaction.CashAmount,
            transaction.Currency,
            transaction.Quantity,
            transaction.UnitPrice,
            transaction.OccurredAt,
            transaction.Memo);
}
