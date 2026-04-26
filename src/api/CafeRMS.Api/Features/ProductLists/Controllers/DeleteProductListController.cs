using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Features.ProductLists.UseCases;
using CafeRMS.Api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeRMS.Api.Features.ProductLists.Controllers;

[ApiController]
public sealed class DeleteProductListController : ControllerBase
{
    [HttpDelete("/api/product-lists/{id:guid}")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
    {
        await DeleteProductList.Execute(id, db);
        return NoContent();
    }
}
