namespace ArcanaLedger.Server.Mocks;

// Internal in-memory mock entities. These intentionally mirror the ERD tables referenced in
// docs/api.md (app_users, portfolios, brokerage_accounts, ...) but are plain mutable classes,
// not EF Core entities or DTOs — they exist only to back the mock services in this folder.

internal sealed class UserEntity
{
    public required Guid Id { get; init; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class PortfolioEntity
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
    public required string BaseCurrency { get; set; }
    public bool IsDefault { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class AccountEntity
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public required string Name { get; set; }
    public required string BrokerName { get; set; }
    public required string Currency { get; set; }
    public bool IsActive { get; set; } = true;
    public required DateTimeOffset CreatedAt { get; init; }
}

internal sealed class InstrumentEntity
{
    public required Guid Id { get; init; }
    public required string Symbol { get; init; }
    public required string Name { get; init; }
    public string? Sector { get; init; }
    public required string AssetType { get; init; }
    public required string Currency { get; init; }
    public bool IsActive { get; init; } = true;
}

internal sealed class QuoteEntity
{
    public required Guid InstrumentId { get; init; }
    public required decimal LastPrice { get; set; }
    public decimal? PreviousClose { get; set; }
    public decimal? ChangeAmount { get; set; }
    public decimal? ChangePercent { get; set; }
    public required string Tone { get; set; }
    public required DateTimeOffset QuotedAt { get; set; }
}

internal sealed class WatchlistItemEntity
{
    public required Guid PortfolioId { get; init; }
    public required Guid InstrumentId { get; init; }
    public int DisplayOrder { get; set; }
    public required DateTimeOffset AddedAt { get; init; }
}

internal sealed class TradeSetupEntity
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Family { get; set; }
    public required string EntryRule { get; set; }
    public required string ExitRule { get; set; }
    public required string Trend { get; set; }
    public bool UsesTranches { get; set; }
    public int? TrancheCount { get; set; }
    public decimal? VarMultiplier { get; set; }
    public string? UsageNotes { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class TradeEntity
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public required Guid AccountId { get; init; }
    public required Guid InstrumentId { get; init; }
    public Guid? SetupId { get; set; }
    public required string OrderType { get; init; }
    public required string Status { get; set; }
    public required DateOnly OpenedOn { get; init; }
    public DateOnly? ClosedOn { get; set; }
    public required decimal EntryTarget { get; set; }
    public decimal? CutLoss { get; set; }
    public decimal? TargetPrice { get; set; }
    public decimal? UnrealizedPnlAmount { get; set; }
    public decimal? UnrealizedPnlPercent { get; set; }
    public string PositionResult { get; set; } = "neutral";
    public string? Notes { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class TradeExecutionEntity
{
    public required Guid Id { get; init; }
    public required Guid TradeId { get; init; }
    public required string Side { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public decimal Commission { get; init; }
    public required DateTimeOffset ExecutedAt { get; init; }
}

internal sealed class TransactionEntity
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public Guid? AccountId { get; init; }
    public Guid? InstrumentId { get; init; }
    public Guid? TradeId { get; init; }
    public required string TransactionType { get; init; }
    public required decimal CashAmount { get; init; }
    public required string Currency { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public string? Memo { get; init; }
}

internal sealed class SnapshotEntity
{
    public required Guid PortfolioId { get; init; }
    public required DateOnly AsOfDate { get; init; }
    public required decimal TotalValue { get; init; }
    public required decimal CashValue { get; init; }
    public required decimal InvestedValue { get; init; }
    public required decimal DailyReturn { get; init; }
    public required decimal TimeWeightedReturn { get; init; }
}

internal sealed class ActivityEntity
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
}

internal sealed class ValuationEntity
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid PortfolioId { get; init; }
    public required Guid InstrumentId { get; init; }
    public required string Name { get; set; }
    public required decimal EstimatedRevenue { get; set; }
    public required decimal EstimatedNetIncome { get; set; }
    public decimal? ActualPriorYearRevenue { get; set; }
    public decimal? ActualPriorYearNetIncome { get; set; }
    public required decimal SharesOutstanding { get; set; }
    public required double RevenueGrowthRate { get; set; }
    public required double TerminalMargin { get; set; }
    public required double PeMultiple { get; set; }
    public required decimal CurrentPrice { get; set; }
    public required double DiscountRate { get; set; }
    public required int ForecastYears { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class SettingsEntity
{
    public required Guid PortfolioId { get; init; }
    public required decimal Capital { get; set; }
    public required double VarRate { get; set; }
    public required decimal CashValue { get; set; }
    public required decimal ReserveValue { get; set; }
    public decimal? PositionEntryPrice { get; set; }
    public decimal? PositionExitPrice { get; set; }
    public decimal? PositionShares { get; set; }
    public decimal? PositionCashValue { get; set; }
    public decimal? TrancheEntryPrice { get; set; }
    public decimal? TrancheExitPrice { get; set; }
    public decimal? TrancheShares { get; set; }
    public decimal? TrancheCashValue { get; set; }
    public decimal? SwingEntryPrice { get; set; }
    public decimal? SwingExitPrice { get; set; }
    public decimal? SwingShares { get; set; }
    public decimal? SwingCashValue { get; set; }
    public decimal? BreakoutPrice { get; set; }
    public decimal? PatternLowPrice { get; set; }
    public decimal? SwingTargetPrice { get; set; }
    public double? MomentumRate { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
}
