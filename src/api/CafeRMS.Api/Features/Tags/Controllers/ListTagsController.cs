using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Tags.UseCases;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tags.Controllers;

[ApiController]
public sealed class ListTagsController : ControllerBase
{
    [HttpGet("/api/tags")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<PagedResult<ListTags.Item>> Handle(
        [FromQuery] ListTagsRequest request,
        [FromServices] AppDbContext db) =>
        ListTags.Execute(
            new ListTags.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListTagsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);
