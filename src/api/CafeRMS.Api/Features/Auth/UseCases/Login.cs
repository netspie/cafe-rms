using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.UseCases;

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


public static class Login
{
    public sealed record Command(string Email, string Password);

    public sealed record Result(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    public static async Task<Result> Execute(
        Command command,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        const string invalid = "Invalid email or password.";

        var user = await userManager.FindByEmailAsync(command.Email)
            ?? throw new UnauthorizedAccessException(invalid);

        if (!await userManager.CheckPasswordAsync(user, command.Password))
            throw new UnauthorizedAccessException(invalid);

        var token = await tokenService.GenerateAsync(user);

        return new Result(token.AccessToken, token.ExpiresAt, user.AccountType.ToString());
    }
}
