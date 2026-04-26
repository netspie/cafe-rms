using CafeRMS.Api.Features.Auth.UseCases;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

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
