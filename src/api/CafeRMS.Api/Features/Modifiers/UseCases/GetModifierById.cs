using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

public static class GetModifierById
{
    public sealed record Result(Guid Id, string Name, decimal PriceDelta, Guid ModifierGroupId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var modifier = await db.Modifiers
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.PriceDelta, x.ModifierGroupId, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Modifier not found.");

        return modifier;
    }
}
