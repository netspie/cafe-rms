using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.UserSettings.UseCases;

public static class GetMyUserSettings
{
    public sealed record Result(string Theme, string UiSettingsJson);

    public static async Task<Result> Execute(Guid userId, AppDbContext db)
    {
        var existing = await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId);
        if (existing is null)
        {
            // Auto-create with defaults so the customer always sees a valid settings object.
            existing = UserSettings.Create(userId);
            db.UserSettings.Add(existing);
            await db.SaveChangesAsync();
        }
        return new Result(existing.Theme, existing.UiSettingsJson);
    }
}
