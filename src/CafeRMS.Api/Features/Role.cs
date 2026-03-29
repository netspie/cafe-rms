namespace CafeRMS.Api.Features;

public class Role
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
}

public class RoleClaim
{
    public int Id { get; private init; }
    public int RoleId { get; set; }
    public string ClaimType { get; init; } = "";
    public string ClaimValue { get; init; } = "";
}
