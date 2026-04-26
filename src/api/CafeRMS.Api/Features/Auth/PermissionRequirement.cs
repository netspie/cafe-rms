using Microsoft.AspNetCore.Authorization;

namespace CafeRMS.Api.Features.Auth;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
