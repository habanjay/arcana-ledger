using ArcanaLedger.Server.Dtos.Valuations;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Valuation model operations backing <c>/api/v1/valuations</c>, mapped to the
/// <c>valuation_models</c> and <c>valuation_forecasts</c> tables.
/// </summary>
public interface IValuationService
{
    IReadOnlyList<ValuationModelResponse> List(Guid userId, Guid? instrumentId, CancellationToken cancellationToken);

    ValuationModelResponse Create(Guid userId, CreateValuationModelRequest request, CancellationToken cancellationToken);

    ValuationModelResponse Get(Guid userId, Guid valuationId, CancellationToken cancellationToken);

    ValuationModelResponse Update(Guid userId, Guid valuationId, UpdateValuationModelRequest request, CancellationToken cancellationToken);

    void Delete(Guid userId, Guid valuationId, CancellationToken cancellationToken);

    IReadOnlyList<ValuationForecastResponse> ListForecasts(Guid userId, Guid valuationId, CancellationToken cancellationToken);
}
