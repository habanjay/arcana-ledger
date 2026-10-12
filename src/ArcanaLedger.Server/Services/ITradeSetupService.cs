using ArcanaLedger.Server.Dtos.TradeSetups;

namespace ArcanaLedger.Server.Services;

/// <summary>Trade setup CRUD operations backing <c>/api/v1/trade-setups</c>, mapped to the <c>trade_setups</c> table.</summary>
public interface ITradeSetupService
{
    IReadOnlyList<TradeSetupResponse> List(Guid userId, CancellationToken cancellationToken);

    TradeSetupResponse Create(Guid userId, CreateTradeSetupRequest request, CancellationToken cancellationToken);

    TradeSetupResponse Get(Guid userId, Guid setupId, CancellationToken cancellationToken);

    TradeSetupResponse Update(Guid userId, Guid setupId, UpdateTradeSetupRequest request, CancellationToken cancellationToken);

    void Delete(Guid userId, Guid setupId, CancellationToken cancellationToken);
}
