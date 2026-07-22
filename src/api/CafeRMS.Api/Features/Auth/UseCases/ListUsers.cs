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
    public async Task<IReadOnlyList<ListUsers.Item>> Handle(
        [FromQuery] ListUsersRequest request,
        [FromServices] AppDbContext db) =>
        await ListUsers.Execute(request.AccountType, request.Q, request.Role, db);
}

public sealed record ListUsersRequest(string? AccountType = null, string? Q = null, string? Role = null);


public static class ListUsers
{
    public sealed record Item(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string AccountType,
        int LoyaltyPoints,
        IReadOnlyList<string> Roles);

    public static async Task<IReadOnlyList<Item>> Execute(
        string? accountTypeFilter,
        string? nameFilter,
        string? roleFilter,
        AppDbContext db)
    {
        var requestedAccountType = accountTypeFilter switch
        {
            "Guest" => AccountType.Guest,
            _ => AccountType.Staff
        };

        var queryable = db.Users.AsQueryable()
            .Where(x => x.AccountType == requestedAccountType);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var needle = nameFilter.ToLower();
            queryable = queryable.Where(x =>
                x.FirstName.ToLower().Contains(needle) ||
                x.LastName.ToLower().Contains(needle));
        }

        var users = await queryable
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new { x.Id, x.Email, x.FirstName, x.LastName, x.AccountType })
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

        var balances = await db.LoyaltyPointLogs
            .Where(x => userIds.Contains(x.UserId))
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Points = g.Sum(e => e.Points) })
            .ToListAsync();

        var items = users
            .Select(x => new Item(
                x.Id,
                x.Email ?? "",
                x.FirstName,
                x.LastName,
                x.AccountType.ToString(),
                balances.FirstOrDefault(y => y.UserId == x.Id)?.Points ?? 0,
                assignments.Where(y => y.UserId == x.Id).Select(y => y.RoleName).ToList()))
            .ToList();

        if (!string.IsNullOrWhiteSpace(roleFilter))
            items = [.. items.Where(x => x.Roles.Contains(roleFilter))];

        return items;
    }
}
