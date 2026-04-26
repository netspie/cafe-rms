using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

public static class LinkPriceGroup
{
    public static async Task Execute(Guid salesChannelId, Guid priceGroupId, AppDbContext db)
    {
        var channelExists = await db.SalesChannels.AnyAsync(x => x.Id == salesChannelId);
        if (!channelExists)
            throw new NotFoundException("Sales channel not found.");

        var priceGroupExists = await db.PriceGroups.AnyAsync(x => x.Id == priceGroupId);
        if (!priceGroupExists)
            throw new NotFoundException("Price group not found.");

        // Idempotent: re-linking is a no-op so callers can retry without 409s.
        var alreadyLinked = await db.SalesChannelPriceGroups
            .AnyAsync(x => x.SalesChannelId == salesChannelId && x.PriceGroupId == priceGroupId);
        if (alreadyLinked)
            return;

        db.SalesChannelPriceGroups.Add(SalesChannelPriceGroup.Create(salesChannelId, priceGroupId));
        await db.SaveChangesAsync();
    }
}
