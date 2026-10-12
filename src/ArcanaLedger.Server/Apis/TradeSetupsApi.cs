using System.Security.Claims;
using ArcanaLedger.Server.Dtos.TradeSetups;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/trade-setups</c>.</summary>
public static class TradeSetupsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListTradeSetups")
            .WithSummary("List the caller's setups.")
            .Produces<IReadOnlyList<TradeSetupResponse>>();

        group.MapPost("", Create)
            .WithName("CreateTradeSetup")
            .WithSummary("Create a setup.")
            .Produces<TradeSetupResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{setupId:guid}", Get)
            .WithName("GetTradeSetup")
            .WithSummary("Get a setup.")
            .Produces<TradeSetupResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{setupId:guid}", Update)
            .WithName("UpdateTradeSetup")
            .WithSummary("Update a setup.")
            .Produces<TradeSetupResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{setupId:guid}", Delete)
            .WithName("DeleteTradeSetup")
            .WithSummary("Delete a setup (trades keep history via ON DELETE SET NULL).")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> List(ClaimsPrincipal user, ITradeSetupService service, CancellationToken cancellationToken)
        => Results.Ok(await service.List(user.GetUserId(), cancellationToken));

    private static async Task<IResult> Create(CreateTradeSetupRequest request, ClaimsPrincipal user, ITradeSetupService service, CancellationToken cancellationToken)
    {
        var setup = await service.Create(user.GetUserId(), request, cancellationToken);
        return Results.Created($"/api/v1/trade-setups/{setup.Id}", setup);
    }

    private static async Task<IResult> Get(Guid setupId, ClaimsPrincipal user, ITradeSetupService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Get(user.GetUserId(), setupId, cancellationToken));

    private static async Task<IResult> Update(Guid setupId, UpdateTradeSetupRequest request, ClaimsPrincipal user, ITradeSetupService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Update(user.GetUserId(), setupId, request, cancellationToken));

    private static async Task<IResult> Delete(Guid setupId, ClaimsPrincipal user, ITradeSetupService service, CancellationToken cancellationToken)
    {
        await service.Delete(user.GetUserId(), setupId, cancellationToken);
        return Results.NoContent();
    }
}
