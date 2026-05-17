using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class DeleteRoleController : ControllerBase
{
    [HttpDelete("/api/roles/{id:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteRole.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteRole
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Role not found.");

        if (string.Equals(role.Name, SystemRoles.Owner, StringComparison.Ordinal))
            throw new ForbiddenException("The Owner role is system-managed and cannot be deleted.");

        db.Roles.Remove(role);
        await db.SaveChangesAsync();
    }
}
