namespace CafeRMS.Api.Shared.Paging;

// Standard REST pagination + sort convention used across all list endpoints.
//
// Pagination (?page=1&pageSize=20) follows the page-number style from the
// Microsoft REST API Guidelines:
//   https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#98-pagination
//
// Sort (?sort=name,-createdAt) follows the JSON:API sort convention: comma-
// separated field names, '-' prefix for descending, fields applied in order:
//   https://jsonapi.org/format/#fetching-sorting
public record PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = DefaultPageSize;
    public string? Sort { get; init; }
}
