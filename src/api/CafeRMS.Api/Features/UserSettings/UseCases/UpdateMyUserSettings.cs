using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.UserSettings.UseCases;

[ApiController]
public sealed class UpdateMyUserSettingsController : ControllerBase
{
    [HttpPut("/api/my/settings")]
    [Authorize(Policy = Policies.RequireGuest)]
    public async Task<IActionResult> Handle(
        [FromBody] UpdateMyUserSettingsRequest request,
        [FromServices] AppDbContext db)
    {
        await UpdateMyUserSettings.Execute(new UpdateMyUserSettings.Command(User.UserId, request.Theme, request.UiSettingsJson), db);
        return NoContent();
    }
}

public sealed record UpdateMyUserSettingsRequest(string Theme, string UiSettingsJson);

public sealed class UpdateMyUserSettingsValidator : AbstractValidator<UpdateMyUserSettingsRequest>
{
    public UpdateMyUserSettingsValidator()
    {
        RuleFor(x => x.Theme).NotEmpty().MaximumLength(50);
        RuleFor(x => x.UiSettingsJson).NotEmpty().MaximumLength(10000);
    }
}


public static class UpdateMyUserSettings
{
    public sealed record Command(Guid UserId, string Theme, string UiSettingsJson);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var existing = await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == command.UserId);
        if (existing is null)
        {
            db.UserSettings.Add(UserSettings.Create(command.UserId, command.Theme, command.UiSettingsJson));
        }
        else
        {
            existing.Update(command.Theme, command.UiSettingsJson);
        }
        await db.SaveChangesAsync();
    }
}
