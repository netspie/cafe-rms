using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class AttachTag
{
    public static async Task Execute(Guid productId, Guid tagId, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var tagExists = await db.Tags.AnyAsync(x => x.Id == tagId);
        if (!tagExists)
            throw new NotFoundException("Tag not found.");

        var alreadyAttached = await db.ProductTags.AnyAsync(x => x.ProductId == productId && x.TagId == tagId);
        if (alreadyAttached)
            return;

        db.ProductTags.Add(ProductTag.Create(productId, tagId));
        await db.SaveChangesAsync();
    }
}
