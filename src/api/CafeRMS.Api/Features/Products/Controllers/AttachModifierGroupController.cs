using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class AttachModifierGroupController : ControllerBase
{
    [HttpPost("/api/products/{id:guid}/modifier-groups/{modifierGroupId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid modifierGroupId,
        [FromServices] AppDbContext db)
    {
        await AttachModifierGroup.Execute(id, modifierGroupId, db);
        return NoContent();
    }
}
