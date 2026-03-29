namespace CafeRMS.Api.Features;

public class User
{
    public int Id { get; private init; }
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}

public class UserSettings
{
    public int UserId { get; set; }
    public string Theme { get; init; } = "";
    public string UiSettingsJson { get; init; } = "";
}
