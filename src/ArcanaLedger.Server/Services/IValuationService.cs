using ArcanaLedger.Server.Dtos.Valuations;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Valuation model operations backing <c>/api/v1/valuations</c>, mapped to the
/// <c>valuation_models</c> and <c>valuation_forecasts</c> tables.
/// </summary>
public interface IValuationService
{
    Task<IReadOnlyList<ValuationModelResponse>> List(Guid userId, Guid? instrumentId, CancellationToken cancellationToken);

    Task<ValuationModelResponse> Create(Guid userId, CreateValuationModelRequest request, CancellationToken cancellationToken);

    Task<ValuationModelResponse> Get(Guid userId, Guid valuationId, CancellationToken cancellationToken);

    Task<ValuationModelResponse> Update(Guid userId, Guid valuationId, UpdateValuationModelRequest request, CancellationToken cancellationToken);

    Task Delete(Guid userId, Guid valuationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ValuationForecastResponse>> ListForecasts(Guid userId, Guid valuationId, CancellationToken cancellationToken);
}
