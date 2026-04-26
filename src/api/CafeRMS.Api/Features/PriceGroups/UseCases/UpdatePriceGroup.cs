using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

public static class UpdatePriceGroup
{
    public sealed record Command(Guid Id, string Name);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var priceGroup = await db.PriceGroups.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Price group not found.");

        var nameTaken = await db.PriceGroups.AnyAsync(x => x.Name == command.Name && x.Id != command.Id);
        if (nameTaken)
            throw new ConflictException($"A price group named '{command.Name}' already exists.");

        priceGroup.Update(command.Name);
        await db.SaveChangesAsync();
    }
}
