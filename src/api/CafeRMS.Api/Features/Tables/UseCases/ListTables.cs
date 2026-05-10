using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Tables.UseCases;

[ApiController]
public sealed class ListTablesController : ControllerBase
{
    [HttpGet("/api/tables")]
    [Authorize]
    public Task<PagedResult<ListTables.Item>> Handle(
        [FromQuery] ListTablesRequest request,
        [FromServices] AppDbContext db) =>
        ListTables.Execute(
            new ListTables.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListTablesRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);


public static class ListTables
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, Guid OutletId, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Table>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Tables.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.OutletId, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
