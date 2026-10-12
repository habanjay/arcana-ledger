using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Dashboard;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/dashboard</c>.</summary>
public static class DashboardApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", Get)
            .WithName("GetDashboardSummary")
            .WithSummary("Get the dashboard summary.")
            .WithDescription("Combines the latest snapshot, watchlist, and activity so the client does not have to make four round trips.")
            .Produces<DashboardSummaryResponse>();
    }

    private static IResult Get(Guid portfolioId, ClaimsPrincipal user, IDashboardService service, CancellationToken cancellationToken)
        => Results.Ok(service.GetSummary(user.GetUserId(), portfolioId, cancellationToken));
}
