using ArcanaLedger.Server.Dtos.Accounts;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Brokerage account operations backing <c>/api/v1/portfolios/{portfolioId}/accounts</c>,
/// mapped to the <c>brokerage_accounts</c> table.
/// </summary>
public interface IBrokerageAccountService
{
    Task<IReadOnlyList<BrokerageAccountResponse>> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken);

    Task<BrokerageAccountResponse> Create(Guid userId, Guid portfolioId, CreateAccountRequest request, CancellationToken cancellationToken);

    Task<BrokerageAccountResponse> Update(Guid userId, Guid portfolioId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken);

    Task Delete(Guid userId, Guid portfolioId, Guid accountId, CancellationToken cancellationToken);
}
