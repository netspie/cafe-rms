using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

public static class ListRoles
{
    public sealed record Item(Guid Id, string Name, IReadOnlyList<string> Permissions);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db)
    {
        // db.Roles is auto-scoped to the current company by AppRole's ICompanyOwned filter.
        var roles = await db.Roles
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        var roleIds = roles.Select(x => x.Id).ToList();
        var claims = await db.RoleClaims
            .Where(x => roleIds.Contains(x.RoleId) && x.ClaimType == ClaimsPrincipalExtensions.PermissionClaim)
            .Select(x => new { x.RoleId, x.ClaimValue })
            .ToListAsync();

        return roles
            .Select(x => new Item(
                x.Id,
                x.Name ?? "",
                claims.Where(y => y.RoleId == x.Id).Select(y => y.ClaimValue ?? "").ToList()))
            .ToList();
    }
}
