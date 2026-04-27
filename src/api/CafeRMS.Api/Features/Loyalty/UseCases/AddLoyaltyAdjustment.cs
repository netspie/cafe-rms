using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Loyalty.UseCases;

public static class AddLoyaltyAdjustment
{
    public sealed record Command(Guid CompanyId, Guid UserId, int Points, string Reason);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required for a loyalty adjustment.");

        if (command.Points == 0)
            throw new DomainException("Points must be non-zero.");

        var userExists = await db.Users.AnyAsync(x => x.Id == command.UserId);
        if (!userExists)
            throw new NotFoundException("User not found.");

        var entry = LoyaltyPointLog.Create(command.UserId, command.Points, command.CompanyId, command.Reason);
        db.LoyaltyPointLogs.Add(entry);
        await db.SaveChangesAsync();
        return new Result(entry.Id);
    }
}
