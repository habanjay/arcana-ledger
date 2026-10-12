using ArcanaLedger.Server.Dtos.Portfolios;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IPortfolioService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockPortfolioService(MockDataStore store) : IPortfolioService
{
    public IReadOnlyList<PortfolioResponse> List(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return store.Portfolios.Values
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.CreatedAt)
            .Select(ToResponse)
            .ToList();
    }

    public PortfolioResponse Create(Guid userId, CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var now = DateTimeOffset.UtcNow;

            if (request.IsDefault)
            {
                foreach (var existing in store.Portfolios.Values.Where(p => p.UserId == userId && p.IsDefault))
                {
                    existing.IsDefault = false;
                    existing.UpdatedAt = now;
                }
            }

            var portfolio = new PortfolioEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                BaseCurrency = request.BaseCurrency,
                IsDefault = request.IsDefault,
                CreatedAt = now,
                UpdatedAt = now,
            };
            store.Portfolios[portfolio.Id] = portfolio;
            return ToResponse(portfolio);
        }
    }

    public PortfolioResponse Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ToResponse(store.GetOwnedPortfolio(userId, portfolioId));
    }

    public PortfolioResponse Update(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var portfolio = store.GetOwnedPortfolio(userId, portfolioId);
            var now = DateTimeOffset.UtcNow;

            if (request.Name is not null)
            {
                portfolio.Name = request.Name;
            }

            if (request.BaseCurrency is not null)
            {
                portfolio.BaseCurrency = request.BaseCurrency;
            }

            if (request.IsDefault is true)
            {
                foreach (var other in store.Portfolios.Values.Where(p => p.UserId == userId && p.Id != portfolioId && p.IsDefault))
                {
                    other.IsDefault = false;
                    other.UpdatedAt = now;
                }

                portfolio.IsDefault = true;
            }
            else if (request.IsDefault is false)
            {
                portfolio.IsDefault = false;
            }

            portfolio.UpdatedAt = now;
            return ToResponse(portfolio);
        }
    }

    public void Delete(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, portfolioId);
            store.Portfolios.TryRemove(portfolioId, out _);

            foreach (var account in store.Accounts.Values.Where(a => a.PortfolioId == portfolioId).ToList())
            {
                store.Accounts.TryRemove(account.Id, out _);
            }

            foreach (var key in store.WatchlistItems.Keys.Where(k => k.PortfolioId == portfolioId).ToList())
            {
                store.WatchlistItems.TryRemove(key, out _);
            }

            foreach (var trade in store.Trades.Values.Where(t => t.PortfolioId == portfolioId).ToList())
            {
                foreach (var execution in store.TradeExecutions.Values.Where(e => e.TradeId == trade.Id).ToList())
                {
                    store.TradeExecutions.TryRemove(execution.Id, out _);
                }

                store.Trades.TryRemove(trade.Id, out _);
            }

            foreach (var transaction in store.Transactions.Values.Where(t => t.PortfolioId == portfolioId).ToList())
            {
                store.Transactions.TryRemove(transaction.Id, out _);
            }

            foreach (var key in store.Snapshots.Where(kvp => kvp.Value.PortfolioId == portfolioId).Select(kvp => kvp.Key).ToList())
            {
                store.Snapshots.TryRemove(key, out _);
            }

            foreach (var activity in store.Activity.Values.Where(a => a.PortfolioId == portfolioId).ToList())
            {
                store.Activity.TryRemove(activity.Id, out _);
            }

            store.Settings.TryRemove(portfolioId, out _);
        }
    }

    public void EnsureOwned(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);
    }

    private static PortfolioResponse ToResponse(PortfolioEntity portfolio)
        => new(portfolio.Id, portfolio.UserId, portfolio.Name, portfolio.BaseCurrency, portfolio.IsDefault, portfolio.CreatedAt, portfolio.UpdatedAt);
}
