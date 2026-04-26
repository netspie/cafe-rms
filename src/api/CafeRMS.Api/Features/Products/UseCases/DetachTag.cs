using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

public static class DetachTag
{
    public static async Task Execute(Guid productId, Guid tagId, AppDbContext db)
    {
        var link = await db.ProductTags.FirstOrDefaultAsync(x => x.ProductId == productId && x.TagId == tagId)
            ?? throw new NotFoundException("Tag is not attached to this product.");

        db.ProductTags.Remove(link);
        await db.SaveChangesAsync();
    }
}
