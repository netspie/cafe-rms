using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

public static class ListMyOrders
{
    public sealed record Query : PagedQuery;

    public sealed record Item(
        Guid Id,
        Guid OutletId,
        OrderStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, Guid userId, AppDbContext db)
    {
        var sortable = new SortMap<Order>()
            .Add("createdAt", x => x.CreatedAt);

        return await db.Orders
            .Where(x => x.UserId == userId)
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.OutletId, x.Status, x.CreatedAt, x.ClosedAt))
            .ToPagedResultAsync(query);
    }
}
