using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class RemoveEventDayController : ControllerBase
{
    [HttpDelete("/api/events/{id:guid}/days/{dayId:guid}")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid dayId,
        [FromServices] AppDbContext db)
    {
        await RemoveEventDay.Execute(id, dayId, db);
        return NoContent();
    }
}


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
