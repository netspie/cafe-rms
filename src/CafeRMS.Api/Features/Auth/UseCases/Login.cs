using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth.UseCases;

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
