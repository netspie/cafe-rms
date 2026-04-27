using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tags.UseCases;

[ApiController]
public sealed class ListTagsController : ControllerBase
{
    [HttpGet("/api/tags")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public Task<PagedResult<ListTags.Item>> Handle(
        [FromQuery] ListTags.Request request,
        [FromServices] AppDbContext db) =>
        ListTags.Execute(request, db);
}

public static class ListTags
{
    public sealed record Request(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);

    public sealed record Item(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Request request, AppDbContext db)
    {
        var query = new Query
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Sort = request.Sort,
            Name = request.Name,
        };

        var sortable = new SortMap<Tag>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Tags.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.ImageUrl, x.CreatedAt))
            .ToPagedResultAsync(query);
    }

    private sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }
}
