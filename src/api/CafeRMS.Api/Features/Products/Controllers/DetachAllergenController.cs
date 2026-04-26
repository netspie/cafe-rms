using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

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
