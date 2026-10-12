using ArcanaLedger.Server.Dtos.Valuations;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IValuationService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockValuationService(MockDataStore store) : IValuationService
{
    public IReadOnlyList<ValuationModelResponse> List(Guid userId, Guid? instrumentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = store.Valuations.Values.Where(v => v.UserId == userId);
        if (instrumentId is { } id)
        {
            query = query.Where(v => v.InstrumentId == id);
        }

        return query.OrderByDescending(v => v.CreatedAt).Select(ToResponse).ToList();
    }

    public ValuationModelResponse Create(Guid userId, CreateValuationModelRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            store.GetOwnedPortfolio(userId, request.PortfolioId);

            if (!store.Instruments.ContainsKey(request.InstrumentId))
            {
                throw ApiException.NotFound("Instrument", request.InstrumentId);
            }

            var now = DateTimeOffset.UtcNow;
            var valuation = new ValuationEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PortfolioId = request.PortfolioId,
                InstrumentId = request.InstrumentId,
                Name = request.Name,
                EstimatedRevenue = request.EstimatedRevenue,
                EstimatedNetIncome = request.EstimatedNetIncome,
                ActualPriorYearRevenue = request.ActualPriorYearRevenue,
                ActualPriorYearNetIncome = request.ActualPriorYearNetIncome,
                SharesOutstanding = request.SharesOutstanding,
                RevenueGrowthRate = request.RevenueGrowthRate,
                TerminalMargin = request.TerminalMargin,
                PeMultiple = request.PeMultiple,
                CurrentPrice = request.CurrentPrice,
                DiscountRate = request.DiscountRate,
                ForecastYears = request.ForecastYears,
                CreatedAt = now,
                UpdatedAt = now,
            };
            store.Valuations[valuation.Id] = valuation;
            return ToResponse(valuation);
        }
    }

    public ValuationModelResponse Get(Guid userId, Guid valuationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ToResponse(GetOwned(userId, valuationId));
    }

    public ValuationModelResponse Update(Guid userId, Guid valuationId, UpdateValuationModelRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            var valuation = GetOwned(userId, valuationId);

            if (request.Name is not null) valuation.Name = request.Name;
            if (request.EstimatedRevenue is not null) valuation.EstimatedRevenue = request.EstimatedRevenue.Value;
            if (request.EstimatedNetIncome is not null) valuation.EstimatedNetIncome = request.EstimatedNetIncome.Value;
            if (request.ActualPriorYearRevenue is not null) valuation.ActualPriorYearRevenue = request.ActualPriorYearRevenue;
            if (request.ActualPriorYearNetIncome is not null) valuation.ActualPriorYearNetIncome = request.ActualPriorYearNetIncome;
            if (request.SharesOutstanding is not null) valuation.SharesOutstanding = request.SharesOutstanding.Value;
            if (request.RevenueGrowthRate is not null) valuation.RevenueGrowthRate = request.RevenueGrowthRate.Value;
            if (request.TerminalMargin is not null) valuation.TerminalMargin = request.TerminalMargin.Value;
            if (request.PeMultiple is not null) valuation.PeMultiple = request.PeMultiple.Value;
            if (request.CurrentPrice is not null) valuation.CurrentPrice = request.CurrentPrice.Value;
            if (request.DiscountRate is not null) valuation.DiscountRate = request.DiscountRate.Value;
            if (request.ForecastYears is not null) valuation.ForecastYears = request.ForecastYears.Value;

            // Any input change recalculates forecasts; they are derived, not stored.
            valuation.UpdatedAt = DateTimeOffset.UtcNow;
            return ToResponse(valuation);
        }
    }

    public void Delete(Guid userId, Guid valuationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (store.Gate)
        {
            GetOwned(userId, valuationId);
            store.Valuations.TryRemove(valuationId, out _);
        }
    }

    public IReadOnlyList<ValuationForecastResponse> ListForecasts(Guid userId, Guid valuationId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return BuildForecasts(GetOwned(userId, valuationId));
    }

    private ValuationEntity GetOwned(Guid userId, Guid valuationId)
    {
        if (!store.Valuations.TryGetValue(valuationId, out var valuation))
        {
            throw ApiException.NotFound("Valuation model", valuationId);
        }

        if (valuation.UserId != userId)
        {
            throw ApiException.Forbidden("You do not have access to this valuation model.");
        }

        return valuation;
    }

    /// <summary>
    /// Projects revenue and net income forward by compounding <see cref="ValuationEntity.RevenueGrowthRate"/>,
    /// then derives EPS and an undiscounted target price from <see cref="ValuationEntity.PeMultiple"/>.
    /// A simplified, deterministic mock calculation — not a substitute for a real DCF engine.
    /// </summary>
    private static IReadOnlyList<ValuationForecastResponse> BuildForecasts(ValuationEntity valuation)
    {
        var forecasts = new List<ValuationForecastResponse>(valuation.ForecastYears);
        var growthFactor = 1 + (decimal)valuation.RevenueGrowthRate;

        for (var year = 1; year <= valuation.ForecastYears; year++)
        {
            var compounding = (decimal)Math.Pow((double)growthFactor, year);
            var revenue = valuation.EstimatedRevenue * compounding;
            var netIncome = valuation.EstimatedNetIncome * compounding;
            var eps = valuation.SharesOutstanding == 0 ? 0m : netIncome / valuation.SharesOutstanding;
            var targetPrice = eps * (decimal)valuation.PeMultiple;

            forecasts.Add(new ValuationForecastResponse(year, revenue, netIncome, eps, targetPrice));
        }

        return forecasts;
    }

    private static ValuationModelResponse ToResponse(ValuationEntity valuation)
        => new(
            valuation.Id,
            valuation.UserId,
            valuation.PortfolioId,
            valuation.InstrumentId,
            valuation.Name,
            valuation.EstimatedRevenue,
            valuation.EstimatedNetIncome,
            valuation.ActualPriorYearRevenue,
            valuation.ActualPriorYearNetIncome,
            valuation.SharesOutstanding,
            valuation.RevenueGrowthRate,
            valuation.TerminalMargin,
            valuation.PeMultiple,
            valuation.CurrentPrice,
            valuation.DiscountRate,
            valuation.ForecastYears,
            BuildForecasts(valuation),
            valuation.CreatedAt,
            valuation.UpdatedAt);
}
