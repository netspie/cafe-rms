using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class DeleteProductController : ControllerBase
{
    [HttpDelete("/api/products/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteProduct.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteProduct
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Product not found.");

        // Phase 2 join-table cleanup: a product on the way out drops every join /
        // child relationship in the same unit of work. Sub-entities (Image, Price)
        // and join rows (Tag, Allergen, ModifierGroup, ProductListItem) all lose
        // meaning once the parent product is gone.
        var tags = await db.ProductTags.Where(x => x.ProductId == id).ToListAsync();
        db.ProductTags.RemoveRange(tags);

        var allergens = await db.ProductAllergens.Where(x => x.ProductId == id).ToListAsync();
        db.ProductAllergens.RemoveRange(allergens);

        var modifierGroups = await db.ProductModifierGroups.Where(x => x.ProductId == id).ToListAsync();
        db.ProductModifierGroups.RemoveRange(modifierGroups);

        var images = await db.ProductImages.Where(x => x.ProductId == id).ToListAsync();
        db.ProductImages.RemoveRange(images);

        var prices = await db.ProductPrices.Where(x => x.ProductId == id).ToListAsync();
        db.ProductPrices.RemoveRange(prices);

        var listItems = await db.ProductListItems.Where(x => x.ProductId == id).ToListAsync();
        db.ProductListItems.RemoveRange(listItems);

        db.Products.Remove(product);
        await db.SaveChangesAsync();
    }
}
