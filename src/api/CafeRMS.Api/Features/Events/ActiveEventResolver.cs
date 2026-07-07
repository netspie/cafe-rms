using CafeRMS.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Events;

public sealed record ActiveEventInfo(Guid Id, Guid PriceGroupId, List<Guid> ProductIds);

public static class ActiveEventResolver
{
    public static async Task<ActiveEventInfo?> ResolveAsync(AppDbContext db, DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var ev = await db.Events
            .Where(x => x.PublishedAt != null && x.ClosedAt == null && x.CancelledAt == null)
            .Where(x => db.EventDays.Any(d => d.EventId == x.Id && d.Date == today))
            .OrderBy(x => x.PublishedAt)
            .Select(x => new { x.Id, x.PriceGroupId, x.ProductListId })
            .FirstOrDefaultAsync();

        if (ev is null || ev.PriceGroupId is not Guid priceGroupId || ev.ProductListId is not Guid listId)
            return null;

        var productIds = await db.ProductListItems
            .Where(x => x.ProductListId == listId)
            .Select(x => x.ProductId)
            .ToListAsync();

        return new ActiveEventInfo(ev.Id, priceGroupId, productIds);
    }
}
