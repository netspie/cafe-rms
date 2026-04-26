using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

[ApiController]
public sealed class ListProductListsController : ControllerBase
{
    [HttpGet("/api/product-lists")]
    [Authorize(Policy = Permissions.MenusManage)]
    public Task<PagedResult<ListProductLists.Item>> Handle(
        [FromQuery] ListProductListsRequest request,
        [FromServices] AppDbContext db) =>
        ListProductLists.Execute(
            new ListProductLists.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListProductListsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
