using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.ProductLists.UseCases;

[ApiController]
public sealed class AddProductToListController : ControllerBase
{
    [HttpPost("/api/product-lists/{id:guid}/items")]
    [Authorize(Policy = Permissions.MenusManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromBody] AddProductToListRequest request,
        [FromServices] AppDbContext db)
    {
        await AddProductToList.Execute(id, request.ProductId, db);
        return NoContent();
    }
}

public sealed record AddProductToListRequest(Guid ProductId);

public sealed class AddProductToListValidator : AbstractValidator<AddProductToListRequest>
{
    public AddProductToListValidator()
    {
        RuleFor(x => x.ProductId).NotEqual(Guid.Empty);
    }
}


public static class AddProductToList
{
    public static async Task Execute(Guid productListId, Guid productId, AppDbContext db)
    {
        var listExists = await db.ProductLists.AnyAsync(x => x.Id == productListId);
        if (!listExists)
            throw new NotFoundException("Product list not found.");

        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        // Idempotent: already-in-list is a no-op so the mobile app can retry without error.
        var alreadyAdded = await db.ProductListItems
            .AnyAsync(x => x.ProductListId == productListId && x.ProductId == productId);
        if (alreadyAdded)
            return;

        db.ProductListItems.Add(ProductListItem.Create(productListId, productId));
        await db.SaveChangesAsync();
    }
}
