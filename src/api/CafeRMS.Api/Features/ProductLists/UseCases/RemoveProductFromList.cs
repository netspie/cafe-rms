using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class RemoveProductFromListController : ControllerBase
{
    [HttpDelete("/api/product-lists/{id:guid}/items/{productId:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid productId,
        [FromServices] AppDbContext db)
    {
        await RemoveProductFromList.Execute(id, productId, db);
        return NoContent();
    }
}


public static class RemoveProductFromList
{
    public static async Task Execute(Guid productListId, Guid productId, AppDbContext db)
    {
        var item = await db.ProductListItems.FirstOrDefaultAsync(x => x.ProductListId == productListId && x.ProductId == productId)
            ?? throw new NotFoundException("Product is not in this list.");

        db.ProductListItems.Remove(item);
        await db.SaveChangesAsync();
    }
}
