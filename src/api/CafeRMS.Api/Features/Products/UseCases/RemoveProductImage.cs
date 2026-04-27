using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

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


public static class RemoveProductImage
{
    public static async Task Execute(Guid productId, Guid imageId, AppDbContext db)
    {
        var image = await db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId)
            ?? throw new NotFoundException("Image not found on this product.");

        db.ProductImages.Remove(image);
        await db.SaveChangesAsync();
    }
}
