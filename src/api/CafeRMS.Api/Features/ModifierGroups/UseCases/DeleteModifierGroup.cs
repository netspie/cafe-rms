using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

public static class DeleteModifierGroup
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var group = await db.ModifierGroups.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modifier group not found.");

        // Block-don't-cascade: refuse to delete a group that still owns Modifiers. The user
        // must delete the children first. Keeps deletion semantics explicit and easy to
        // explain in thesis defense (vs a recursive soft-delete with second-order side
        // effects). Modifier global filter automatically scopes to the current company.
        var hasChildren = await db.Modifiers.AnyAsync(x => x.ModifierGroupId == id);
        if (hasChildren)
            throw new ConflictException("Cannot delete a modifier group that still has modifiers. Delete the modifiers first.");

        db.ModifierGroups.Remove(group);
        await db.SaveChangesAsync();
    }
}
