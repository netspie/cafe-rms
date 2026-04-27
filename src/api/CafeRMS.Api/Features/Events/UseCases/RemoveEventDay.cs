using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class RemoveEventDay
{
    public static async Task Execute(Guid eventId, Guid dayId, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == eventId)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsClosed || ev.IsCancelled)
            throw new ConflictException("Cannot remove days from a closed or cancelled event.");

        var day = await db.EventDays.FirstOrDefaultAsync(x => x.Id == dayId && x.EventId == eventId)
            ?? throw new NotFoundException("Event day not found.");

        db.EventDays.Remove(day);
        await db.SaveChangesAsync();
    }
}
