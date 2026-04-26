using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

public static class UnlinkPriceGroup
{
    public static async Task Execute(Guid salesChannelId, Guid priceGroupId, AppDbContext db)
    {
        var link = await db.SalesChannelPriceGroups
            .FirstOrDefaultAsync(x => x.SalesChannelId == salesChannelId && x.PriceGroupId == priceGroupId)
            ?? throw new NotFoundException("Link not found.");

        db.SalesChannelPriceGroups.Remove(link);
        await db.SaveChangesAsync();
    }
}
