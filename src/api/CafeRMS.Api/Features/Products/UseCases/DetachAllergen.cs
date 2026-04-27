using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class DetachAllergenController : ControllerBase
{
    [HttpDelete("/api/products/{id:guid}/allergens/{allergenId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid allergenId,
        [FromServices] AppDbContext db)
    {
        await DetachAllergen.Execute(id, allergenId, db);
        return NoContent();
    }
}


public static class DetachAllergen
{
    public static async Task Execute(Guid productId, Guid allergenId, AppDbContext db)
    {
        var link = await db.ProductAllergens.FirstOrDefaultAsync(x => x.ProductId == productId && x.AllergenId == allergenId)
            ?? throw new NotFoundException("Allergen is not attached to this product.");

        db.ProductAllergens.Remove(link);
        await db.SaveChangesAsync();
    }
}
