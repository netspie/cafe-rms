using CafeRMS.Api.Features.Events;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Menu.UseCases;

[ApiController]
public sealed class GetMenuItemController : ControllerBase
{
    [HttpGet("/api/menu/{id:guid}")]
    [Authorize]
    public Task<GetMenuItem.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        GetMenuItem.Execute(id, db, DateTimeOffset.UtcNow);
}


public static class GetMenuItem
{
    public sealed record Result(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        bool IsEventPrice,
        IReadOnlyList<Guid> AllergenIds,
        IReadOnlyList<ImageInfo> Images);

    public sealed record ImageInfo(Guid Id, string Url);

    public static async Task<Result> Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Product not found.");

        var outlet = await db.Outlets
            .Select(x => new { x.DefaultPriceGroupId })
            .FirstOrDefaultAsync();

        var activeEvent = await ActiveEventResolver.ResolveAsync(db, now);
        var isEventPrice = activeEvent is not null && activeEvent.ProductIds.Contains(id);
        var priceGroupId = isEventPrice ? activeEvent!.PriceGroupId : outlet?.DefaultPriceGroupId;

        var price = await db.ProductPrices
            .Where(x => x.ProductId == id && x.PriceGroupId == priceGroupId)
            .Select(x => x.Gross)
            .FirstOrDefaultAsync();

        var allergenIds = await db.ProductAllergens.Where(x => x.ProductId == id).Select(x => x.AllergenId).ToListAsync();
        var images = await db.ProductImages.Where(x => x.ProductId == id).Select(x => new ImageInfo(x.Id, x.Url)).ToListAsync();

        return new Result(product.Id, product.Name, product.Description, price, isEventPrice, allergenIds, images);
    }
}
