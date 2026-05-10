using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class ListProductsController : ControllerBase
{
    [HttpGet("/api/products")]
    [Authorize]
    public Task<PagedResult<ListProducts.Item>> Handle(
        [FromQuery] ListProductsRequest request,
        [FromServices] AppDbContext db) =>
        ListProducts.Execute(
            new ListProducts.Query
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Sort = request.Sort,
                Name = request.Name,
                Barcode = request.Barcode,
                TaxRateId = request.TaxRateId,
                TagId = request.TagId
            },
            db);
}

public sealed record ListProductsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Name = null,
    string? Barcode = null,
    Guid? TaxRateId = null,
    Guid? TagId = null);


public static class ListProducts
{
    public sealed record Query : PagedQuery
    {
        public string? Name { get; init; }
        public string? Barcode { get; init; }
        public Guid? TaxRateId { get; init; }
        public Guid? TagId { get; init; }
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
        if (query.TaxRateId is Guid taxRateId)
            queryable = queryable.Where(x => x.TaxRateId == taxRateId);
        if (query.TagId is Guid tagId)
            queryable = queryable.Where(x =>
                db.ProductTags.Any(pt => pt.ProductId == x.Id && pt.TagId == tagId));

        return await queryable
            .ApplySort(query.Sort, sortable, defaultSortExpression: "name")
            .Select(x => new Item(x.Id, x.Name, x.Barcode, x.TaxRateId, x.CreatedAt))
            .ToPagedResultAsync(query);
    }
}
