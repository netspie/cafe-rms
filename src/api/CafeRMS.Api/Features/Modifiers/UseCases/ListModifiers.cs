using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

[ApiController]
public sealed class ListModifiersController : ControllerBase
{
    [HttpGet("/api/modifiers")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public Task<PagedResult<ListModifiers.Item>> Handle(
        [FromQuery] ListModifiersRequest request,
        [FromServices] AppDbContext db) =>
        ListModifiers.Execute(
            new ListModifiers.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name,
                ModifierGroupId = request.ModifierGroupId
            },
            db);
}

public sealed record ListModifiersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Name = null,
    Guid? ModifierGroupId = null);


public static class ListModifiers
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
        public Guid? ModifierGroupId { get; init; }
    }

    public sealed record Item(Guid Id, string Name, Guid ModifierGroupId, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Modifier>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Modifiers.AsQueryable();
        if (query.ModifierGroupId is Guid groupId)
            queryable = queryable.Where(x => x.ModifierGroupId == groupId);
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.ModifierGroupId, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
