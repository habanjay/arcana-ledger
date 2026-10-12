using System.Collections.Concurrent;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>
/// In-memory mock store shared by every <c>Mocks/*</c> service implementation. Registered as
/// a singleton in <c>Program.cs</c> so all resource services see the same mock state. Local
/// and demo use only — not persistent storage.
/// </summary>
internal sealed class MockDataStore
{
    public ConcurrentDictionary<Guid, UserEntity> Users { get; } = new();
    public ConcurrentDictionary<Guid, PortfolioEntity> Portfolios { get; } = new();
    public ConcurrentDictionary<Guid, AccountEntity> Accounts { get; } = new();
    public ConcurrentDictionary<Guid, InstrumentEntity> Instruments { get; } = new();
    public ConcurrentDictionary<Guid, QuoteEntity> Quotes { get; } = new();
    public ConcurrentDictionary<(Guid PortfolioId, Guid InstrumentId), WatchlistItemEntity> WatchlistItems { get; } = new();
    public ConcurrentDictionary<Guid, TradeSetupEntity> TradeSetups { get; } = new();
    public ConcurrentDictionary<Guid, TradeEntity> Trades { get; } = new();
    public ConcurrentDictionary<Guid, TradeExecutionEntity> TradeExecutions { get; } = new();
    public ConcurrentDictionary<Guid, TransactionEntity> Transactions { get; } = new();
    public ConcurrentDictionary<Guid, SnapshotEntity> Snapshots { get; } = new();
    public ConcurrentDictionary<Guid, ActivityEntity> Activity { get; } = new();
    public ConcurrentDictionary<Guid, ValuationEntity> Valuations { get; } = new();
    public ConcurrentDictionary<Guid, SettingsEntity> Settings { get; } = new();

    /// <summary>Single mutation lock. The mock store favors simplicity and consistency over
    /// throughput; all demo data fits comfortably under a coarse-grained lock.</summary>
    public readonly object Gate = new();

    public MockDataStore()
    {
        Seed();
    }

    /// <summary>Resolves a portfolio and verifies it belongs to <paramref name="userId"/>.</summary>
    public PortfolioEntity GetOwnedPortfolio(Guid userId, Guid portfolioId)
    {
        if (!Portfolios.TryGetValue(portfolioId, out var portfolio))
        {
            throw ApiException.NotFound("Portfolio", portfolioId);
        }

        if (portfolio.UserId != userId)
        {
            throw ApiException.Forbidden("You do not have access to this portfolio.");
        }

        return portfolio;
    }

    /// <summary>Resolves a trade and verifies its parent portfolio belongs to <paramref name="userId"/>.</summary>
    public TradeEntity GetOwnedTrade(Guid userId, Guid tradeId)
    {
        if (!Trades.TryGetValue(tradeId, out var trade))
        {
            throw ApiException.NotFound("Trade", tradeId);
        }

        GetOwnedPortfolio(userId, trade.PortfolioId);
        return trade;
    }

    /// <summary>
    /// Recalculates a trade's cached unrealized P&amp;L and position result from its
    /// executions and the instrument's latest quote. Must be called under <see cref="Gate"/>.
    /// </summary>
    public void RecalculateTradePnl(TradeEntity trade)
    {
        var executions = TradeExecutions.Values.Where(e => e.TradeId == trade.Id).ToList();
        if (executions.Count == 0 || !Quotes.TryGetValue(trade.InstrumentId, out var quote))
        {
            trade.UnrealizedPnlAmount = null;
            trade.UnrealizedPnlPercent = null;
            trade.PositionResult = "neutral";
            return;
        }

        var buyQuantity = executions.Where(e => e.Side == "buy").Sum(e => e.Quantity);
        var sellQuantity = executions.Where(e => e.Side == "sell").Sum(e => e.Quantity);
        var netShares = buyQuantity - sellQuantity;
        var costBasis = executions.Where(e => e.Side == "buy").Sum(e => e.Quantity * e.Price);

        if (netShares <= 0 || buyQuantity == 0)
        {
            trade.UnrealizedPnlAmount = null;
            trade.UnrealizedPnlPercent = null;
            trade.PositionResult = "neutral";
            return;
        }

        var averageEntryPrice = costBasis / buyQuantity;
        var direction = trade.OrderType == "short" ? -1m : 1m;
        var pnlAmount = (quote.LastPrice - averageEntryPrice) * netShares * direction;
        var pnlPercent = averageEntryPrice == 0 ? 0 : pnlAmount / (averageEntryPrice * netShares) * 100m;

        trade.UnrealizedPnlAmount = pnlAmount;
        trade.UnrealizedPnlPercent = pnlPercent;
        trade.PositionResult = ToneHelper.PositionResult(pnlAmount);

        if (trade.Status == "planned")
        {
            trade.Status = "open";
        }
    }

