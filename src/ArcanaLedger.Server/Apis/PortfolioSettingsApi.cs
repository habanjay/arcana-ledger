using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Settings;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/settings</c>.</summary>
public static class PortfolioSettingsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", Get)
            .WithName("GetPortfolioSettings")
            .WithSummary("Get the portfolio's configuration.")
            .Produces<PortfolioSettingsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("", Upsert)
            .WithName("UpsertPortfolioSettings")
            .WithSummary("Replace the portfolio's configuration (upsert).")
            .Produces<PortfolioSettingsResponse>()
            .ProducesValidationProblem();
    }

    private static IResult Get(Guid portfolioId, ClaimsPrincipal user, IPortfolioSettingsService service, CancellationToken cancellationToken)
        => Results.Ok(service.Get(user.GetUserId(), portfolioId, cancellationToken));

    private static IResult Upsert(Guid portfolioId, UpsertPortfolioSettingsRequest request, ClaimsPrincipal user, IPortfolioSettingsService service, CancellationToken cancellationToken)
        => Results.Ok(service.Upsert(user.GetUserId(), portfolioId, request, cancellationToken));
}
