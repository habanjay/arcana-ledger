namespace ArcanaLedger.Server.Apis;

/// <summary>
/// Maps the <c>/api/v1</c> route group for the Arcana Ledger business API. Resource modules
/// each expose a static <c>Map</c> method that receives the appropriate route group; add new
/// resource modules here rather than mapping routes directly in <c>Program.cs</c>.
/// </summary>
public static class ArcanaLedgerEndpoints
{
    public static IEndpointRouteBuilder MapArcanaLedgerApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").AddEndpointFilter<ApiValidationFilter>();

        // Public, unauthenticated endpoints (register/login).
        var anonymousAuth = api.MapGroup("/auth").WithTags("Auth").AllowAnonymous();
        AuthApi.MapAnonymous(anonymousAuth);

        // Everything else requires a valid bearer token.
        var authorized = api.MapGroup("").RequireAuthorization();

        AuthApi.MapAuthorized(authorized.MapGroup("/auth").WithTags("Auth"));
        PortfoliosApi.Map(authorized.MapGroup("/portfolios").WithTags("Portfolios"));
        InstrumentsApi.Map(authorized.MapGroup("/instruments").WithTags("Instruments"));
        TradeSetupsApi.Map(authorized.MapGroup("/trade-setups").WithTags("Trade Setups"));
        TradesApi.Map(authorized.MapGroup("/trades").WithTags("Trades"));
        ValuationsApi.Map(authorized.MapGroup("/valuations").WithTags("Valuations"));

        return app;
    }
}
