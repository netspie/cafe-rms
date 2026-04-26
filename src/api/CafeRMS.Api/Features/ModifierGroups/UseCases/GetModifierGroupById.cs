using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

public static class GetModifierGroupById
{
    public sealed record Result(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var group = await db.ModifierGroups
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Modifier group not found.");

        return group;
    }
}
