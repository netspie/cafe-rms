using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

public static class RemoveProductFromList
{
    public static async Task Execute(Guid productListId, Guid productId, AppDbContext db)
    {
        var item = await db.ProductListItems
            .FirstOrDefaultAsync(x => x.ProductListId == productListId && x.ProductId == productId)
            ?? throw new NotFoundException("Product is not in this list.");

        db.ProductListItems.Remove(item);
        await db.SaveChangesAsync();
    }
}
