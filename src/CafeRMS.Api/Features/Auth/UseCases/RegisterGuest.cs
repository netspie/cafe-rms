using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth.UseCases;

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
