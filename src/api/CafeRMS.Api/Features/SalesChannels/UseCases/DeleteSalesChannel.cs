using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

public static class DeleteSalesChannel
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var channel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Sales channel not found.");

        // Phase 2 join-table cleanup: drop the SalesChannel ↔ PriceGroup links — they
        // have no meaning once the channel is gone.
        var links = await db.SalesChannelPriceGroups.Where(x => x.SalesChannelId == id).ToListAsync();
        db.SalesChannelPriceGroups.RemoveRange(links);

        db.SalesChannels.Remove(channel);
        await db.SaveChangesAsync();
    }
}
