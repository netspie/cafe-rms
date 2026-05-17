using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Loyalty;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class CloseEventController : ControllerBase
{
    [HttpPost("/api/events/{id:guid}/close")]
    [Authorize(Policy = Permissions.EventsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await CloseEvent.Execute(id, db, DateTimeOffset.UtcNow);
        return NoContent();
    }
}


public static class CloseEvent
{
    public const int AttendanceBonusPoints = 50;

    public static async Task Execute(Guid id, AppDbContext db, DateTimeOffset now)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Event not found.");

        if (ev.IsCancelled)
            throw new ConflictException("Cannot close a cancelled event.");
        if (ev.IsClosed)
            throw new ConflictException("Event is already closed.");
        if (!ev.IsPublished)
            throw new ConflictException("Event must be published before it can be closed.");

        await using var tx = await db.Database.BeginTransactionAsync();

        ev.Close(now);

        var attendeeUserIds = await db.Orders
            .Where(x => x.EventId == id && x.UserId != null)
            .Select(x => x.UserId!.Value)
            .Distinct()
            .ToListAsync();

        foreach (var userId in attendeeUserIds)
        {
            db.LoyaltyPointLogs.Add(LoyaltyPointLog.Create(
                userId,
                AttendanceBonusPoints,
                reason: $"{LoyaltyReasons.AttendancePrefix}{ev.Id}"));
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
