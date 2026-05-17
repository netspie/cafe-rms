using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PriceGroups.UseCases;

[ApiController]
public sealed class DeletePriceGroupController : ControllerBase
{
    [HttpDelete("/api/price-groups/{id:guid}")]
    [Authorize(Policy = Permissions.PricingManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeletePriceGroup.Execute(id, db);
        return NoContent();
    }
}


public static class DeletePriceGroup
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var priceGroup = await db.PriceGroups.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Price group not found.");

        var channelLinks = await db.SalesChannelPriceGroups.Where(x => x.PriceGroupId == id).ToListAsync();
        db.SalesChannelPriceGroups.RemoveRange(channelLinks);

        var prices = await db.ProductPrices.Where(x => x.PriceGroupId == id).ToListAsync();
        db.ProductPrices.RemoveRange(prices);

        db.PriceGroups.Remove(priceGroup);
        await db.SaveChangesAsync();
    }
}
