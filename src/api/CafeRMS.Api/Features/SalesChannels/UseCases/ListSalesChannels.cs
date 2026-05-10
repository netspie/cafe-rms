using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class ListSalesChannelsController : ControllerBase
{
    [HttpGet("/api/sales-channels")]
    [Authorize]
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


public static class ListSalesChannels
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, bool IsTakeout, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<SalesChannel>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.SalesChannels.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.IsTakeout, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
