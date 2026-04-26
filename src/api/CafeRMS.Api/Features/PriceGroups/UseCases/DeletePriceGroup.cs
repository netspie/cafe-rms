using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

public static class DeletePriceGroup
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var priceGroup = await db.PriceGroups.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Price group not found.");

        // Phase 2 join-table cleanup convention: when a parent on one side of a join is
        // (soft-)deleted, the matching join rows are hard-deleted in the same unit of work.
        // Here: SalesChannel ↔ PriceGroup links lose meaning once the PriceGroup is gone.
        // Load-then-RemoveRange (not ExecuteDeleteAsync) so the same code works under both
        // PG and the InMemory test provider. Join tables are tiny (per-tenant), so the
        // cost of materializing them is negligible.
        var channelLinks = await db.SalesChannelPriceGroups.Where(x => x.PriceGroupId == id).ToListAsync();
        db.SalesChannelPriceGroups.RemoveRange(channelLinks);

        // ProductPrice isn't a pure join (carries Net), but its meaning evaporates when
        // the PriceGroup goes — global filter would hide the PG, leaving an orphaned price
        // row whose nav resolves to null. Drop them so the price table stays clean.
        var prices = await db.ProductPrices.Where(x => x.PriceGroupId == id).ToListAsync();
        db.ProductPrices.RemoveRange(prices);

        db.PriceGroups.Remove(priceGroup);
        await db.SaveChangesAsync();
    }
}
