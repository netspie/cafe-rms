using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

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
