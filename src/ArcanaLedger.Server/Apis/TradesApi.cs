using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Trades;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/trades</c> and <c>/api/v1/trades/{tradeId}/executions</c>.</summary>
public static class TradesApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListTrades")
            .WithSummary("List trades with filters, includes cached P&L.")
            .Produces<Dtos.Common.PagedResult<TradeListItemResponse>>();

        group.MapPost("", Create)
            .WithName("CreateTrade")
            .WithSummary("Open a new trade (planned or open).")
            .Produces<TradeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{tradeId:guid}", Get)
            .WithName("GetTrade")
            .WithSummary("Get a trade, including its executions.")
            .Produces<TradeResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{tradeId:guid}", Update)
            .WithName("UpdateTrade")
            .WithSummary("Update targets, status, or notes.")
            .Produces<TradeResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{tradeId:guid}", Delete)
            .WithName("DeleteTrade")
            .WithSummary("Delete a trade (only if it has no executions).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{tradeId:guid}/executions", ListExecutions)
            .WithName("ListTradeExecutions")
            .WithSummary("List executions/fills for a trade.")
            .Produces<IReadOnlyList<TradeExecutionResponse>>();

        group.MapPost("/{tradeId:guid}/executions", AddExecution)
            .WithName("CreateTradeExecution")
            .WithSummary("Record a fill; recalculates cached P&L.")
            .Produces<TradeExecutionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static IResult List(
        Guid? portfolioId,
        string? status,
        string? symbol,
        int? page,
        int? pageSize,
        ClaimsPrincipal user,
        ITradeService service,
        CancellationToken cancellationToken)
    {
        var all = service.List(user.GetUserId(), new TradeListFilter(portfolioId, status, symbol), cancellationToken);
        var effectivePage = page is > 0 ? page.Value : 1;
        var effectivePageSize = pageSize is > 0 ? pageSize.Value : 25;
        var pageItems = all.Skip((effectivePage - 1) * effectivePageSize).Take(effectivePageSize).ToList();
        return Results.Ok(new Dtos.Common.PagedResult<TradeListItemResponse>(pageItems, effectivePage, effectivePageSize, all.Count));
    }

    private static IResult Create(CreateTradeRequest request, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
    {
        var trade = service.Create(user.GetUserId(), request, cancellationToken);
        return Results.Created($"/api/v1/trades/{trade.Id}", trade);
    }

    private static IResult Get(Guid tradeId, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
        => Results.Ok(service.Get(user.GetUserId(), tradeId, cancellationToken));

    private static IResult Update(Guid tradeId, UpdateTradeRequest request, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
        => Results.Ok(service.Update(user.GetUserId(), tradeId, request, cancellationToken));

    private static IResult Delete(Guid tradeId, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
    {
        service.Delete(user.GetUserId(), tradeId, cancellationToken);
        return Results.NoContent();
    }

    private static IResult ListExecutions(Guid tradeId, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
        => Results.Ok(service.ListExecutions(user.GetUserId(), tradeId, cancellationToken));

    private static IResult AddExecution(Guid tradeId, CreateTradeExecutionRequest request, ClaimsPrincipal user, ITradeService service, CancellationToken cancellationToken)
    {
        var execution = service.AddExecution(user.GetUserId(), tradeId, request, cancellationToken);
        return Results.Created($"/api/v1/trades/{tradeId}/executions/{execution.Id}", execution);
    }
}
