using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class DeleteRole
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Role not found.");

        if (string.Equals(role.Name, SystemRoles.Owner, StringComparison.Ordinal))
            throw new ForbiddenException("The Owner role is system-managed and cannot be deleted.");

        // SoftDeletableSaveChangesInterceptor converts Remove → soft-delete.
        db.Roles.Remove(role);
        await db.SaveChangesAsync();
    }
}
