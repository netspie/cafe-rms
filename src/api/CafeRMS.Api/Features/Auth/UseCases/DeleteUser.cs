using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class DeleteUser
{
    public sealed record Command(Guid CompanyId, Guid ActingUserId, Guid TargetUserId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        if (command.TargetUserId == command.ActingUserId)
            throw new ConflictException("You cannot delete your own account.");

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == command.TargetUserId)
            ?? throw new NotFoundException("User not found.");

        if (user.AccountType != AccountType.Staff || user.CompanyId != command.CompanyId)
            throw new ForbiddenException("User is not a staff member of the current company.");

        // Last-Owner orphan-prevention: if target holds the Owner role, ensure another Owner remains.
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
                    throw new ConflictException("Cannot delete the last Owner of the company.");
            }
        }

        // SoftDeletableSaveChangesInterceptor converts Remove → soft-delete.
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
}
