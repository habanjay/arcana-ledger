using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Activity;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/activity</c>.</summary>
public static class ActivityApi
{
    private const int DefaultLimit = 10;

    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListPortfolioActivity")
            .WithSummary("List recent activity, newest first.")
            .Produces<IReadOnlyList<PortfolioActivityResponse>>();
    }

    private static async Task<IResult> List(Guid portfolioId, int? limit, ClaimsPrincipal user, IPortfolioActivityService service, CancellationToken cancellationToken)
        => Results.Ok(new { items = await service.List(user.GetUserId(), portfolioId, limit ?? DefaultLimit, cancellationToken) });
}
