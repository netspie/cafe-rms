using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class DeleteProductListController : ControllerBase
{
    [HttpDelete("/api/product-lists/{id:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteProductList.Execute(id, db);
        return NoContent();
    }
}


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
