using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Identity;

namespace CafeRMS.Api.Features.Auth.UseCases;

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
