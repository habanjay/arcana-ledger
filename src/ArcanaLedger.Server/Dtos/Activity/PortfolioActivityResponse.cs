namespace ArcanaLedger.Server.Dtos.Activity;

/// <summary>A recent activity entry, mapped to <c>portfolio_activity</c>.</summary>
public sealed record PortfolioActivityResponse(
    Guid Id,
    string Category,
    string Title,
    string Detail,
    DateTimeOffset OccurredAt);
