using ArcanaLedger.Server.Dtos.Instruments;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/instruments</c>. Read-only; maintained by a market-data sync job.</summary>
public static class InstrumentsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", Search)
            .WithName("SearchInstruments")
            .WithSummary("Search instruments by symbol/name.")
            .Produces<IReadOnlyList<InstrumentResponse>>();

        group.MapGet("/{instrumentId:guid}", Get)
            .WithName("GetInstrument")
            .WithSummary("Get an instrument, including its latest quote.")
            .Produces<InstrumentResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Search(string? search, IInstrumentService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Search(search, cancellationToken));

    private static async Task<IResult> Get(Guid instrumentId, IInstrumentService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Get(instrumentId, cancellationToken));
}
