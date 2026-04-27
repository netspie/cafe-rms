using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class UnlinkPriceGroupController : ControllerBase
{
    [HttpDelete("/api/sales-channels/{id:guid}/price-groups/{priceGroupId:guid}")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid priceGroupId,
        [FromServices] AppDbContext db)
    {
        await UnlinkPriceGroup.Execute(id, priceGroupId, db);
        return NoContent();
    }
}


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
