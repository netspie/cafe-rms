using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

[ApiController]
public sealed class ListMobileEventsController : ControllerBase
{
    [HttpGet("/api/events/mobile")]
    [Authorize]
    public Task<IReadOnlyList<ListMobileEvents.Item>> Handle(
        [FromServices] AppDbContext db) =>
        ListMobileEvents.Execute(db, DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date));
}


public static class ListMobileEvents
{
    public sealed record Item(Guid Id, string Name, string? ImageUrl, DateOnly NextDate, bool IsToday);

    public static async Task<IReadOnlyList<Item>> Execute(AppDbContext db, DateOnly today)
    {
        var events = await db.Events
            .Where(e => e.PublishedAt != null && e.ClosedAt == null && e.CancelledAt == null)
            .Where(e => db.EventDays.Any(d => d.EventId == e.Id && d.Date >= today))
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.ImageUrl,
                Days = db.EventDays.Where(d => d.EventId == e.Id).Select(d => d.Date).ToList()
            })
            .ToListAsync();

        return events
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.ImageUrl,
                NextDate = e.Days.Where(d => d >= today).Min(),
                IsToday = e.Days.Contains(today)
            })
            .OrderByDescending(e => e.IsToday)
            .ThenBy(e => e.NextDate)
            .Select(e => new Item(e.Id, e.Name, e.ImageUrl, e.NextDate, e.IsToday))
            .ToList();
    }
}
