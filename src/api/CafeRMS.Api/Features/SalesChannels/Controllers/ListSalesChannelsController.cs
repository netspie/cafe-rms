using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

[ApiController]
public sealed class ListSalesChannelsController : ControllerBase
{
    [HttpGet("/api/sales-channels")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public Task<PagedResult<ListSalesChannels.Item>> Handle(
        [FromQuery] ListSalesChannelsRequest request,
        [FromServices] AppDbContext db) =>
        ListSalesChannels.Execute(
            new ListSalesChannels.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListSalesChannelsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
