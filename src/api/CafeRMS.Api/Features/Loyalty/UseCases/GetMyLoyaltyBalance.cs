using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

[ApiController]
public sealed class GetMyLoyaltyBalanceController : ControllerBase
{
    [HttpGet("/api/my/loyalty/balance")]
    [Authorize(Policy = Policies.RequireGuest)]
    public Task<GetMyLoyaltyBalance.Result> Handle(
        [FromServices] AppDbContext db) =>
        GetMyLoyaltyBalance.Execute(User.UserId, db);
}


public static class GetMyLoyaltyBalance
{
    public sealed record Result(int Balance);

    public static async Task<Result> Execute(Guid userId, AppDbContext db)
    {
        var balance = await db.Database
            .SqlQuery<int>($"SELECT func_loyalty_balance({userId}) AS \"Value\"")
            .SingleAsync();
        return new Result(balance);
    }
}
