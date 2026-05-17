using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

[ApiController]
public sealed class DeleteModifierGroupController : ControllerBase
{
    [HttpDelete("/api/modifier-groups/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteModifierGroup.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteModifierGroup
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var group = await db.ModifierGroups.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modifier group not found.");

        var hasChildren = await db.Modifiers.AnyAsync(x => x.ModifierGroupId == id);
        if (hasChildren)
            throw new ConflictException("Cannot delete a modifier group that still has modifiers. Delete the modifiers first.");

        var links = await db.ProductModifierGroups.Where(x => x.ModifierGroupId == id).ToListAsync();
        db.ProductModifierGroups.RemoveRange(links);

        db.ModifierGroups.Remove(group);
        await db.SaveChangesAsync();
    }
}
