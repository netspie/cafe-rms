using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class DeleteUserController : ControllerBase
{
    [HttpDelete("/api/users/{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        var command = new DeleteUser.Command(User.UserId, id);
        await DeleteUser.Execute(command, db);
        return NoContent();
    }
}


public static class DeleteUser
{
    public sealed record Command(Guid ActingUserId, Guid TargetUserId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        if (command.TargetUserId == command.ActingUserId)
            throw new ConflictException("You cannot delete your own account.");

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == command.TargetUserId)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountType != AccountType.Staff)
            throw new ForbiddenException("Only staff users can be deleted via this endpoint.");

        var ownerRoleId = await db.Roles
            .Where(x => x.NormalizedName == SystemRoles.Owner.ToUpperInvariant())
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (ownerRoleId != Guid.Empty)
        {
            var holdsOwner = await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == ownerRoleId);
            if (holdsOwner)
            {
                var ownerCount = await db.UserRoles.CountAsync(x => x.RoleId == ownerRoleId);
                if (ownerCount <= 1)
                    throw new ConflictException("Cannot delete the last Owner.");
            }
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
}
