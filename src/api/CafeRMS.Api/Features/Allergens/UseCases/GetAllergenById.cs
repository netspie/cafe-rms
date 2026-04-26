using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

public static class GetAllergenById
{
    public sealed record Result(Guid Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var allergen = await db.Allergens
            .Where(x => x.Id == id)
            .Select(x => new Result(x.Id, x.Name, x.CreatedAt, x.UpdatedAt))
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Allergen not found.");

        return allergen;
    }
}
