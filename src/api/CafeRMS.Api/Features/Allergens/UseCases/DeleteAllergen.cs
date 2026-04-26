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

        db.Allergens.Remove(allergen);
        await db.SaveChangesAsync();
    }
}
