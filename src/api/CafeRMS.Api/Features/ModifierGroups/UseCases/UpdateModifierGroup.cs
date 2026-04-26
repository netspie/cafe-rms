using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ModifierGroups.UseCases;

public static class UpdateModifierGroup
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var group = await db.ModifierGroups.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Modifier group not found.");

        var nameTaken = await db.ModifierGroups.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A modifier group named '{command.Name}' already exists.");

        group.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
