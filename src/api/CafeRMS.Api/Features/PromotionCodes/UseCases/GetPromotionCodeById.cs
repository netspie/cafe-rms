using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

public static class GetPromotionCodeById
{
    public sealed record Result(
        Guid Id,
        string Code,
        decimal DiscountPercentage,
        DateTimeOffset? ValidFrom,
        DateTimeOffset? ValidUntil,
        int? MaxUses,
        int UsesCount,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var promo = await db.PromotionCodes
            .Where(x => x.Id == id)
            .Select(x => new Result(
                x.Id, x.Code, x.DiscountPercentage,
                x.ValidFrom, x.ValidUntil, x.MaxUses, x.UsesCount,
                x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Promotion code not found.");

        return promo;
    }
}
