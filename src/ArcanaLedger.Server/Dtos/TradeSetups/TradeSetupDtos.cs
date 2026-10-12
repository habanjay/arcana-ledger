using System.ComponentModel.DataAnnotations;

namespace ArcanaLedger.Server.Dtos.TradeSetups;

/// <summary>Request body for <c>POST /api/v1/trade-setups</c>.</summary>
public sealed record CreateTradeSetupRequest
{
    [Required]
    [MaxLength(32)]
    public required string Code { get; init; }

    [Required]
    [MaxLength(128)]
    public required string Name { get; init; }

    [Required]
    [AllowedValues("position", "swing", "momentum", "day", ErrorMessage = "Family must be one of: position, swing, momentum, day.")]
    public required string Family { get; init; }

    [Required]
    public required string EntryRule { get; init; }

    [Required]
    public required string ExitRule { get; init; }

    [Required]
    [AllowedValues("up", "down", "neutral", ErrorMessage = "Trend must be one of: up, down, neutral.")]
    public required string Trend { get; init; }

    public bool UsesTranches { get; init; }

    [Range(1, 20)]
    public int? TrancheCount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? VarMultiplier { get; init; }

    public string? UsageNotes { get; init; }
}

/// <summary>Request body for <c>PATCH /api/v1/trade-setups/{setupId}</c>. All fields are optional.</summary>
public sealed record UpdateTradeSetupRequest
{
    [MaxLength(32)]
    public string? Code { get; init; }

    [MaxLength(128)]
    public string? Name { get; init; }

    [AllowedValues("position", "swing", "momentum", "day", ErrorMessage = "Family must be one of: position, swing, momentum, day.")]
    public string? Family { get; init; }

    public string? EntryRule { get; init; }

    public string? ExitRule { get; init; }

    [AllowedValues("up", "down", "neutral", ErrorMessage = "Trend must be one of: up, down, neutral.")]
    public string? Trend { get; init; }

    public bool? UsesTranches { get; init; }

    [Range(1, 20)]
    public int? TrancheCount { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? VarMultiplier { get; init; }

    public string? UsageNotes { get; init; }
}

/// <summary>Response body representing a trade setup, mapped to <c>trade_setups</c>.</summary>
public sealed record TradeSetupResponse(
    Guid Id,
    Guid UserId,
    string Code,
    string Name,
    string Family,
    string EntryRule,
    string ExitRule,
    string Trend,
    bool UsesTranches,
    int? TrancheCount,
    decimal? VarMultiplier,
    string? UsageNotes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
