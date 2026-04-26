using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

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
