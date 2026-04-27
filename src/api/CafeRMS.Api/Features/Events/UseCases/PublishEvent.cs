using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class PublishEventController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/publish")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await PublishEvent.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}


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
