using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.Products.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.Products.Controllers;

[ApiController]
public sealed class RemoveProductImageController : ControllerBase
{
    [HttpDelete("/api/products/{id:guid}/images/{imageId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid imageId,
        [FromServices] AppDbContext db)
    {
        await RemoveProductImage.Execute(id, imageId, db);
        return NoContent();
    }
}
