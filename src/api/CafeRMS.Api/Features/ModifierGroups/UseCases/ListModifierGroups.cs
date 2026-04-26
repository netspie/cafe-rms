using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

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
