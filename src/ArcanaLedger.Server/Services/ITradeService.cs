using ArcanaLedger.Server.Dtos.Trades;

namespace ArcanaLedger.Server.Services;

/// <summary>Filters accepted by <c>GET /api/v1/trades</c>.</summary>
public sealed record TradeListFilter(Guid? PortfolioId, string? Status, string? Symbol);

/// <summary>
/// Trade and execution operations backing <c>/api/v1/trades</c>, mapped to the <c>trades</c>
/// and <c>trade_executions</c> tables.
/// </summary>
public interface ITradeService
{
    Task<IReadOnlyList<TradeListItemResponse>> List(Guid userId, TradeListFilter filter, CancellationToken cancellationToken);

    Task<TradeResponse> Create(Guid userId, CreateTradeRequest request, CancellationToken cancellationToken);

    Task<TradeResponse> Get(Guid userId, Guid tradeId, CancellationToken cancellationToken);

    Task<TradeResponse> Update(Guid userId, Guid tradeId, UpdateTradeRequest request, CancellationToken cancellationToken);

    Task Delete(Guid userId, Guid tradeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TradeExecutionResponse>> ListExecutions(Guid userId, Guid tradeId, CancellationToken cancellationToken);

    Task<TradeExecutionResponse> AddExecution(Guid userId, Guid tradeId, CreateTradeExecutionRequest request, CancellationToken cancellationToken);
}
