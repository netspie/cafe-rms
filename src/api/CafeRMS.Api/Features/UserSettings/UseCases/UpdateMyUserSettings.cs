using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.UserSettings.UseCases;

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
