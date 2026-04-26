using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Modifiers.UseCases;

public static class UpdateModifier
{
    public sealed record Command(Guid Id, string Name, decimal PriceDelta);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var modifier = await db.Modifiers.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Modifier not found.");

        var nameTaken = await db.Modifiers.AnyAsync(x =>
            x.ModifierGroupId == modifier.ModifierGroupId &&
            x.Name == command.Name &&
            x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A modifier named '{command.Name}' already exists in this group.");

        modifier.Update(command.Name, command.PriceDelta);
        await db.SaveChangesAsync();
    }
}
