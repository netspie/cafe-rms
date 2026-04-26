using CafeRMS.Api.Features.Allergens.UseCases;
using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Allergens.Controllers;

[ApiController]
public sealed class DeleteAllergenController : ControllerBase
{
    [HttpDelete("/api/allergens/{id:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteAllergen.Execute(id, db);
        return NoContent();
    }
}
