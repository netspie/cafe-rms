using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class UpdateEvent
{
    public sealed record Command(
        Guid Id,
        string Name,
        string? Description,
        string? ImageUrl,
        Guid? ProductListId,
        Guid? PriceGroupId);

    public static async Task Execute(Command command, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == command.Id)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsClosed || ev.IsCancelled)
            throw new ConflictException("Cannot update a closed or cancelled event.");

        if (command.ProductListId is { } plId)
        {
            var exists = await db.ProductLists.AnyAsync(x => x.Id == plId);
            if (!exists)
                throw new NotFoundException("Product list not found.");
        }
        if (command.PriceGroupId is { } pgId)
        {
            var exists = await db.PriceGroups.AnyAsync(x => x.Id == pgId);
            if (!exists)
                throw new NotFoundException("Price group not found.");
        }

        ev.Update(command.Name, command.Description, command.ImageUrl, command.ProductListId, command.PriceGroupId);
        await db.SaveChangesAsync();
    }
}
