using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

public static class ListLoyaltyEntries
{
    public sealed record Query : PagedQuery
    {
        public Guid? UserId { get; init; }
    }

    public sealed record Item(Guid Id, Guid UserId, int Points, string? Reason, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<LoyaltyPointLog>()
            .Add("createdAt", x => x.CreatedAt);

        // Staff side: ICompanyOwned global filter applies — caller's company only.
        var queryable = db.LoyaltyPointLogs.AsQueryable();
        if (query.UserId is { } userId)
            queryable = queryable.Where(x => x.UserId == userId);

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.UserId, x.Points, x.Reason, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
