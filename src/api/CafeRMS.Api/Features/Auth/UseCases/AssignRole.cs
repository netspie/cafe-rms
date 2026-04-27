using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class AssignRoleController : ControllerBase
{
    [HttpPost("/api/users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid userId,
        [FromRoute] Guid roleId,
        [FromServices] AppDbContext db)
    {
        await AssignRole.Execute(new AssignRole.Command(db.CurrentCompanyId, userId, roleId), db);
        return NoContent();
    }
}


public static class AssignRole
{
    public sealed record Command(Guid CompanyId, Guid UserId, Guid RoleId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required.");

        // Role must be in the current company (filter ensures this; else NotFound).
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId)
            ?? throw new NotFoundException("Role not found in the current company.");

        // User must be a Staff user in the current company.
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == command.UserId)
            ?? throw new NotFoundException("User not found.");
        if (user.AccountType != AccountType.Staff || user.CompanyId != command.CompanyId)
            throw new ForbiddenException("User is not a staff member of the current company.");

        var alreadyAssigned = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
        if (alreadyAssigned)
            return;

        db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });
        await db.SaveChangesAsync();
    }
}
