using ArcanaLedger.Server.Dtos.Watchlist;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Watchlist operations backing <c>/api/v1/portfolios/{portfolioId}/watchlist</c>, mapped
/// to the <c>watchlist_items</c> table.
/// </summary>
public interface IWatchlistService
{
    Task<IReadOnlyList<WatchlistItemResponse>> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken);

    Task<WatchlistItemResponse> Add(Guid userId, Guid portfolioId, AddWatchlistItemRequest request, CancellationToken cancellationToken);

    Task<WatchlistItemResponse> Reorder(Guid userId, Guid portfolioId, Guid instrumentId, UpdateWatchlistItemRequest request, CancellationToken cancellationToken);

    Task Remove(Guid userId, Guid portfolioId, Guid instrumentId, CancellationToken cancellationToken);
}
