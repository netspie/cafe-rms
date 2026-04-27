using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Auth.UseCases;

[ApiController]
public sealed class ListUsersController : ControllerBase
{
    [HttpGet("/api/users")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IReadOnlyList<ListUsers.Item>> Handle([FromServices] AppDbContext db) =>
        await ListUsers.Execute(db.CurrentCompanyId, db);
}


public static class ListUsers
{
    public sealed record Item(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        IReadOnlyList<string> Roles);

    public static async Task<IReadOnlyList<Item>> Execute(Guid companyId, AppDbContext db)
    {
        var users = await db.Users
            .Where(x => x.AccountType == AccountType.Staff && x.CompanyId == companyId)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new { x.Id, x.Email, x.FirstName, x.LastName })
            .ToListAsync();

        var userIds = users.Select(x => x.Id).ToList();
        var assignments = await db.UserRoles
            .Where(x => userIds.Contains(x.UserId))
            .Join(
                db.Roles,
                x => x.RoleId,
                x => x.Id,
                (x, y) => new { x.UserId, RoleName = y.Name ?? "" })
            .ToListAsync();

        return users
            .Select(x => new Item(
                x.Id,
                x.Email ?? "",
                x.FirstName,
                x.LastName,
                assignments.Where(y => y.UserId == x.Id).Select(y => y.RoleName).ToList()))
            .ToList();
    }
}
