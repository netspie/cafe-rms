using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.SalesChannels.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.SalesChannels.Controllers;

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
