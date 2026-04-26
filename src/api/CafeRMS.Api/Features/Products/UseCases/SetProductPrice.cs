using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

// Upsert: a (Product, PriceGroup) pair has at most one Net price. Re-PUTting overwrites.
public static class SetProductPrice
{
    public static async Task Execute(Guid productId, Guid priceGroupId, decimal net, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var priceGroupExists = await db.PriceGroups.AnyAsync(x => x.Id == priceGroupId);
        if (!priceGroupExists)
            throw new NotFoundException("Price group not found.");

        var existing = await db.ProductPrices.FirstOrDefaultAsync(x => x.ProductId == productId && x.PriceGroupId == priceGroupId);
        if (existing is not null)
        {
            db.ProductPrices.Remove(existing);
            db.ProductPrices.Add(ProductPrice.Create(productId, priceGroupId, net));
        }
        else
        {
            db.ProductPrices.Add(ProductPrice.Create(productId, priceGroupId, net));
        }

        await db.SaveChangesAsync();
    }
}
