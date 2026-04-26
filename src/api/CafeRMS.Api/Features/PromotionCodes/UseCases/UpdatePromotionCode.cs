using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

public static class UpdatePromotionCode
{
    public sealed record Command(
        Guid Id,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Promotion code not found.");

        if (command.ValidFrom is { } from && command.ValidUntil is { } until && from > until)
            throw new DomainException("ValidFrom must be earlier than ValidUntil.");

        var codeTaken = await db.PromotionCodes.AnyAsync(x => x.Code == command.Code && x.Id != command.Id);
        if (codeTaken)
            throw new ConflictException($"A promotion code '{command.Code}' already exists.");

        promo.Update(command.Code, command.DiscountPercentage, command.ValidFrom, command.ValidUntil, command.MaxUses);
        await db.SaveChangesAsync();
    }
}
