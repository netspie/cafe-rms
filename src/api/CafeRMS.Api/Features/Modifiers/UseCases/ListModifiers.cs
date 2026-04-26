using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

public static class ListModifiers
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
        public Guid? ModifierGroupId { get; init; }
    }

    public sealed record Item(Guid Id, string Name, decimal PriceDelta, Guid ModifierGroupId, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Modifier>()
            .Add("name", x => x.Name)
            .Add("priceDelta", x => x.PriceDelta)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Modifiers.AsQueryable();
        if (query.ModifierGroupId is { } groupId)
            queryable = queryable.Where(x => x.ModifierGroupId == groupId);
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.PriceDelta, x.ModifierGroupId, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
