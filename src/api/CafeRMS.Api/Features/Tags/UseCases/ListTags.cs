using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.Tags.UseCases;

public static class ListTags
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
    }

    public sealed record Item(Guid Id, string Name, string? ImageUrl, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
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
}
