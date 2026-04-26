using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

public static class ListPromotionCodes
{
    public sealed record Query : PagedQuery
    {
        public string? Code { get; init; }
    }

    public sealed record Item(
        Guid Id,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses,
        int UsesCount,
        DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<PromotionCode>()
            .Add("code", x => x.Code)
            .Add("createdAt", x => x.CreatedAt)
            .Add("validUntil", x => x.ValidUntil);

        var queryable = db.PromotionCodes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Code))
        {
            var needle = query.Code.ToLower();
            queryable = queryable.Where(x => x.Code.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "code")
            .Select(x => new Item(
                x.Id, x.Code, x.DiscountPercentage,
                x.ValidFrom, x.ValidUntil, x.MaxUses, x.UsesCount,
                x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
