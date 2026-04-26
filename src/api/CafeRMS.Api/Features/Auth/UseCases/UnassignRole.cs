using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class UnassignRole
{
    public sealed record Command(Guid CompanyId, Guid UserId, Guid RoleId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required.");

        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == command.RoleId)
            ?? throw new NotFoundException("Role not found in the current company.");

        var assignment = await db.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == command.UserId && x.RoleId == command.RoleId)
            ?? throw new NotFoundException("This user does not have that role.");

        // Owner orphan-prevention: cannot remove the last Owner from the company.
        if (string.Equals(role.Name, SystemRoles.Owner, StringComparison.Ordinal))
        {
            var ownerCount = await db.UserRoles.CountAsync(x => x.RoleId == role.Id);
            if (ownerCount <= 1)
                throw new ConflictException("Cannot remove the last Owner from the company.");
        }

        db.UserRoles.Remove(assignment);
        await db.SaveChangesAsync();
    }
}
