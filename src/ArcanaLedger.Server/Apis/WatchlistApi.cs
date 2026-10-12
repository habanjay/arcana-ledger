using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Watchlist;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/watchlist</c>.</summary>
public static class WatchlistApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListWatchlistItems")
            .WithSummary("List watchlist items in display order.")
            .Produces<IReadOnlyList<WatchlistItemResponse>>();

        group.MapPost("", Add)
            .WithName("AddWatchlistItem")
            .WithSummary("Add an instrument to the watchlist.")
            .Produces<WatchlistItemResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{instrumentId:guid}", Reorder)
            .WithName("ReorderWatchlistItem")
            .WithSummary("Reorder an item (displayOrder).")
            .Produces<WatchlistItemResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{instrumentId:guid}", Remove)
            .WithName("RemoveWatchlistItem")
            .WithSummary("Remove an item.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> List(Guid portfolioId, ClaimsPrincipal user, IWatchlistService service, CancellationToken cancellationToken)
        => Results.Ok(await service.List(user.GetUserId(), portfolioId, cancellationToken));

    private static async Task<IResult> Add(Guid portfolioId, AddWatchlistItemRequest request, ClaimsPrincipal user, IWatchlistService service, CancellationToken cancellationToken)
    {
        var item = await service.Add(user.GetUserId(), portfolioId, request, cancellationToken);
        return Results.Created($"/api/v1/portfolios/{portfolioId}/watchlist/{request.InstrumentId}", item);
    }

    private static async Task<IResult> Reorder(Guid portfolioId, Guid instrumentId, UpdateWatchlistItemRequest request, ClaimsPrincipal user, IWatchlistService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Reorder(user.GetUserId(), portfolioId, instrumentId, request, cancellationToken));

    private static async Task<IResult> Remove(Guid portfolioId, Guid instrumentId, ClaimsPrincipal user, IWatchlistService service, CancellationToken cancellationToken)
    {
        await service.Remove(user.GetUserId(), portfolioId, instrumentId, cancellationToken);
        return Results.NoContent();
    }
}
