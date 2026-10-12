using ArcanaLedger.Server.Dtos.Instruments;

namespace ArcanaLedger.Server.Services;

/// <summary>
/// Read-only instrument and quote lookups backing <c>/api/v1/instruments</c>, mapped to
/// the <c>instruments</c> and <c>instrument_quotes</c> tables.
/// </summary>
public interface IInstrumentService
{
    IReadOnlyList<InstrumentResponse> Search(string? search, CancellationToken cancellationToken);

    InstrumentResponse Get(Guid instrumentId, CancellationToken cancellationToken);
}
