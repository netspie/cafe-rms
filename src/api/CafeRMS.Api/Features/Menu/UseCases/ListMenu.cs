using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Menu.UseCases;

[ApiController]
public sealed class ListMenuController : ControllerBase
{
    [HttpGet("/api/menu")]
    [Authorize]
    public Task<IReadOnlyList<ListMenu.Item>> Handle(
        [FromQuery] string? name,
        [FromQuery] Guid? tagId,
        [FromServices] AppDbContext db) =>
        ListMenu.Execute(name, tagId, db, DateTimeOffset.UtcNow);
}


public static class ListMenu
{
    public sealed record Item(Guid Id, string Name, string? Barcode, string? ImageUrl, decimal Price, decimal? OriginalPrice, bool IsEventPrice);

    public static async Task<IReadOnlyList<Item>> Execute(string? name, Guid? tagId, AppDbContext db, DateTimeOffset now)
    {
        var outlet = await db.Outlets
            .Select(x => new { x.DefaultPriceGroupId, x.DefaultProductListId })
            .FirstOrDefaultAsync();

        var defaultPriceGroupId = outlet?.DefaultPriceGroupId;
        var activeEvent = await ActiveEventResolver.ResolveAsync(db, now);
        var eventPriceGroupId = activeEvent?.PriceGroupId;
        var eventProductIds = activeEvent?.ProductIds ?? new List<Guid>();

        var queryable = db.Products.AsQueryable();
        if (outlet?.DefaultProductListId is Guid listId)
            queryable = queryable.Where(x =>
                db.ProductListItems.Any(pli => pli.ProductId == x.Id && pli.ProductListId == listId));
        if (!string.IsNullOrWhiteSpace(name))
        {
            var needle = name.ToLower();
            queryable = queryable.Where(x => x.Name.ToLower().Contains(needle));
        }
        if (tagId is Guid tag)
            queryable = queryable.Where(x => db.ProductTags.Any(pt => pt.ProductId == x.Id && pt.TagId == tag));

        return await queryable
            .OrderBy(x => x.Name)
            .Select(x => new Item(
                x.Id,
                x.Name,
                x.Barcode,
                db.ProductImages.Where(pi => pi.ProductId == x.Id).Select(pi => pi.Url).FirstOrDefault(),
                eventProductIds.Contains(x.Id)
                    ? db.ProductPrices.Where(p => p.ProductId == x.Id && p.PriceGroupId == eventPriceGroupId).Select(p => p.Gross).FirstOrDefault()
                    : db.ProductPrices.Where(p => p.ProductId == x.Id && p.PriceGroupId == defaultPriceGroupId).Select(p => p.Gross).FirstOrDefault(),
                eventProductIds.Contains(x.Id)
                    ? db.ProductPrices.Where(p => p.ProductId == x.Id && p.PriceGroupId == defaultPriceGroupId).Select(p => (decimal?)p.Gross).FirstOrDefault()
                    : null,
                eventProductIds.Contains(x.Id)))
            .ToListAsync();
    }
}
