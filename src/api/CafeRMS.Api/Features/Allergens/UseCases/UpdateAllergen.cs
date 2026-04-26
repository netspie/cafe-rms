using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Allergens.UseCases;

public static class UpdateAllergen
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var allergen = await db.Allergens.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Allergen not found.");

        var nameTaken = await db.Allergens.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"An allergen named '{command.Name}' already exists.");

        allergen.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
