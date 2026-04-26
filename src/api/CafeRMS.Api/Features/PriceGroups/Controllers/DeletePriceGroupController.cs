using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PriceGroups.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PriceGroups.Controllers;

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
