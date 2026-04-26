namespace CafeRMS.Api.Features.Auth;

public static class Policies
{
    public const string RequireGuest = "RequireGuest";
    public const string RequireStaff = "RequireStaff";
    public const string RequireSuperAdmin = "RequireSuperAdmin";
    public const string RequireStaffOrSuperAdmin = "RequireStaffOrSuperAdmin";
}
