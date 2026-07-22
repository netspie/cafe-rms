using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

[ApiController]
public sealed class ListLoyaltyCustomersController : ControllerBase
{
    [HttpGet("/api/loyalty/customers")]
    [Authorize(Policy = Permissions.LoyaltyManage)]
    public Task<IReadOnlyList<ListLoyaltyCustomers.Item>> Handle(
        [FromServices] AppDbContext db) =>
        ListLoyaltyCustomers.Execute(db);
}


public static class ListLoyaltyCustomers
{
    public sealed record Item(Guid Id, string FirstName, string LastName, string Email, int Balance);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db)
    {
        var users = await db.Users
            .Where(x => x.AccountType == AccountType.Guest)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new { x.Id, x.FirstName, x.LastName, Email = x.Email ?? "" })
            .ToListAsync();

        var userIds = users.Select(x => x.Id).ToList();
        var balances = await db.LoyaltyPointLogs
            .Where(x => userIds.Contains(x.UserId))
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Points = g.Sum(e => e.Points) })
            .ToListAsync();

        return users
            .Select(x => new Item(
                x.Id, x.FirstName, x.LastName, x.Email,
                balances.FirstOrDefault(b => b.UserId == x.Id)?.Points ?? 0))
            .ToList();
    }
}
