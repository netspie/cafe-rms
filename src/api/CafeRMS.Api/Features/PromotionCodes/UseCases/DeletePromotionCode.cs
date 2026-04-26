using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

public static class DeletePromotionCode
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Promotion code not found.");

        db.PromotionCodes.Remove(promo);
        await db.SaveChangesAsync();
    }
}
