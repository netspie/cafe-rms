using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class AttachTagController : ControllerBase
{
    [HttpPost("/api/products/{id:guid}/tags/{tagId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid tagId,
        [FromServices] AppDbContext db)
    {
        await AttachTag.Execute(id, tagId, db);
        return NoContent();
    }
}


public static class AttachTag
{
    public static async Task Execute(Guid productId, Guid tagId, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var tagExists = await db.Tags.AnyAsync(x => x.Id == tagId);
        if (!tagExists)
            throw new NotFoundException("Tag not found.");

        var alreadyAttached = await db.ProductTags.AnyAsync(x => x.ProductId == productId && x.TagId == tagId);
        if (alreadyAttached)
            return;

        db.ProductTags.Add(ProductTag.Create(productId, tagId));
        await db.SaveChangesAsync();
    }
}
