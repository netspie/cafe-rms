using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class AddProductImage
{
    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Guid productId, string url, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var image = ProductImage.Create(productId, url);
        db.ProductImages.Add(image);
        await db.SaveChangesAsync();
        return new Result(image.Id);
    }
}