    private void Seed()
    {
        // Seed IDs intentionally reuse the example identifiers from docs/api.md so the
        // ArcanaLedger.Server.http requests can be copy/pasted as working smoke tests.
        var now = new DateTimeOffset(2026, 9, 29, 14, 3, 0, TimeSpan.Zero);

        var userId = Guid.Parse("b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10");
        Users[userId] = new UserEntity
        {
            Id = userId,
            Email = "trader@example.com",
            DisplayName = "Ada Trader",
            PasswordHash = PasswordHasher.Hash("Str0ngPassword!"),
            CreatedAt = now,
            UpdatedAt = now,
        };

        var portfolioId = Guid.Parse("1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f");
        Portfolios[portfolioId] = new PortfolioEntity
        {
            Id = portfolioId,
            UserId = userId,
            Name = "Growth",
            BaseCurrency = "CAD",
            IsDefault = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var accountId = Guid.Parse("2b1c6a2e-9f9f-4c3c-8b9b-3c4d5e6f7a8b");
        Accounts[accountId] = new AccountEntity
        {
            Id = accountId,
            PortfolioId = portfolioId,
            Name = "Questrade Margin",
            BrokerName = "Questrade",
            Currency = "CAD",
            IsActive = true,
            CreatedAt = now,
        };

        var aaplId = Guid.Parse("6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f");
        Instruments[aaplId] = new InstrumentEntity
        {
            Id = aaplId,
            Symbol = "AAPL",
            Name = "Apple Inc.",
            Sector = "Technology",
            AssetType = "equity",
            Currency = "USD",
            IsActive = true,
        };
        Quotes[aaplId] = new QuoteEntity
        {
            InstrumentId = aaplId,
            LastPrice = 182.50000000m,
            PreviousClose = 180.10000000m,
            ChangeAmount = 2.40000000m,
            ChangePercent = 1.33259300m,
            Tone = "positive",
            QuotedAt = now.AddMinutes(-4),
        };

        var msftId = Guid.Parse("a1b2c3d4-e5f6-4a1a-8b9c-0d1e2f3a4b5c");
        Instruments[msftId] = new InstrumentEntity
        {
            Id = msftId,
            Symbol = "MSFT",
            Name = "Microsoft Corporation",
            Sector = "Technology",
            AssetType = "equity",
            Currency = "USD",
            IsActive = true,
        };
        Quotes[msftId] = new QuoteEntity
        {
            InstrumentId = msftId,
            LastPrice = 415.20000000m,
            PreviousClose = 418.60000000m,
            ChangeAmount = -3.40000000m,
            ChangePercent = -0.81223000m,
            Tone = "negative",
            QuotedAt = now.AddMinutes(-4),
        };

        WatchlistItems[(portfolioId, aaplId)] = new WatchlistItemEntity
        {
            PortfolioId = portfolioId,
            InstrumentId = aaplId,
            DisplayOrder = 0,
            AddedAt = now,
        };
        WatchlistItems[(portfolioId, msftId)] = new WatchlistItemEntity
        {
            PortfolioId = portfolioId,
            InstrumentId = msftId,
            DisplayOrder = 1,
            AddedAt = now,
        };

        var setupId = Guid.Parse("4f5e6d7c-8b9a-4a1a-9b8b-7c6d5e4f3a2b");
        TradeSetups[setupId] = new TradeSetupEntity
        {
            Id = setupId,
            UserId = userId,
            Code = "BOOMER",
            Name = "Boomer",
            Family = "position",
            EntryRule = "Breakout above 20-day high with volume confirmation.",
            ExitRule = "Close below 10-day EMA.",
            Trend = "up",
            UsesTranches = true,
            TrancheCount = 3,
            VarMultiplier = 1.5m,
            UsageNotes = "Use for high-conviction, longer-duration positions.",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var tradeId = Guid.Parse("9a8b7c6d-5e4f-4a1a-9b8b-3a2b1c6a2e9f");
        var trade = new TradeEntity
        {
            Id = tradeId,
            PortfolioId = portfolioId,
            AccountId = accountId,
            InstrumentId = aaplId,
            SetupId = setupId,
            OrderType = "long",
            Status = "planned",
            OpenedOn = DateOnly.FromDateTime(now.Date),
            EntryTarget = 180.00000000m,
            CutLoss = 172.00000000m,
            TargetPrice = 205.00000000m,
            Notes = "Breakout setup off the August base.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        Trades[tradeId] = trade;

        var executionId = Guid.Parse("c6d5e4f3-a2b1-4c6a-8e9f-9a8b7c6d5e4f");
        TradeExecutions[executionId] = new TradeExecutionEntity
        {
            Id = executionId,
            TradeId = tradeId,
            Side = "buy",
            Quantity = 50.00000000m,
            Price = 181.20000000m,
            Commission = 1.99000000m,
            ExecutedAt = now.AddMinutes(28),
        };
        RecalculateTradePnl(trade);

        var transactionId = Guid.NewGuid();
        Transactions[transactionId] = new TransactionEntity
        {
            Id = transactionId,
            PortfolioId = portfolioId,
            AccountId = accountId,
            TransactionType = "deposit",
            CashAmount = 5000.00000000m,
            Currency = "CAD",
            OccurredAt = now.AddMinutes(-3),
            Memo = "Initial funding",
        };

        var snapshotId = Guid.NewGuid();
        Snapshots[snapshotId] = new SnapshotEntity
        {
            PortfolioId = portfolioId,
            AsOfDate = DateOnly.FromDateTime(now.Date),
            TotalValue = 128450.32000000m,
            CashValue = 12450.00000000m,
            InvestedValue = 116000.32000000m,
            DailyReturn = 0.00842100m,
            TimeWeightedReturn = 0.08213400m,
        };

        var activityId = Guid.Parse("7c6d5e4f-3a2b-4c6a-8e9f-1e3a3b3e2f2f");
        Activity[activityId] = new ActivityEntity
        {
            Id = activityId,
            PortfolioId = portfolioId,
            Category = "trade",
            Title = "Opened AAPL long",
            Detail = "50 shares at $181.20 using the Boomer setup.",
            OccurredAt = now.AddMinutes(28),
        };

        var valuationId = Guid.Parse("5e4f3a2b-1c6a-4c6a-8e9f-6d5e4f3a2b1c");
        Valuations[valuationId] = new ValuationEntity
        {
            Id = valuationId,
            UserId = userId,
            PortfolioId = portfolioId,
            InstrumentId = aaplId,
            Name = "AAPL base case",
            EstimatedRevenue = 400_000_000_000m,
            EstimatedNetIncome = 100_000_000_000m,
            ActualPriorYearRevenue = 383_000_000_000m,
            ActualPriorYearNetIncome = 97_000_000_000m,
            SharesOutstanding = 15_200_000_000m,
            RevenueGrowthRate = 0.08,
            TerminalMargin = 0.25,
            PeMultiple = 28,
            CurrentPrice = 182.50000000m,
            DiscountRate = 0.08,
            ForecastYears = 5,
            CreatedAt = now,
            UpdatedAt = now,
        };

        Settings[portfolioId] = new SettingsEntity
        {
            PortfolioId = portfolioId,
            Capital = 100000.00000000m,
            VarRate = 0.02,
            CashValue = 15000.00000000m,
            ReserveValue = 10000.00000000m,
            PositionEntryPrice = 180.00000000m,
            PositionExitPrice = 172.00000000m,
            PositionShares = 50.00000000m,
            PositionCashValue = 9000.00000000m,
            TrancheEntryPrice = 180.00000000m,
            TrancheExitPrice = 172.00000000m,
            TrancheShares = 17.00000000m,
            TrancheCashValue = 3060.00000000m,
            SwingEntryPrice = 180.00000000m,
            SwingExitPrice = 172.00000000m,
            SwingShares = 25.00000000m,
            SwingCashValue = 4500.00000000m,
            BreakoutPrice = 185.00000000m,
            PatternLowPrice = 170.00000000m,
            SwingTargetPrice = 205.00000000m,
            MomentumRate = 0.015,
            UpdatedAt = now,
        };
    }
}
