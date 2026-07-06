using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class ListMyOrdersController : ControllerBase
{
    [HttpGet("/api/my/orders")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<PagedResult<ListMyOrders.Item>> Handle(
        [FromQuery] ListMyOrdersRequest request,
        [FromServices] AppDbContext db) =>
        ListMyOrders.Execute(
            new ListMyOrders.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Status = request.Status
            },
            User.UserId,
            db);
}

public sealed record ListMyOrdersRequest(int Page = 1, int PageSize = 20, string? Sort = null, OrderStatus? Status = null);


public static class ListMyOrders
{
    public sealed record Query : PagedQuery
    {
        public OrderStatus? Status { get; init; }
    }

    public sealed record Item(
        Guid Id,
        Guid OutletId,
        OrderStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, Guid userId, AppDbContext db)
    {
        var sortable = new SortMap<Order>().Add("createdAt", x => x.CreatedAt);

        var queryable = db.Orders.Where(x => x.UserId == userId);

        if (query.Status is OrderStatus status)
            queryable = status switch
            {
                OrderStatus.Placed => queryable.Where(x => x.ClosedAt == null && x.CancelledAt == null),
                OrderStatus.Closed => queryable.Where(x => x.ClosedAt != null && x.CancelledAt == null),
                OrderStatus.Cancelled => queryable.Where(x => x.CancelledAt != null),
                _ => queryable
            };

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(x.Id, x.OutletId, x.Status, x.CreatedAt, x.ClosedAt))
            .ToPagedResultAsync(query);
    }
}
