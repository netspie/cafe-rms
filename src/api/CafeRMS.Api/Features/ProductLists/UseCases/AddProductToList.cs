using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

public static class AddProductToList
{
    public static async Task Execute(Guid productListId, Guid productId, AppDbContext db)
    {
        var listExists = await db.ProductLists.AnyAsync(x => x.Id == productListId);
        if (!listExists)
            throw new NotFoundException("Product list not found.");

        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        // Idempotent: already-in-list is a no-op so the mobile app can retry without error.
        var alreadyAdded = await db.ProductListItems
            .AnyAsync(x => x.ProductListId == productListId && x.ProductId == productId);
        if (alreadyAdded)
            return;

        db.ProductListItems.Add(ProductListItem.Create(productListId, productId));
        await db.SaveChangesAsync();
    }
}
