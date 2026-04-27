using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class AddEvent
{
    public sealed record Command(
        Guid CompanyId,
        string Name,
        string? Description,
        string? ImageUrl,
        Guid? ProductListId,
        Guid? PriceGroupId);

    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Command command, AppDbContext db)
    {
        if (command.CompanyId == Guid.Empty)
            throw new ForbiddenException("A company context is required to create an event.");

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

        var ev = Event.Create(command.Name, command.CompanyId, command.Description, command.ImageUrl, command.ProductListId, command.PriceGroupId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return new Result(ev.Id);
    }
}
