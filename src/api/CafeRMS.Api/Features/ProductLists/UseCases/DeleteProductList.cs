using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

public static class DeleteProductList
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var list = await db.ProductLists.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Product list not found.");

        // Phase 2 join-table cleanup: items lose meaning once the list is gone.
        var items = await db.ProductListItems.Where(x => x.ProductListId == id).ToListAsync();
        db.ProductListItems.RemoveRange(items);

        db.ProductLists.Remove(list);
        await db.SaveChangesAsync();
    }
}
