using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class GetMeController : ControllerBase
{
    [HttpGet("/api/me")]
    [Authorize]
    public async Task<GetMe.Result> Handle(
        [FromServices] UserManager<AppUser> userManager,
        [FromServices] AppDbContext db) =>
        await GetMe.Execute(User.UserId, userManager, db);
}


public static class GetMe
{
    public sealed record Result(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        AccountType AccountType,
        Guid? OutletId,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions);

    public static async Task<Result> Execute(Guid userId, UserManager<AppUser> userManager, AppDbContext db)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await userManager.GetRolesAsync(user);

        var outletId = await db.Outlets.Select(x => (Guid?)x.Id).FirstOrDefaultAsync();

        var roleIds = await db.Roles
            .Where(r => roles.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var permissions = await db.RoleClaims
            .Where(c => roleIds.Contains(c.RoleId) && c.ClaimType == ClaimsPrincipalExtensions.PermissionClaim)
            .Select(c => c.ClaimValue ?? "")
            .Distinct()
            .ToListAsync();

        return new Result(
            user.Id,
            user.Email ?? "",
            user.FirstName,
            user.LastName,
            user.AccountType,
            outletId,
            roles.ToList(),
            permissions);
    }
}
