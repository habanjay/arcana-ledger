namespace ArcanaLedger.Server.Mocks;

/// <summary>
/// Shared helpers for deriving presentational "tone"/direction fields from numeric deltas,
/// matching the <c>positive</c>/<c>neutral</c>/<c>negative</c> and <c>up</c>/<c>neutral</c>/
/// <c>down</c> vocabularies used throughout <c>docs/api.md</c>.
/// </summary>
internal static class ToneHelper
{
    public static string PriceTone(decimal? change)
        => change switch
        {
            > 0 => "positive",
            < 0 => "negative",
            _ => "neutral",
        };

    public static string PositionResult(decimal? pnl)
        => pnl switch
        {
            > 0 => "up",
            < 0 => "down",
            _ => "neutral",
        };
}
