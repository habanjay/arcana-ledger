using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Snapshots;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/snapshots</c>.</summary>
public static class SnapshotsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListPortfolioSnapshots")
            .WithSummary("List daily snapshots for charting.")
            .Produces<IReadOnlyList<PortfolioSnapshotResponse>>();
    }

    private static async Task<IResult> List(
        Guid portfolioId,
        DateOnly? from,
        DateOnly? to,
        ClaimsPrincipal user,
        IPortfolioSnapshotService service,
        CancellationToken cancellationToken)
        => Results.Ok(new { items = await service.List(user.GetUserId(), portfolioId, from, to, cancellationToken) });
}
