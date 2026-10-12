using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Portfolios;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios</c> and the portfolio-scoped sub-resource groups.</summary>
public static class PortfoliosApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListPortfolios")
            .WithSummary("List the caller's portfolios.")
            .Produces<IReadOnlyList<PortfolioResponse>>();

        group.MapPost("", Create)
            .WithName("CreatePortfolio")
            .WithSummary("Create a portfolio.")
            .Produces<PortfolioResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{portfolioId:guid}", Get)
            .WithName("GetPortfolio")
            .WithSummary("Get a portfolio.")
            .Produces<PortfolioResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{portfolioId:guid}", Update)
            .WithName("UpdatePortfolio")
            .WithSummary("Update name, base currency, or default flag.")
            .Produces<PortfolioResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{portfolioId:guid}", Delete)
            .WithName("DeletePortfolio")
            .WithSummary("Delete a portfolio (cascades accounts, trades, etc.).")
            .Produces(StatusCodes.Status204NoContent);

        var portfolioScoped = group.MapGroup("/{portfolioId:guid}");
        AccountsApi.Map(portfolioScoped.MapGroup("/accounts").WithTags("Accounts"));
        WatchlistApi.Map(portfolioScoped.MapGroup("/watchlist").WithTags("Watchlist"));
        TransactionsApi.Map(portfolioScoped.MapGroup("/transactions").WithTags("Transactions"));
        SnapshotsApi.Map(portfolioScoped.MapGroup("/snapshots").WithTags("Snapshots"));
        ActivityApi.Map(portfolioScoped.MapGroup("/activity").WithTags("Activity"));
        DashboardApi.Map(portfolioScoped.MapGroup("/dashboard").WithTags("Dashboard"));
        PortfolioSettingsApi.Map(portfolioScoped.MapGroup("/settings").WithTags("Portfolio settings"));
    }

    private static IResult List(ClaimsPrincipal user, IPortfolioService service, CancellationToken cancellationToken)
        => Results.Ok(service.List(user.GetUserId(), cancellationToken));

    private static IResult Create(CreatePortfolioRequest request, ClaimsPrincipal user, IPortfolioService service, CancellationToken cancellationToken)
    {
        var portfolio = service.Create(user.GetUserId(), request, cancellationToken);
        return Results.Created($"/api/v1/portfolios/{portfolio.Id}", portfolio);
    }

    private static IResult Get(Guid portfolioId, ClaimsPrincipal user, IPortfolioService service, CancellationToken cancellationToken)
        => Results.Ok(service.Get(user.GetUserId(), portfolioId, cancellationToken));

    private static IResult Update(Guid portfolioId, UpdatePortfolioRequest request, ClaimsPrincipal user, IPortfolioService service, CancellationToken cancellationToken)
        => Results.Ok(service.Update(user.GetUserId(), portfolioId, request, cancellationToken));

    private static IResult Delete(Guid portfolioId, ClaimsPrincipal user, IPortfolioService service, CancellationToken cancellationToken)
    {
        service.Delete(user.GetUserId(), portfolioId, cancellationToken);
        return Results.NoContent();
    }
}
