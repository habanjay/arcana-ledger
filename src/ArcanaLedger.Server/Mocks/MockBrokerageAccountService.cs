using ArcanaLedger.Server.Dtos.Accounts;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IBrokerageAccountService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockBrokerageAccountService(MockDataStore store) : IBrokerageAccountService
{
    public IReadOnlyList<BrokerageAccountResponse> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        return store.Accounts.Values
            .Where(a => a.PortfolioId == portfolioId)
            .OrderBy(a => a.CreatedAt)
            .Select(ToResponse)
            .ToList();
    }

    public BrokerageAccountResponse Create(Guid userId, Guid portfolioId, CreateAccountRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, portfolioId);

            var account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                Name = request.Name,
                BrokerName = request.BrokerName,
                Currency = request.Currency,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            store.Accounts[account.Id] = account;
            return ToResponse(account);
        }
    }

    public BrokerageAccountResponse Update(Guid userId, Guid portfolioId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var account = GetOwnedAccount(userId, portfolioId, accountId);

            if (request.Name is not null)
            {
                account.Name = request.Name;
            }

            if (request.Currency is not null)
            {
                account.Currency = request.Currency;
            }

            if (request.IsActive is not null)
            {
                account.IsActive = request.IsActive.Value;
            }

            return ToResponse(account);
        }
    }

    public void Delete(Guid userId, Guid portfolioId, Guid accountId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            GetOwnedAccount(userId, portfolioId, accountId);
            store.Accounts.TryRemove(accountId, out _);
        }
    }

    private AccountEntity GetOwnedAccount(Guid userId, Guid portfolioId, Guid accountId)
    {
        store.GetOwnedPortfolio(userId, portfolioId);

        if (!store.Accounts.TryGetValue(accountId, out var account) || account.PortfolioId != portfolioId)
        {
            throw ApiException.NotFound("Account", accountId);
        }

        return account;
    }

    private static BrokerageAccountResponse ToResponse(AccountEntity account)
        => new(account.Id, account.PortfolioId, account.Name, account.BrokerName, account.Currency, account.IsActive, account.CreatedAt);
}
