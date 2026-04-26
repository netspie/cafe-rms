using CafeRMS.Api.Features.Auth.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class RegisterGuestController : ControllerBase
{
    [HttpPost("/api/auth/register/guest")]
    [AllowAnonymous]
    public async Task<RegisterGuestResponse> Handle(
        [FromBody] RegisterGuestRequest request,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] JwtTokenService tokenService)
    {
        var command = new RegisterGuest.Command(request.Email, request.Password, request.FirstName, request.LastName);
        var result = await RegisterGuest.Execute(command, userManager, tokenService);
        return new RegisterGuestResponse(result.AccessToken, result.ExpiresAt, result.AccountType);
    }
}

public sealed record RegisterGuestRequest(string Email, string Password, string FirstName, string LastName);

public sealed record RegisterGuestResponse(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

public sealed class RegisterGuestValidator : AbstractValidator<RegisterGuestRequest>
{
    public RegisterGuestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
