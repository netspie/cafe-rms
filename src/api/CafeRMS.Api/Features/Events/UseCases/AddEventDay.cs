using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class AddEventDay
{
    public sealed record Result(Guid Id);

    public static async Task<Result> Execute(Guid eventId, DateOnly date, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == eventId)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsClosed || ev.IsCancelled)
            throw new ConflictException("Cannot add days to a closed or cancelled event.");

        var dupe = await db.EventDays.AnyAsync(x => x.EventId == eventId && x.Date == date);
        if (dupe)
            throw new ConflictException("That date is already on the event.");

        var day = EventDay.Create(eventId, date);
        db.EventDays.Add(day);
        await db.SaveChangesAsync();
        return new Result(day.Id);
    }
}
