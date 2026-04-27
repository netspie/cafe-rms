using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

public static class ListMyLoyaltyHistory
{
    public sealed record Query : PagedQuery;

    public sealed record Item(Guid Id, int Points, string? Reason, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Guid userId, Query query, AppDbContext db)
    {
        var sortable = new SortMap<LoyaltyPointLog>()
            .Add("createdAt", x => x.CreatedAt);

        return await db.LoyaltyPointLogs.IgnoreQueryFilters()
            .Where(x => x.UserId == userId)
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.Points, x.Reason, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
