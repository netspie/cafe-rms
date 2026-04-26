using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

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
