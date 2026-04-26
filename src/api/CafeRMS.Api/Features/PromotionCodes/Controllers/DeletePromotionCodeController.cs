using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.PromotionCodes.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.PromotionCodes.Controllers;

[ApiController]
public sealed class DeletePromotionCodeController : ControllerBase
{
    [HttpDelete("/api/promotion-codes/{id:guid}")]
    [Authorize(Policy = Permissions.PromotionsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeletePromotionCode.Execute(id, db);
        return NoContent();
    }
}
