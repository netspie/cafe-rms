using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

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
