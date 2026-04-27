using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class AttachAllergenController : ControllerBase
{
    [HttpPost("/api/products/{id:guid}/allergens/{allergenId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid allergenId,
        [FromServices] AppDbContext db)
    {
        await AttachAllergen.Execute(id, allergenId, db);
        return NoContent();
    }
}


public static class AttachAllergen
{
    public static async Task Execute(Guid productId, Guid allergenId, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var allergenExists = await db.Allergens.AnyAsync(x => x.Id == allergenId);
        if (!allergenExists)
            throw new NotFoundException("Allergen not found.");

        var alreadyAttached = await db.ProductAllergens.AnyAsync(x => x.ProductId == productId && x.AllergenId == allergenId);
        if (alreadyAttached)
            return;

        db.ProductAllergens.Add(ProductAllergen.Create(productId, allergenId));
        await db.SaveChangesAsync();
    }
}
