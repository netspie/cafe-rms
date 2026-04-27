using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class RegisterStaffController : ControllerBase
{
    [HttpPost("/api/auth/register/staff")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<RegisterStaffResponse> Handle(
        [FromBody] RegisterStaffRequest request,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] RoleManager<AppRole> roleManager,
        [FromServices] AppDbContext db)
    {
        var command = new RegisterStaff.Command(
            db.CurrentCompanyId,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Roles ?? []);

        var result = await RegisterStaff.Execute(command, userManager, roleManager, db);

        return new RegisterStaffResponse(result.UserId, result.Email, result.AccountType, result.Roles);
    }
}

public sealed record RegisterStaffRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IReadOnlyList<string>? Roles);

public sealed record RegisterStaffResponse(
    Guid UserId,
    string Email,
    string AccountType,
    IReadOnlyList<string> Roles);

public sealed class RegisterStaffValidator : AbstractValidator<RegisterStaffRequest>
{
    public RegisterStaffValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}


public static class RegisterStaff
{
    public sealed record Command(
        Guid CompanyId,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        IReadOnlyList<string> Roles);

    public sealed record Result(
        Guid UserId,
        string Email,
        string AccountType,
        IReadOnlyList<string> Roles);

    public static async Task<Result> Execute(
        Command command,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to register staff.");

        // Owner role is assigned only via POST /api/users/{userId}/roles/{roleId}
        // (gated by Permissions.RolesManage). Block it here so a UsersManage holder
        // can't promote themselves or anyone else to Owner via the registration endpoint.
        if (command.Roles.Any(r => string.Equals(r, SystemRoles.Owner, StringComparison.OrdinalIgnoreCase)))
            throw new ForbiddenException("The Owner role cannot be assigned through registration.");

        foreach (var roleName in command.Roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
                throw new NotFoundException($"Role '{roleName}' not found in the current company.");
        }

        var existing = await userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            throw new ConflictException("Email already in use.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var user = AppUser.Create(
            command.Email,
            command.FirstName,
            command.LastName,
            AccountType.Staff,
            command.CompanyId);

        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        foreach (var roleName in command.Roles)
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
                throw new DomainException(string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
        }

        await tx.CommitAsync();

        return new Result(user.Id, user.Email!, user.AccountType.ToString(), command.Roles);
    }
}
