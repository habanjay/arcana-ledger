namespace ArcanaLedger.Server.Dtos.Common;

/// <summary>
/// A page of <typeparamref name="T"/> items, matching the pagination envelope documented
/// in <c>docs/api.md</c>.
/// </summary>
/// <param name="Items">The items in the current page.</param>
/// <param name="Page">The 1-based page number.</param>
/// <param name="PageSize">The number of items requested per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
