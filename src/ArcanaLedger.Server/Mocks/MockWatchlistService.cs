using ArcanaLedger.Server.Dtos.Watchlist;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IWatchlistService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockWatchlistService(MockDataStore store) : IWatchlistService
{
    public IReadOnlyList<WatchlistItemResponse> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        return store.WatchlistItems.Values
            .Where(w => w.PortfolioId == portfolioId)
            .OrderBy(w => w.DisplayOrder)
            .Select(ToResponse)
            .ToList();
    }

    public WatchlistItemResponse Add(Guid userId, Guid portfolioId, AddWatchlistItemRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, portfolioId);

            if (!store.Instruments.ContainsKey(request.InstrumentId))
            {
                throw ApiException.NotFound("Instrument", request.InstrumentId);
            }

            var key = (portfolioId, request.InstrumentId);
            if (store.WatchlistItems.ContainsKey(key))
            {
                throw ApiException.Conflict("watchlist_item_exists", "Instrument is already on the watchlist.");
            }

            var item = new WatchlistItemEntity
            {
                PortfolioId = portfolioId,
                InstrumentId = request.InstrumentId,
                DisplayOrder = request.DisplayOrder,
                AddedAt = DateTimeOffset.UtcNow,
            };
            store.WatchlistItems[key] = item;
            return ToResponse(item);
        }
    }

    public WatchlistItemResponse Reorder(Guid userId, Guid portfolioId, Guid instrumentId, UpdateWatchlistItemRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var item = GetOwnedItem(userId, portfolioId, instrumentId);
            item.DisplayOrder = request.DisplayOrder;
            return ToResponse(item);
        }
    }

    public void Remove(Guid userId, Guid portfolioId, Guid instrumentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            GetOwnedItem(userId, portfolioId, instrumentId);
            store.WatchlistItems.TryRemove((portfolioId, instrumentId), out _);
        }
    }

    private WatchlistItemEntity GetOwnedItem(Guid userId, Guid portfolioId, Guid instrumentId)
    {
        store.GetOwnedPortfolio(userId, portfolioId);

        if (!store.WatchlistItems.TryGetValue((portfolioId, instrumentId), out var item))
        {
            throw ApiException.NotFound("Watchlist item", instrumentId);
        }

        return item;
    }

    private WatchlistItemResponse ToResponse(WatchlistItemEntity item)
    {
        var instrument = store.Instruments[item.InstrumentId];
        var instrumentSummary = new WatchlistInstrumentSummary(instrument.Id, instrument.Symbol, instrument.Name, instrument.Sector);

        WatchlistQuoteSummary? quoteSummary = null;
        if (store.Quotes.TryGetValue(item.InstrumentId, out var quote))
        {
            quoteSummary = new WatchlistQuoteSummary(quote.LastPrice, quote.ChangeAmount, quote.ChangePercent, quote.Tone);
        }

        return new WatchlistItemResponse(item.PortfolioId, instrumentSummary, quoteSummary, item.DisplayOrder, item.AddedAt);
    }
}
