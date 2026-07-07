using CafeRMS.Api.Features.Auth;
using CafeRMS.Api.Persistence;
using CafeRMS.Api.Shared.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeRMS.Api.Features.Products.UseCases;

[ApiController]
public sealed class SetProductPriceController : ControllerBase
{
    [HttpPut("/api/products/{id:guid}/prices/{priceGroupId:guid}")]
    [Authorize(Policy = Permissions.ProductsManage)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid id,
        [FromRoute] Guid priceGroupId,
        [FromBody] SetProductPriceRequest request,
        [FromServices] AppDbContext db)
    {
        await SetProductPrice.Execute(id, priceGroupId, request.Gross, db);
        return NoContent();
    }
}

public sealed record SetProductPriceRequest(decimal Gross);

public sealed class SetProductPriceValidator : AbstractValidator<SetProductPriceRequest>
{
    public SetProductPriceValidator()
    {
        RuleFor(x => x.Gross).GreaterThanOrEqualTo(0m);
    }
}


public static class SetProductPrice
{
    public static async Task Execute(Guid productId, Guid priceGroupId, decimal gross, AppDbContext db)
    {
        var productExists = await db.Products.AnyAsync(x => x.Id == productId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        var priceGroupExists = await db.PriceGroups.AnyAsync(x => x.Id == priceGroupId);
        if (!priceGroupExists)
            throw new NotFoundException("Price group not found.");

        var existing = await db.ProductPrices.FirstOrDefaultAsync(x => x.ProductId == productId && x.PriceGroupId == priceGroupId);
        if (existing is not null)
        {
            db.ProductPrices.Remove(existing);
            db.ProductPrices.Add(ProductPrice.Create(productId, priceGroupId, gross));
        }
        else
        {
            db.ProductPrices.Add(ProductPrice.Create(productId, priceGroupId, gross));
        }

        await db.SaveChangesAsync();
    }
}
