using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class RemoveProductImage
{
    public static async Task Execute(Guid productId, Guid imageId, AppDbContext db)
    {
        var image = await db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId)
            ?? throw new NotFoundException("Image not found on this product.");

        db.ProductImages.Remove(image);
        await db.SaveChangesAsync();
    }
}
