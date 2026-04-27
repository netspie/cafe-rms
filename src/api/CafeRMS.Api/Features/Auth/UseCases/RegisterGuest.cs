using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Auth.UseCases;

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


public static class RegisterGuest
{
    public sealed record Command(string Email, string Password, string FirstName, string LastName);

    public sealed record Result(string AccessToken, DateTimeOffset ExpiresAt, string AccountType);

    public static async Task<Result> Execute(
        Command command,
        UserManager<AppUser> userManager,
        JwtTokenService tokenService)
    {
        var existing = await userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            throw new ConflictException("Email already in use.");

        var user = AppUser.Create(command.Email, command.FirstName, command.LastName, AccountType.Guest);
        var createResult = await userManager.CreateAsync(user, command.Password);
        if (!createResult.Succeeded)
            throw new DomainException(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var token = await tokenService.GenerateAsync(user);
        return new Result(token.AccessToken, token.ExpiresAt, user.AccountType.ToString());
    }
}
