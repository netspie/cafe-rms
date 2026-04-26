using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PriceGroups.Controllers;

[ApiController]
public sealed class ListPriceGroupsController : ControllerBase
{
    [HttpGet("/api/price-groups")]
    [Authorize(Policy = Permissions.PricingManage)]
    public Task<PagedResult<ListPriceGroups.Item>> Handle(
        [FromQuery] ListPriceGroupsRequest request,
        [FromServices] AppDbContext db) =>
        ListPriceGroups.Execute(
            new ListPriceGroups.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListPriceGroupsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
