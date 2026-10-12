using ArcanaLedger.Server.Dtos.Instruments;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Mocks;

/// <summary>Mock <see cref="IInstrumentService"/> backed by the in-memory <see cref="MockDataStore"/>.</summary>
internal sealed class MockInstrumentService(MockDataStore store) : IInstrumentService
{
    public IReadOnlyList<InstrumentResponse> Search(string? search, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = store.Instruments.Values.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(i => i.Symbol).Select(ToResponse).ToList();
    }

    public InstrumentResponse Get(Guid instrumentId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!store.Instruments.TryGetValue(instrumentId, out var instrument))
        {
            throw ApiException.NotFound("Instrument", instrumentId);
        }

        return ToResponse(instrument);
    }

    private InstrumentResponse ToResponse(InstrumentEntity instrument)
    {
        store.Quotes.TryGetValue(instrument.Id, out var quote);
        var quoteResponse = quote is null
            ? null
            : new QuoteResponse(quote.LastPrice, quote.PreviousClose, quote.ChangeAmount, quote.ChangePercent, quote.Tone, quote.QuotedAt);

        return new InstrumentResponse(
            instrument.Id,
            instrument.Symbol,
            instrument.Name,
            instrument.Sector,
            instrument.AssetType,
            instrument.Currency,
            instrument.IsActive,
            quoteResponse);
    }
}
