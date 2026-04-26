using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

public static class DeleteAllergen
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var allergen = await db.Allergens.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Allergen not found.");

        // Phase 2 join-table cleanup: ProductAllergen rows lose meaning once the Allergen is gone.
        var links = await db.ProductAllergens.Where(x => x.AllergenId == id).ToListAsync();
        db.ProductAllergens.RemoveRange(links);

        db.Allergens.Remove(allergen);
        await db.SaveChangesAsync();
    }
}
