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
public sealed class CreateRoleController : ControllerBase
{
    [HttpPost("/api/roles")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<CreateRoleResponse> Handle(
        [FromBody] CreateRoleRequest request,
        [FromServices] AppDbContext db)
    {
        var command = new CreateRole.Command(db.CurrentCompanyId, request.Name, request.Permissions ?? []);
        var result = await CreateRole.Execute(command, db);
        return new CreateRoleResponse(result.RoleId);
    }
}

public sealed record CreateRoleRequest(string Name, IReadOnlyList<string>? Permissions);

public sealed record CreateRoleResponse(Guid RoleId);

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}


public static class CreateRole
{
    public sealed record Command(Guid CompanyId, string Name, IReadOnlyList<string> Permissions);

    public sealed record Result(Guid RoleId);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a role.");

        if (string.Equals(command.Name, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("The Owner role is system-managed and cannot be created.");

        var normalized = command.Name.ToUpperInvariant();
        var nameTaken = await db.Roles.AnyAsync(x => x.NormalizedName == normalized);
        if (nameTaken)
            throw new ConflictException($"A role named '{command.Name}' already exists.");

        var role = AppRole.Create(command.Name, command.CompanyId);
        db.Roles.Add(role);

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
        return new Result(role.Id);
    }
}
