using ArcanaLedger.Server.Dtos.Settings;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IPortfolioSettingsService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockPortfolioSettingsService(MockDataStore store) : IPortfolioSettingsService
{
    public PortfolioSettingsResponse Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        store.GetOwnedPortfolio(userId, portfolioId);

        if (!store.Settings.TryGetValue(portfolioId, out var settings))
        {
            throw ApiException.NotFound("Portfolio settings", portfolioId);
        }

        return ToResponse(settings);
    }

    public PortfolioSettingsResponse Upsert(Guid userId, Guid portfolioId, UpsertPortfolioSettingsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, portfolioId);

            var settings = new SettingsEntity
            {
                PortfolioId = portfolioId,
                Capital = request.Capital,
                VarRate = request.VarRate,
                CashValue = request.CashValue,
                ReserveValue = request.ReserveValue,
                PositionEntryPrice = request.PositionEntryPrice,
                PositionExitPrice = request.PositionExitPrice,
                PositionShares = request.PositionShares,
                PositionCashValue = request.PositionCashValue,
                TrancheEntryPrice = request.TrancheEntryPrice,
                TrancheExitPrice = request.TrancheExitPrice,
                TrancheShares = request.TrancheShares,
                TrancheCashValue = request.TrancheCashValue,
                SwingEntryPrice = request.SwingEntryPrice,
                SwingExitPrice = request.SwingExitPrice,
                SwingShares = request.SwingShares,
                SwingCashValue = request.SwingCashValue,
                BreakoutPrice = request.BreakoutPrice,
                PatternLowPrice = request.PatternLowPrice,
                SwingTargetPrice = request.SwingTargetPrice,
                MomentumRate = request.MomentumRate,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            store.Settings[portfolioId] = settings;
            return ToResponse(settings);
        }
    }

    private static PortfolioSettingsResponse ToResponse(SettingsEntity settings)
    {
        var cashAllocationPercent = settings.Capital == 0 ? 0 : (double)(settings.CashValue / settings.Capital * 100m);
        var reserveAllocationPercent = settings.Capital == 0 ? 0 : (double)(settings.ReserveValue / settings.Capital * 100m);

        return new PortfolioSettingsResponse(
            settings.PortfolioId,
            settings.Capital,
            settings.VarRate,
            settings.CashValue,
            cashAllocationPercent,
            settings.ReserveValue,
            reserveAllocationPercent,
            settings.PositionEntryPrice,
            settings.PositionExitPrice,
            settings.PositionShares,
            settings.PositionCashValue,
            settings.TrancheEntryPrice,
            settings.TrancheExitPrice,
            settings.TrancheShares,
            settings.TrancheCashValue,
            settings.SwingEntryPrice,
            settings.SwingExitPrice,
            settings.SwingShares,
            settings.SwingCashValue,
            settings.BreakoutPrice,
            settings.PatternLowPrice,
            settings.SwingTargetPrice,
            settings.MomentumRate,
            settings.UpdatedAt);
    }
}
