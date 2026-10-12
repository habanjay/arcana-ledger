using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Accounts;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/portfolios/{portfolioId}/accounts</c>.</summary>
public static class AccountsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListAccounts")
            .WithSummary("List accounts in a portfolio.")
            .Produces<IReadOnlyList<BrokerageAccountResponse>>();

        group.MapPost("", Create)
            .WithName("CreateAccount")
            .WithSummary("Add an account.")
            .Produces<BrokerageAccountResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPatch("/{accountId:guid}", Update)
            .WithName("UpdateAccount")
            .WithSummary("Rename, change currency, or activate/deactivate.")
            .Produces<BrokerageAccountResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{accountId:guid}", Delete)
            .WithName("DeleteAccount")
            .WithSummary("Remove an account.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> List(Guid portfolioId, ClaimsPrincipal user, IBrokerageAccountService service, CancellationToken cancellationToken)
        => Results.Ok(await service.List(user.GetUserId(), portfolioId, cancellationToken));

    private static async Task<IResult> Create(Guid portfolioId, CreateAccountRequest request, ClaimsPrincipal user, IBrokerageAccountService service, CancellationToken cancellationToken)
    {
        var account = await service.Create(user.GetUserId(), portfolioId, request, cancellationToken);
        return Results.Created($"/api/v1/portfolios/{portfolioId}/accounts/{account.Id}", account);
    }

    private static async Task<IResult> Update(Guid portfolioId, Guid accountId, UpdateAccountRequest request, ClaimsPrincipal user, IBrokerageAccountService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Update(user.GetUserId(), portfolioId, accountId, request, cancellationToken));

    private static async Task<IResult> Delete(Guid portfolioId, Guid accountId, ClaimsPrincipal user, IBrokerageAccountService service, CancellationToken cancellationToken)
    {
        await service.Delete(user.GetUserId(), portfolioId, accountId, cancellationToken);
        return Results.NoContent();
    }
}
