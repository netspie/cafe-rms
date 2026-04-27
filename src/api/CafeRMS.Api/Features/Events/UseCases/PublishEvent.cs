using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class PublishEvent
{
    public static async Task Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsCancelled || ev.IsClosed)
            throw new ConflictException("Cannot publish a closed or cancelled event.");
        if (ev.IsPublished)
            throw new ConflictException("Event is already published.");

        var hasDays = await db.EventDays.AnyAsync(x => x.EventId == id);
        if (!hasDays)
            throw new ConflictException("Cannot publish an event with no days.");

        ev.Publish(now);
        await db.SaveChangesAsync();
    }
}
