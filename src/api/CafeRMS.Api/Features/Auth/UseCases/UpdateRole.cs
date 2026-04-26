using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class UpdateRole
{
    public sealed record Command(Guid Id, string Name, IReadOnlyList<string> Permissions);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Role not found.");

        if (string.Equals(role.Name, SystemRoles.Owner, StringComparison.Ordinal))
            throw new ForbiddenException("The Owner role is system-managed and cannot be modified.");

        if (string.Equals(command.Name, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Cannot rename a role to 'Owner' — that name is reserved.");

        var normalized = command.Name.ToUpperInvariant();
        var nameTaken = await db.Roles.AnyAsync(x => x.NormalizedName == normalized && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A role named '{command.Name}' already exists.");

        role.Name = command.Name;
        role.NormalizedName = normalized;

        // Replace claims wholesale.
        var existingClaims = await db.RoleClaims
            .Where(x => x.RoleId == role.Id && x.ClaimType == ClaimsPrincipalExtensions.PermissionClaim)
            .ToListAsync();
        db.RoleClaims.RemoveRange(existingClaims);

        foreach (var permission in command.Permissions)
        {
            db.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = ClaimsPrincipalExtensions.PermissionClaim,
                ClaimValue = permission
            });
        }

        await db.SaveChangesAsync();
    }
}
