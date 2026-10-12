using ArcanaLedger.Server.Dtos.TradeSetups;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="ITradeSetupService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockTradeSetupService(MockDataStore store) : ITradeSetupService
{
    public IReadOnlyList<TradeSetupResponse> List(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return store.TradeSetups.Values
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.CreatedAt)
            .Select(ToResponse)
            .ToList();
    }

    public TradeSetupResponse Create(Guid userId, CreateTradeSetupRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var now = DateTimeOffset.UtcNow;
            var setup = new TradeSetupEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Code = request.Code,
                Name = request.Name,
                Family = request.Family,
                EntryRule = request.EntryRule,
                ExitRule = request.ExitRule,
                Trend = request.Trend,
                UsesTranches = request.UsesTranches,
                TrancheCount = request.TrancheCount,
                VarMultiplier = request.VarMultiplier,
                UsageNotes = request.UsageNotes,
                CreatedAt = now,
                UpdatedAt = now,
            };
            store.TradeSetups[setup.Id] = setup;
            return ToResponse(setup);
        }
    }

    public TradeSetupResponse Get(Guid userId, Guid setupId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ToResponse(GetOwned(userId, setupId));
    }

    public TradeSetupResponse Update(Guid userId, Guid setupId, UpdateTradeSetupRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var setup = GetOwned(userId, setupId);

            if (request.Code is not null) setup.Code = request.Code;
            if (request.Name is not null) setup.Name = request.Name;
            if (request.Family is not null) setup.Family = request.Family;
            if (request.EntryRule is not null) setup.EntryRule = request.EntryRule;
            if (request.ExitRule is not null) setup.ExitRule = request.ExitRule;
            if (request.Trend is not null) setup.Trend = request.Trend;
            if (request.UsesTranches is not null) setup.UsesTranches = request.UsesTranches.Value;
            if (request.TrancheCount is not null) setup.TrancheCount = request.TrancheCount;
            if (request.VarMultiplier is not null) setup.VarMultiplier = request.VarMultiplier;
            if (request.UsageNotes is not null) setup.UsageNotes = request.UsageNotes;

            setup.UpdatedAt = DateTimeOffset.UtcNow;
            return ToResponse(setup);
        }
    }

    public void Delete(Guid userId, Guid setupId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            GetOwned(userId, setupId);
            store.TradeSetups.TryRemove(setupId, out _);

            // Trades keep history via ON DELETE SET NULL on trade_setups.
            foreach (var trade in store.Trades.Values.Where(t => t.SetupId == setupId))
            {
                trade.SetupId = null;
            }
        }
    }

    private TradeSetupEntity GetOwned(Guid userId, Guid setupId)
    {
        if (!store.TradeSetups.TryGetValue(setupId, out var setup))
        {
            throw ApiException.NotFound("Trade setup", setupId);
        }

        if (setup.UserId != userId)
        {
            throw ApiException.Forbidden("You do not have access to this trade setup.");
        }

        return setup;
    }

    private static TradeSetupResponse ToResponse(TradeSetupEntity setup)
        => new(
            setup.Id,
            setup.UserId,
            setup.Code,
            setup.Name,
            setup.Family,
            setup.EntryRule,
            setup.ExitRule,
            setup.Trend,
            setup.UsesTranches,
            setup.TrancheCount,
            setup.VarMultiplier,
            setup.UsageNotes,
            setup.CreatedAt,
            setup.UpdatedAt);
}
