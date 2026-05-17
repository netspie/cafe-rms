using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.SalesChannels.UseCases;

[ApiController]
public sealed class DeleteSalesChannelController : ControllerBase
{
    [HttpDelete("/api/sales-channels/{id:guid}")]
    [Authorize(Policy = Permissions.SalesChannelsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteSalesChannel.Execute(id, db);
        return NoContent();
    }
}


public static class DeleteSalesChannel
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var channel = await db.SalesChannels.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Sales channel not found.");

        var links = await db.SalesChannelPriceGroups.Where(x => x.SalesChannelId == id).ToListAsync();
        db.SalesChannelPriceGroups.RemoveRange(links);

        db.SalesChannels.Remove(channel);
        await db.SaveChangesAsync();
    }
}
