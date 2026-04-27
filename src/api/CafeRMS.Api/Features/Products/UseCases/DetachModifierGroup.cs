using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class DetachModifierGroupController : ControllerBase
{
    [HttpDelete("/api/products/{id:guid}/modifier-groups/{modifierGroupId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid modifierGroupId,
        [FromServices] AppDbContext db)
    {
        await DetachModifierGroup.Execute(id, modifierGroupId, db);
        return NoContent();
    }
}


public static class DetachModifierGroup
{
    public static async Task Execute(Guid productId, Guid modifierGroupId, AppDbContext db)
    {
        var link = await db.ProductModifierGroups
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.ModifierGroupId == modifierGroupId)
            ?? throw new NotFoundException("Modifier group is not attached to this product.");

        db.ProductModifierGroups.Remove(link);
        await db.SaveChangesAsync();
    }
}
