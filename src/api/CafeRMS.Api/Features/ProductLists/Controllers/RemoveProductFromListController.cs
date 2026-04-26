using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

[ApiController]
public sealed class RemoveProductFromListController : ControllerBase
{
    [HttpDelete("/api/product-lists/{id:guid}/items/{productId:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid productId,
        [FromServices] AppDbContext db)
    {
        await RemoveProductFromList.Execute(id, productId, db);
        return NoContent();
    }
}
