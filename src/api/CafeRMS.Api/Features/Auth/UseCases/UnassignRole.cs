using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class UnassignRoleController : ControllerBase
{
    [HttpDelete("/api/users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid userId,
        [FromRoute] Guid roleId,
        [FromServices] AppDbContext db)
    {
        await UnassignRole.Execute(new UnassignRole.Command(userId, roleId), db);
        return NoContent();
    }
}


public static class UnassignRole
{
    public sealed record Command(Guid UserId, Guid RoleId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId)
            ?? throw new NotFoundException("Role not found.");

        var assignment = await db.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == command.UserId && x.RoleId == command.RoleId)
            ?? throw new NotFoundException("This user does not have that role.");

        // Owner orphan-prevention: cannot remove the last Owner.
        if (string.Equals(role.Name, SystemRoles.Owner, StringComparison.Ordinal))
        {
            var ownerCount = await db.UserRoles.CountAsync(x => x.RoleId == role.Id);
            if (ownerCount <= 1)
                throw new ConflictException("Cannot remove the last Owner.");
        }

        db.UserRoles.Remove(assignment);
        await db.SaveChangesAsync();
    }
}
