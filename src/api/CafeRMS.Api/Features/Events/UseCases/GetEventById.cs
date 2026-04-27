using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events.UseCases;

public static class GetEventById
{
    public sealed record Result(
        Guid Id,
        string Name,
        string? Description,
        string? ImageUrl,
        Guid? ProductListId,
        Guid? PriceGroupId,
        EventStatus Status,
        DateTimeOffset? PublishedAt,
        DateTimeOffset? ClosedAt,
        DateTimeOffset? CancelledAt,
        string? CancellationReason,
        IReadOnlyList<DayInfo> Days,
        DateTimeOffset CreatedAt);

    public sealed record DayInfo(Guid Id, DateOnly Date);

    public static async Task<Result> Execute(Guid id, AppDbContext db)
    {
        var ev = await db.Events.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Event not found.");

        var days = await db.EventDays
            .Where(x => x.EventId == id)
            .OrderBy(x => x.Date)
            .Select(x => new DayInfo(x.Id, x.Date))
            .ToListAsync();

        return new Result(ev.Id, ev.Name, ev.Description, ev.ImageUrl, ev.ProductListId, ev.PriceGroupId,
            ev.Status, ev.PublishedAt, ev.ClosedAt, ev.CancelledAt, ev.CancellationReason,
            days, ev.CreatedAt);
    }
}
