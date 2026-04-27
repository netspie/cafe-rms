using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.UseCases;

[ApiController]
public sealed class ListOrdersController : ControllerBase
{
    [HttpGet("/api/orders")]
    [Authorize(Policy = Permissions.OrdersView)]
    public Task<PagedResult<ListOrders.Item>> Handle(
        [FromQuery] ListOrdersRequest request,
        [FromServices] AppDbContext db) =>
        ListOrders.Execute(
            new ListOrders.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                OutletId = request.OutletId,
                UserId = request.UserId,
                SalesChannelId = request.SalesChannelId,
                EventId = request.EventId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            },
            db.CurrentCompanyId,
            db);
}

public sealed record ListOrdersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    Guid? OutletId = null,
    Guid? UserId = null,
    Guid? SalesChannelId = null,
    Guid? EventId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null);


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
