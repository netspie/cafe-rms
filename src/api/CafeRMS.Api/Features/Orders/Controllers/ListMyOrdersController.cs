using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Orders.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Orders.Controllers;

[ApiController]
public sealed class ListMyOrdersController : ControllerBase
{
    [HttpGet("/api/my/orders")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<PagedResult<ListMyOrders.Item>> Handle(
        [FromQuery] ListMyOrdersRequest request,
        [FromServices] AppDbContext db) =>
        ListMyOrders.Execute(
            new ListMyOrders.Query { Page = request.Page, PageSize = request.PageSize, Sort = request.Sort },
            User.UserId,
            db);
}

public sealed record ListMyOrdersRequest(int Page = 1, int PageSize = 20, string? Sort = null);
