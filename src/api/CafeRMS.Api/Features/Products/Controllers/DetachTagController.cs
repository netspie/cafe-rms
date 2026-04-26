using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class DetachTagController : ControllerBase
{
    [HttpDelete("/api/products/{id:guid}/tags/{tagId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid tagId,
        [FromServices] AppDbContext db)
    {
        await DetachTag.Execute(id, tagId, db);
        return NoContent();
    }
}
