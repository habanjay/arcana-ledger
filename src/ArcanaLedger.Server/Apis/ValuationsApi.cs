using System.Security.Claims;
using ArcanaLedger.Server.Dtos.Valuations;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>Maps <c>/api/v1/valuations</c>.</summary>
public static class ValuationsApi
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", List)
            .WithName("ListValuations")
            .WithSummary("List the caller's valuation models.")
            .Produces<IReadOnlyList<ValuationModelResponse>>();

        group.MapPost("", Create)
            .WithName("CreateValuation")
            .WithSummary("Create a valuation model (also generates its forecasts).")
            .Produces<ValuationModelResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{valuationId:guid}", Get)
            .WithName("GetValuation")
            .WithSummary("Get a valuation model, including forecasts.")
            .Produces<ValuationModelResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{valuationId:guid}", Update)
            .WithName("UpdateValuation")
            .WithSummary("Update inputs (recalculates forecasts).")
            .Produces<ValuationModelResponse>()
            .ProducesValidationProblem();

        group.MapDelete("/{valuationId:guid}", Delete)
            .WithName("DeleteValuation")
            .WithSummary("Delete a valuation model.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{valuationId:guid}/forecasts", ListForecasts)
            .WithName("ListValuationForecasts")
            .WithSummary("List forecast years.")
            .Produces<IReadOnlyList<ValuationForecastResponse>>();
    }

    private static async Task<IResult> List(Guid? instrumentId, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.List(user.GetUserId(), instrumentId, cancellationToken));

    private static async Task<IResult> Create(CreateValuationModelRequest request, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
    {
        var valuation = await service.Create(user.GetUserId(), request, cancellationToken);
        return Results.Created($"/api/v1/valuations/{valuation.Id}", valuation);
    }

    private static async Task<IResult> Get(Guid valuationId, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Get(user.GetUserId(), valuationId, cancellationToken));

    private static async Task<IResult> Update(Guid valuationId, UpdateValuationModelRequest request, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.Update(user.GetUserId(), valuationId, request, cancellationToken));

    private static async Task<IResult> Delete(Guid valuationId, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
    {
        await service.Delete(user.GetUserId(), valuationId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListForecasts(Guid valuationId, ClaimsPrincipal user, IValuationService service, CancellationToken cancellationToken)
        => Results.Ok(await service.ListForecasts(user.GetUserId(), valuationId, cancellationToken));
}
