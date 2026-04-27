using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class AttachModifierGroupController : ControllerBase
{
    [HttpPost("/api/products/{id:guid}/modifier-groups/{modifierGroupId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid modifierGroupId,
        [FromServices] AppDbContext db)
    {
        await AttachModifierGroup.Execute(id, modifierGroupId, db);
        return NoContent();
    }
}


public static class AttachModifierGroup
{
    public static async Task Execute(Guid productId, Guid modifierGroupId, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var groupExists = await db.ModifierGroups.AnyAsync(x => x.Id == modifierGroupId);
        if (!groupExists)
            throw new NotFoundException("Modifier group not found.");

        var alreadyAttached = await db.ProductModifierGroups.AnyAsync(x =>
            x.ProductId == productId && x.ModifierGroupId == modifierGroupId);
        if (alreadyAttached)
            return;

        db.ProductModifierGroups.Add(ProductModifierGroup.Create(productId, modifierGroupId));
        await db.SaveChangesAsync();
    }
}
