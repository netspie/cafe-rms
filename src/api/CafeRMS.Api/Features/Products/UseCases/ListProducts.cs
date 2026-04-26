using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class ListProducts
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
        public string? Barcode { get; init; }
        public Guid? TaxRateId { get; init; }
    }

    public sealed record Item(Guid Id, string Name, string? Barcode, Guid TaxRateId, DateTimeOffset CreatedAt);

    public static async Task<PagedResult<Item>> Execute(Query query, AppDbContext db)
    {
        var sortable = new SortMap<Product>()
            .Add("name", x => x.Name)
            .Add("createdAt", x => x.CreatedAt);

        var queryable = db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var needle = query.Name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }
        if (!string.IsNullOrWhiteSpace(query.Barcode))
            queryable = queryable.Where(x => x.Barcode == query.Barcode);
        if (query.TaxRateId is { } taxRateId)
            queryable = queryable.Where(x => x.TaxRateId == taxRateId);

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.Barcode, x.TaxRateId, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
