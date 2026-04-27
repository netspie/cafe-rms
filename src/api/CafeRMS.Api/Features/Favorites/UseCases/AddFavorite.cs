using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Favorites.UseCases;

public static class AddFavorite
{
    public static async Task Execute(Guid userId, Guid productId, AppDbContext db)
    {
        // Bypass the Product ICompanyOwned filter: Guests have no company in their JWT,
        // and a favorite is just a UserId+ProductId pair anyway. We still need to confirm
        // the product exists.
        var productExists = await db.Products.IgnoreQueryFilters().AnyAsync(x => x.Id == productId && x.DeletedAt == null);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var alreadyFavorited = await db.Favorites.AnyAsync(x => x.UserId == userId && x.ProductId == productId);
        if (alreadyFavorited)
            return;

        db.Favorites.Add(Favorite.Create(userId, productId));
        await db.SaveChangesAsync();
    }
}
