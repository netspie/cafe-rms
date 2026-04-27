using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.Orders.UseCases;

public static class ListOrders
{
    public sealed record Query : PagedQuery
    {
        public Guid? OutletId { get; init; }
        public Guid? UserId { get; init; }
        public Guid? SalesChannelId { get; init; }
        public Guid? EventId { get; init; }
        public DateTimeOffset? FromDate { get; init; }
        public DateTimeOffset? ToDate { get; init; }
    }

    public sealed record Item(
        Guid Id,
        Guid OutletId,
        Guid? UserId,
        OrderStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, Guid companyId, AppDbContext db)
    {
        var sortable = new SortMap<Order>()
            .Add("createdAt", x => x.CreatedAt);

        // Order isn't ICompanyOwned (it's scoped via Outlet) — filter explicitly.
        var queryable = db.Orders.Where(x => x.Outlet!.CompanyId == companyId);
        if (query.OutletId is { } outletId)
            queryable = queryable.Where(x => x.OutletId == outletId);
        if (query.UserId is { } userId)
            queryable = queryable.Where(x => x.UserId == userId);
        if (query.SalesChannelId is { } scId)
            queryable = queryable.Where(x => x.SalesChannelId == scId);
        if (query.EventId is { } eventId)
            queryable = queryable.Where(x => x.EventId == eventId);
        if (query.FromDate is { } from)
            queryable = queryable.Where(x => x.CreatedAt >= from);
        if (query.ToDate is { } to)
            queryable = queryable.Where(x => x.CreatedAt <= to);

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.OutletId, x.UserId, x.Status, x.CreatedAt, x.ClosedAt))
            .ToPagedResultAsync(query);
    }
}
