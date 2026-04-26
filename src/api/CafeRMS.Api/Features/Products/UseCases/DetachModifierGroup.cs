using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class DetachModifierGroup
{
    public static async Task Execute(Guid productId, Guid modifierGroupId, AppDbContext db)
    {
        var link = await db.ProductModifierGroups
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ModifierGroupId == modifierGroupId)
            ?? throw new NotFoundException("Modifier group is not attached to this product.");

        db.ProductModifierGroups.Remove(link);
        await db.SaveChangesAsync();
    }
}
