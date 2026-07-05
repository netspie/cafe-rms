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
                Status = request.Status,
                TableId = request.TableId,
                OutletId = request.OutletId,
                UserId = request.UserId,
                SalesChannelId = request.SalesChannelId,
                EventId = request.EventId,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            },
            db);
}

public sealed record ListOrdersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    OrderStatus? Status = null,
    Guid? TableId = null,
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
        public OrderStatus? Status { get; init; }
        public Guid? TableId { get; init; }
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
        Guid? TableId,
        string? TableName,
        Guid? UserId,
        string? CustomerName,
        OrderStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt,
        decimal Total);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Order>()
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Orders.AsQueryable();
        if (query.Status is OrderStatus status)
            queryable = queryable.Where(x => x.Status == status);
        if (query.TableId is Guid tableId)
            queryable = queryable.Where(x => x.TableId == tableId);
        if (query.OutletId is Guid outletId)
            queryable = queryable.Where(x => x.OutletId == outletId);
        if (query.UserId is Guid userId)
            queryable = queryable.Where(x => x.UserId == userId);
        if (query.SalesChannelId is Guid scId)
            queryable = queryable.Where(x => x.SalesChannelId == scId);
        if (query.EventId is Guid eventId)
            queryable = queryable.Where(x => x.EventId == eventId);
        if (query.FromDate is DateTimeOffset from)
            queryable = queryable.Where(x => x.CreatedAt >= from);
        if (query.ToDate is DateTimeOffset to)
            queryable = queryable.Where(x => x.CreatedAt <= to);

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "-createdAt")
            .Select(x => new Item(
                x.Id,
                x.OutletId,
                x.TableId,
                x.TableId == null
                    ? null
                    : db.Tables.Where(t => t.Id == x.TableId).Select(t => t.Name).FirstOrDefault(),
                x.UserId,
                x.UserId == null
                    ? null
                    : db.Users.Where(u => u.Id == x.UserId)
                        .Select(u => (u.FirstName + " " + u.LastName).Trim())
                        .FirstOrDefault(),
                x.Status,
                x.CreatedAt,
                x.ClosedAt,
                (db.OrderLines.Where(ol => ol.OrderId == x.Id)
                    .Sum(ol => (decimal?)((ol.NetPerOne + ol.VatPerOne) * ol.Quantity)) ?? 0m)
                    - x.Discount - x.LoyaltyPointsUsed))
            .ToPagedResultAsync(query);
    }
}
