using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

public static class GetMyLoyaltyBalance
{
    public sealed record Result(int Balance);

    public static async Task<Result> Execute(Guid userId, AppDbContext db)
    {
        // LoyaltyPointLog is ICompanyOwned but Guests have no company context. Bypass the
        // filter — the per-user sum is the customer's total balance across every cafe.
        var balance = await db.LoyaltyPointLogs.IgnoreQueryFilters()
            .Where(x => x.UserId == userId)
            .SumAsync(x => (int?)x.Points) ?? 0;
        return new Result(balance);
    }
}
