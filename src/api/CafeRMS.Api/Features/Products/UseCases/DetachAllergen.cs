using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class DetachAllergen
{
    public static async Task Execute(Guid productId, Guid allergenId, AppDbContext db)
    {
        var link = await db.ProductAllergens.FirstOrDefaultAsync(x => x.ProductId == productId && x.AllergenId == allergenId)
            ?? throw new NotFoundException("Allergen is not attached to this product.");

        db.ProductAllergens.Remove(link);
        await db.SaveChangesAsync();
    }
}
