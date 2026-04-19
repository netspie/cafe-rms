namespace CafeRMS.Api.Shared.Paging;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
