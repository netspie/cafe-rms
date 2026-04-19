using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Shared;

namespace CafeRMS.Api.Features.UserSettings;

public class UserSettings : IAuditable
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public AppUser? User { get; private init; }
    public string Theme { get; private set; } = "";
    public string UiSettingsJson { get; private set; } = "{}";

    public DateTimeOffset CreatedAt { get; private init; }
    public Guid CreatedBy { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private UserSettings() { }

    public static UserSettings Create(Guid userId, Guid createdBy, string theme = "light", string uiSettingsJson = "{}")
    {
        return new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Theme = theme,
            UiSettingsJson = uiSettingsJson,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
