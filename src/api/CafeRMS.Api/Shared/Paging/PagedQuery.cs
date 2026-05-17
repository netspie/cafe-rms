namespace CafeRMS.Api.Shared.Paging;

public record PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
    public string? Sort { get; init; }
}
