using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class GetProductByIdController : ControllerBase
{
    [HttpGet("/api/products/{id:guid}")]
    [Authorize]
    public async Task<GetProductById.Result> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db) =>
        await GetProductById.Execute(id, db);
}


public static class GetProductById
{
    public sealed record Result(
        Guid Id,
        string Name,
        string? Description,
        string? Barcode,
        Guid TaxRateId,
        IReadOnlyList<Guid> TagIds,
        IReadOnlyList<Guid> AllergenIds,
        IReadOnlyList<Guid> ModifierGroupIds,
        IReadOnlyList<ImageInfo> Images,
        IReadOnlyList<PriceInfo> Prices,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public sealed record ImageInfo(Guid Id, string Url);

    public sealed record PriceInfo(Guid PriceGroupId, decimal Net);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Product not found.");

        var tagIds = await db.ProductTags.Where(x => x.ProductId == id).Select(x => x.TagId).ToListAsync();
        var allergenIds = await db.ProductAllergens.Where(x => x.ProductId == id).Select(x => x.AllergenId).ToListAsync();
        var modifierGroupIds = await db.ProductModifierGroups.Where(x => x.ProductId == id).Select(x => x.ModifierGroupId).ToListAsync();
        var images = await db.ProductImages.Where(x => x.ProductId == id).Select(x => new ImageInfo(x.Id, x.Url)).ToListAsync();
        var prices = await db.ProductPrices.Where(x => x.ProductId == id).Select(x => new PriceInfo(x.PriceGroupId, x.Net)).ToListAsync();

        return new Result(
            product.Id, product.Name, product.Description, product.Barcode, product.TaxRateId,
            tagIds, allergenIds, modifierGroupIds, images, prices,
            product.CreatedAt, product.UpdatedAt);
    }
}
