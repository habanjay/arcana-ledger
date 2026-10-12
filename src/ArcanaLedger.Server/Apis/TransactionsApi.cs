using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Transactions;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/transactions</c>.</summary>
public static class TransactionsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListPortfolioTransactions")
            .WithSummary("List ledger entries.")
            .Produces<IReadOnlyList<PortfolioTransactionResponse>>();

        group.MapPost("", Create)
            .WithName("CreatePortfolioTransaction")
            .WithSummary("Record a manual entry (deposit, withdrawal, fee, adjustment).")
            .Produces<PortfolioTransactionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> List(
        Guid portfolioId,
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ClaimsPrincipal user,
        IPortfolioTransactionService service,
        CancellationToken cancellationToken)
        => Results.Ok(await service.List(user.GetUserId(), portfolioId, new TransactionListFilter(type, from, to), cancellationToken));

    private static async Task<IResult> Create(Guid portfolioId, CreatePortfolioTransactionRequest request, ClaimsPrincipal user, IPortfolioTransactionService service, CancellationToken cancellationToken)
    {
        var transaction = await service.Create(user.GetUserId(), portfolioId, request, cancellationToken);
        return Results.Created($"/api/v1/portfolios/{portfolioId}/transactions/{transaction.Id}", transaction);
    }
}
