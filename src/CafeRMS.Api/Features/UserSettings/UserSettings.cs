using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Shared.Entities;

namespace CafeRMS.Api.Features.UserSettings;

public class UserSettings : Entity
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public AppUser? User { get; private init; }
    public string Theme { get; private set; } = "";
    public string UiSettingsJson { get; private set; } = "{}";

    private UserSettings() { }

    public static UserSettings Create(Guid userId, string theme = "light", string uiSettingsJson = "{}")
    {
        return new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Theme = theme,
            UiSettingsJson = uiSettingsJson
        };
    }
}
