using CafeRMS.Api.Shared.Errors;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class ChangePasswordController : ControllerBase
{
    [HttpPut("/api/auth/password")]
    public async Task<IActionResult> Handle(
        [FromBody] ChangePasswordRequest request,
        [FromServices] UserManager<AppUser> userManager)
    {
        var command = new ChangePassword.Command(User.UserId, request.CurrentPassword, request.NewPassword);
        await ChangePassword.Execute(command, userManager);
        return NoContent();
    }
}

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}


public static class ChangePassword
{
    public sealed record Command(Guid UserId, string CurrentPassword, string NewPassword);

    public static async Task Execute(Command command, UserManager<AppUser> userManager)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString())
            ?? throw new NotFoundException("User not found.");

        var result = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        if (!result.Succeeded)
            throw new DomainException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
