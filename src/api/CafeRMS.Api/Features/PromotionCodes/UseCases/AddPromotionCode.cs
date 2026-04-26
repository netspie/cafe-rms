using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

public static class AddPromotionCode
{
    public sealed record Command(
        Guid CompanyId,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create a promotion code.");

        if (command.ValidFrom is { } from && command.ValidUntil is { } until && from > until)
            throw new DomainException("ValidFrom must be earlier than ValidUntil.");

        var codeTaken = await db.PromotionCodes.AnyAsync(x => x.Code == command.Code);
        if (codeTaken)
            throw new ConflictException($"A promotion code '{command.Code}' already exists.");

        var promo = PromotionCode.Create(
            command.Code,
            command.DiscountPercentage,
            command.CompanyId,
            command.ValidFrom,
            command.ValidUntil,
            command.MaxUses);

        db.PromotionCodes.Add(promo);
        await db.SaveChangesAsync();
        return new Result(promo.Id);
    }
}
