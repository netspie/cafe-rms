using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

[ApiController]
public sealed class ListModifierGroupsController : ControllerBase
{
    [HttpGet("/api/modifier-groups")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public Task<PagedResult<ListModifierGroups.Item>> Handle(
        [FromQuery] ListModifierGroupsRequest request,
        [FromServices] AppDbContext db) =>
        ListModifierGroups.Execute(
            new ListModifierGroups.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name
            },
            db);
}

public sealed record ListModifierGroupsRequest(int Page = 1, int PageSize = 20, string? Sort = null, string? Name = null);


public static class ListModifierGroups
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<ModifierGroup>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.ModifierGroups.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
