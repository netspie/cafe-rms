using CafeRMS.Api.Features.Auth.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.Controllers;

[ApiController]
public sealed class LoginController : ControllerBase
{
    [HttpPost("/api/auth/login")]
    [AllowAnonymous]
    public async Task<LoginResponse> Handle(
        [FromBody] LoginRequest request,
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] JwtTokenService tokenService)
    {
        var command = new Login.Command(request.Email, request.Password);
        var result = await Login.Execute(command, userManager, tokenService);
        return new LoginResponse(result.AccessToken, result.ExpiresAt, result.AccountType);
    }
}

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
