using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.UserSettings.UseCases;

[ApiController]
public sealed class GetMyUserSettingsController : ControllerBase
{
    [HttpGet("/api/my/settings")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<GetMyUserSettings.Result> Handle(
        [FromServices] AppDbContext db) =>
        GetMyUserSettings.Execute(User.UserId, db);
}


public static class GetMyUserSettings
{
    public sealed record Result(string Theme, string UiSettingsJson);

    public static async Task<Result> Execute(Guid userId, AppDbContext db)
    {
        var existing = await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId);
        if (existing is null)
        {
            existing = UserSettings.Create(userId);
            db.UserSettings.Add(existing);
            await db.SaveChangesAsync();
        }
        return new Result(existing.Theme, existing.UiSettingsJson);
    }
}
