using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArcanaLedger.Server.Dtos.Json;

namespace ArcanaLedger.Server.Dtos.Valuations;

/// <summary>Request body for <c>POST /api/v1/valuations</c>.</summary>
public sealed record CreateValuationModelRequest
{
    [Required]
    public required Guid PortfolioId { get; init; }

    [Required]
    public required Guid InstrumentId { get; init; }

    [Required]
    [MaxLength(128)]
    public required string Name { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal EstimatedRevenue { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal EstimatedNetIncome { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? ActualPriorYearRevenue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? ActualPriorYearNetIncome { get; init; }

    [Required]
    [Range(1, double.MaxValue)]
    public required decimal SharesOutstanding { get; init; }

    [Required]
    [Range(-1, 10)]
    public required double RevenueGrowthRate { get; init; }

    [Required]
    [Range(0, 1)]
    public required double TerminalMargin { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required double PeMultiple { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal CurrentPrice { get; init; }

    [Required]
    [Range(0.0001, 1)]
    public required double DiscountRate { get; init; }

    [Required]
    [Range(1, 20)]
    public required int ForecastYears { get; init; }
}

/// <summary>
/// Request body for <c>PATCH /api/v1/valuations/{valuationId}</c>. All fields are optional;
/// updating any input recalculates the forecasts.
/// </summary>
public sealed record UpdateValuationModelRequest
{
    [MaxLength(128)]
    public string? Name { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedRevenue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedNetIncome { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? ActualPriorYearRevenue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? ActualPriorYearNetIncome { get; init; }

    [Range(1, double.MaxValue)]
    public decimal? SharesOutstanding { get; init; }

    [Range(-1, 10)]
    public double? RevenueGrowthRate { get; init; }

    [Range(0, 1)]
    public double? TerminalMargin { get; init; }

    [Range(0, double.MaxValue)]
    public double? PeMultiple { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? CurrentPrice { get; init; }

    [Range(0.0001, 1)]
    public double? DiscountRate { get; init; }

    [Range(1, 20)]
    public int? ForecastYears { get; init; }
}

/// <summary>A single forecast year, mapped to <c>valuation_forecasts</c>.</summary>
public sealed record ValuationForecastResponse(
    int ForecastYear,
    decimal Revenue,
    decimal NetIncome,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal Eps,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal TargetPrice);

/// <summary>Response body representing a valuation model and its forecasts, mapped to <c>valuation_models</c>.</summary>
public sealed record ValuationModelResponse(
    Guid Id,
    Guid UserId,
    Guid PortfolioId,
    Guid InstrumentId,
    string Name,
    decimal EstimatedRevenue,
    decimal EstimatedNetIncome,
    decimal? ActualPriorYearRevenue,
    decimal? ActualPriorYearNetIncome,
    decimal SharesOutstanding,
    double RevenueGrowthRate,
    double TerminalMargin,
    double PeMultiple,
    [property: JsonConverter(typeof(MoneyJsonConverter))] decimal CurrentPrice,
    double DiscountRate,
    int ForecastYears,
    IReadOnlyList<ValuationForecastResponse> Forecasts,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
