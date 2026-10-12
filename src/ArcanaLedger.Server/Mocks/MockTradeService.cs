using ArcanaLedger.Server.Dtos.Trades;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="ITradeService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockTradeService(MockDataStore store) : ITradeService
{
    public IReadOnlyList<TradeListItemResponse> List(Guid userId, TradeListFilter filter, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<TradeEntity> trades;
        if (filter.PortfolioId is { } portfolioId)
        {
            store.GetOwnedPortfolio(userId, portfolioId);
            trades = store.Trades.Values.Where(t => t.PortfolioId == portfolioId);
        }
        else
        {
            var ownedPortfolioIds = store.Portfolios.Values.Where(p => p.UserId == userId).Select(p => p.Id).ToHashSet();
            trades = store.Trades.Values.Where(t => ownedPortfolioIds.Contains(t.PortfolioId));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            trades = trades.Where(t => string.Equals(t.Status, filter.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Symbol))
        {
            trades = trades.Where(t => store.Instruments.TryGetValue(t.InstrumentId, out var instrument)
                && string.Equals(instrument.Symbol, filter.Symbol, StringComparison.OrdinalIgnoreCase));
        }

        return trades.OrderByDescending(t => t.OpenedOn).Select(ToListItem).ToList();
    }

    public TradeResponse Create(Guid userId, CreateTradeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            if (!store.Accounts.TryGetValue(request.AccountId, out var account))
            {
                throw ApiException.NotFound("Account", request.AccountId);
            }

            store.GetOwnedPortfolio(userId, account.PortfolioId);

            if (!store.Instruments.ContainsKey(request.InstrumentId))
            {
                throw ApiException.NotFound("Instrument", request.InstrumentId);
            }

            if (request.SetupId is { } setupId)
            {
                if (!store.TradeSetups.TryGetValue(setupId, out var setup) || setup.UserId != userId)
                {
                    throw ApiException.NotFound("Trade setup", setupId);
                }
            }

            var now = DateTimeOffset.UtcNow;
            var trade = new TradeEntity
            {
                Id = Guid.NewGuid(),
                PortfolioId = account.PortfolioId,
                AccountId = request.AccountId,
                InstrumentId = request.InstrumentId,
                SetupId = request.SetupId,
                OrderType = request.OrderType,
                Status = "planned",
                OpenedOn = request.OpenedOn,
                EntryTarget = request.EntryTarget,
                CutLoss = request.CutLoss,
                TargetPrice = request.TargetPrice,
                Notes = request.Notes,
                CreatedAt = now,
                UpdatedAt = now,
            };
            store.Trades[trade.Id] = trade;
            return ToResponse(trade, includeExecutions: false);
        }
    }

    public TradeResponse Get(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ToResponse(store.GetOwnedTrade(userId, tradeId), includeExecutions: true);
    }

    public TradeResponse Update(Guid userId, Guid tradeId, UpdateTradeRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var trade = store.GetOwnedTrade(userId, tradeId);

            if (request.EntryTarget is not null) trade.EntryTarget = request.EntryTarget.Value;
            if (request.CutLoss is not null) trade.CutLoss = request.CutLoss;
            if (request.TargetPrice is not null) trade.TargetPrice = request.TargetPrice;
            if (request.Status is not null) trade.Status = request.Status;
            if (request.ClosedOn is not null) trade.ClosedOn = request.ClosedOn;
            if (request.Notes is not null) trade.Notes = request.Notes;

            trade.UpdatedAt = DateTimeOffset.UtcNow;
            return ToResponse(trade, includeExecutions: true);
        }
    }

    public void Delete(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var trade = store.GetOwnedTrade(userId, tradeId);
            if (store.TradeExecutions.Values.Any(e => e.TradeId == tradeId))
            {
                throw ApiException.Conflict("trade_has_executions", "Trade cannot be deleted because it has executions.");
            }

            store.Trades.TryRemove(tradeId, out _);
        }
    }

    public IReadOnlyList<TradeExecutionResponse> ListExecutions(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedTrade(userId, tradeId);

        return store.TradeExecutions.Values
            .Where(e => e.TradeId == tradeId)
            .OrderBy(e => e.ExecutedAt)
            .Select(ToExecutionResponse)
            .ToList();
    }

    public TradeExecutionResponse AddExecution(Guid userId, Guid tradeId, CreateTradeExecutionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var trade = store.GetOwnedTrade(userId, tradeId);

            var execution = new TradeExecutionEntity
            {
                Id = Guid.NewGuid(),
                TradeId = tradeId,
                Side = request.Side,
                Quantity = request.Quantity,
                Price = request.Price,
                Commission = request.Commission,
                ExecutedAt = request.ExecutedAt,
            };
            store.TradeExecutions[execution.Id] = execution;
            store.RecalculateTradePnl(trade);

            var instrument = store.Instruments[trade.InstrumentId];
            store.Activity[Guid.NewGuid()] = new ActivityEntity
            {
                Id = Guid.NewGuid(),
                PortfolioId = trade.PortfolioId,
                Category = "trade",
                Title = $"{(request.Side == "buy" ? "Bought" : "Sold")} {instrument.Symbol}",
                Detail = $"{request.Quantity:F2} shares at ${request.Price:F2}.",
                OccurredAt = request.ExecutedAt,
            };

            return ToExecutionResponse(execution);
        }
    }

    private TradeResponse ToResponse(TradeEntity trade, bool includeExecutions)
    {
        var instrument = store.Instruments[trade.InstrumentId];
        TradeSetupSummary? setupSummary = null;
        if (trade.SetupId is { } setupId && store.TradeSetups.TryGetValue(setupId, out var setup))
        {
            setupSummary = new TradeSetupSummary(setup.Id, setup.Code, setup.Name);
        }

        IReadOnlyList<TradeExecutionResponse>? executions = includeExecutions
            ? store.TradeExecutions.Values.Where(e => e.TradeId == trade.Id).OrderBy(e => e.ExecutedAt).Select(ToExecutionResponse).ToList()
            : null;

        return new TradeResponse(
            trade.Id,
            trade.PortfolioId,
            trade.AccountId,
            new TradeInstrumentSummary(instrument.Id, instrument.Symbol),
            setupSummary,
            trade.OrderType,
            trade.Status,
            trade.OpenedOn,
            trade.ClosedOn,
            trade.EntryTarget,
            trade.CutLoss,
            trade.TargetPrice,
            trade.UnrealizedPnlAmount,
            trade.UnrealizedPnlPercent,
            trade.PositionResult,
            trade.Notes,
            trade.CreatedAt,
            trade.UpdatedAt,
            executions);
    }

    private TradeListItemResponse ToListItem(TradeEntity trade)
    {
        var instrument = store.Instruments[trade.InstrumentId];
        string? setupName = trade.SetupId is { } setupId && store.TradeSetups.TryGetValue(setupId, out var setup) ? setup.Name : null;

        var executions = store.TradeExecutions.Values.Where(e => e.TradeId == trade.Id).ToList();
        var buyQuantity = executions.Where(e => e.Side == "buy").Sum(e => e.Quantity);
        var sellQuantity = executions.Where(e => e.Side == "sell").Sum(e => e.Quantity);
        var netShares = buyQuantity - sellQuantity;
        var entryPrice = buyQuantity > 0
            ? executions.Where(e => e.Side == "buy").Sum(e => e.Quantity * e.Price) / buyQuantity
            : trade.EntryTarget;

        store.Quotes.TryGetValue(trade.InstrumentId, out var quote);

        return new TradeListItemResponse(
            trade.Id,
            instrument.Symbol,
            setupName,
            netShares,
            trade.OpenedOn,
            entryPrice,
            trade.CutLoss,
            trade.TargetPrice,
            trade.PositionResult,
            quote?.LastPrice,
            quote?.ChangePercent);
    }

    private static TradeExecutionResponse ToExecutionResponse(TradeExecutionEntity execution)
        => new(execution.Id, execution.TradeId, execution.Side, execution.Quantity, execution.Price, execution.Commission, execution.ExecutedAt);
}
