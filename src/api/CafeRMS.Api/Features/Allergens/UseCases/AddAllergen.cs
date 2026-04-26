using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

public static class AddAllergen
{
    public sealed record Command(Guid CompanyId, string Name);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create an allergen.");

        var nameTaken = await db.Allergens.AnyAsync(x => x.Name == command.Name);
        if (nameTaken)
            throw new ConflictException($"An allergen named '{command.Name}' already exists.");

        var allergen = Allergen.Create(command.Name, command.CompanyId);
        db.Allergens.Add(allergen);
        await db.SaveChangesAsync();
        return new Result(allergen.Id);
    }
}
