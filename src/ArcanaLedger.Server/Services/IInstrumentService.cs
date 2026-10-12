using ArcanaLedger.Server.Dtos.Instruments;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Read-only instrument and quote lookups backing <c>/api/v1/instruments</c>, mapped to
/// the <c>instruments</c> and <c>instrument_quotes</c> tables.
/// </summary>
public interface IInstrumentService
{
    Task<IReadOnlyList<InstrumentResponse>> Search(string? search, CancellationToken cancellationToken);

    Task<InstrumentResponse> Get(Guid instrumentId, CancellationToken cancellationToken);
}
