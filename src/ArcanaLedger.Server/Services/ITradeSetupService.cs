using ArcanaLedger.Server.Dtos.TradeSetups;

namespace ArcanaLedger.Server.Services;

/// <summary>Trade setup CRUD operations backing <c>/api/v1/trade-setups</c>, mapped to the <c>trade_setups</c> table.</summary>
public interface ITradeSetupService
{
    Task<IReadOnlyList<TradeSetupResponse>> List(Guid userId, CancellationToken cancellationToken);

    Task<TradeSetupResponse> Create(Guid userId, CreateTradeSetupRequest request, CancellationToken cancellationToken);

    Task<TradeSetupResponse> Get(Guid userId, Guid setupId, CancellationToken cancellationToken);

    Task<TradeSetupResponse> Update(Guid userId, Guid setupId, UpdateTradeSetupRequest request, CancellationToken cancellationToken);

    Task Delete(Guid userId, Guid setupId, CancellationToken cancellationToken);
}
