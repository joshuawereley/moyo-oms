namespace Moyo.Oms.Application.Common;

/// <summary>
/// A single page of results together with the total count for paging.
/// </summary>

public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
