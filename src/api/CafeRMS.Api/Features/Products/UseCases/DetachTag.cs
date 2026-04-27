using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

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


public static class DetachTag
{
    public static async Task Execute(Guid productId, Guid tagId, AppDbContext db)
    {
        var link = await db.ProductTags.FirstOrDefaultAsync(x => x.ProductId == productId && x.TagId == tagId)
            ?? throw new NotFoundException("Tag is not attached to this product.");

        db.ProductTags.Remove(link);
        await db.SaveChangesAsync();
    }
}
