using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class CancelEvent
{
    public static async Task Execute(Guid id, string? reason, AppDbContext db, DateTimeOffset now)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsCancelled)
            throw new ConflictException("Event is already cancelled.");
        if (ev.IsClosed)
            throw new ConflictException("Cannot cancel a closed event.");

        ev.Cancel(now, reason);
        await db.SaveChangesAsync();
    }
}
