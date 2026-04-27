using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

public static class RemoveFavorite
{
    public static async Task Execute(Guid userId, Guid productId, AppDbContext db)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId)
            ?? throw new NotFoundException("Favorite not found.");

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync();
    }
}
