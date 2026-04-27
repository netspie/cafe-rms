using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

[ApiController]
public sealed class DeleteModifierController : ControllerBase
{
    [HttpDelete("/api/modifiers/{id:guid}")]
    [Authorize(Policy = Permissions.ModifiersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteModifier.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteModifier
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var modifier = await db.Modifiers.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Modifier not found.");

        db.Modifiers.Remove(modifier);
        await db.SaveChangesAsync();
    }
}
