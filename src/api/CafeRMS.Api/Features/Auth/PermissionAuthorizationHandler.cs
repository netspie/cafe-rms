using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;

namespace CafeRMS.Api.Features.Auth;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;

        if (user.IsInRole(SystemRoles.Owner) ||
            user.HasPermission(requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
