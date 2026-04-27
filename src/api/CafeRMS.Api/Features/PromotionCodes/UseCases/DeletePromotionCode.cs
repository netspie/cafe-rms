using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.PromotionCodes.UseCases;

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


public static class DeletePromotionCode
{
    public static async Task Execute(Guid id, AppDbContext db)
    {
        var promo = await db.PromotionCodes.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException("Promotion code not found.");

        db.PromotionCodes.Remove(promo);
        await db.SaveChangesAsync();
    }
}
