using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class UpdateRoleController : ControllerBase
{
    [HttpPut("/api/roles/{id:guid}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateRoleRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new UpdateRole.Command(id, request.Name, request.Permissions ?? []);
        await UpdateRole.Execute(command, db);
        return NoContent();
    }
}

public sealed record UpdateRoleRequest(string Name, IReadOnlyList<string>? Permissions);

public sealed class UpdateRoleValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}


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
